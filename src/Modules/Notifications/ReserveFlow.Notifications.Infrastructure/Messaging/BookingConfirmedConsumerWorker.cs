using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReserveFlow.Notifications.Infrastructure.Messaging.Contracts;
using ReserveFlow.Notifications.Infrastructure.Persistence;
using ReserveFlow.Notifications.Infrastructure.Persistence.Entities;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;
using System.Text.Json;

namespace ReserveFlow.Notifications.Infrastructure.Messaging;

internal sealed partial class BookingConfirmedConsumerWorker(
    IRabbitMqConnectionProvider connectionProvider,
    IRabbitMqPublisher publisher,
    BookingConfirmedTopology topology,
    IOptions<NotificationsMessagingOptions> options,
    ILogger<BookingConfirmedConsumerWorker> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private const string RetryCountHeader = "x-retry-count";

    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

    private readonly NotificationsMessagingOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunConsumerAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogConsumerConnectionFailed(logger, exception);

                try
                {
                    await Task.Delay(ReconnectDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private static int GetRetryCount(BasicDeliverEventArgs eventArgs)
    {
        if (eventArgs.BasicProperties.Headers is null)
        {
            return 0;
        }

        if (!eventArgs.BasicProperties.Headers.TryGetValue(RetryCountHeader, out object? value))
        {
            return 0;
        }

        return value switch
        {
            byte byteValue => byteValue,

            short shortValue => shortValue,

            int intValue => intValue,

            long longValue => checked((int)longValue),

            _ => 0
        };
    }

    private async Task RunConsumerAsync(CancellationToken cancellationToken)
    {
        await topology.EnsureAsync(cancellationToken);

        IConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            await HandleMessageAsync(
                channel,
                eventArgs,
                cancellationToken);
        };

        await channel.BasicConsumeAsync(
            queue: _options.BookingConfirmedQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        LogConsumerStarted(logger, _options.BookingConfirmedQueueName);

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = scopeFactory.CreateScope();

            NotificationsDbContext dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

            IntegrationEventEnvelope envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(eventArgs.Body.Span)
                ?? throw new JsonException("Integration event envelope could not be deserialized.");

            if (!string.Equals(
                envelope.Type,
                BookingConfirmedTopology.EventType,
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Unexpected integration event type '{envelope.Type}'.");
            }

            bool alreadyProcessed = await dbContext.InboxMessages.AnyAsync(x => x.MessageId == envelope.MessageId, cancellationToken);

            if (alreadyProcessed)
            {
                LogMessageAlreadyProcessed(logger, envelope.MessageId);

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);

                return;
            }

            BookingConfirmedPayload payload = envelope.Payload.Deserialize<BookingConfirmedPayload>()
                ?? throw new JsonException("BookingConfirmed payload could not be deserialized.");

            BookingConfirmationNotification notification = BookingConfirmationNotification.Create(
                    envelope.MessageId,
                    payload.BookingId,
                    payload.PropertyId,
                    envelope.OccurredAtUtc);

            InboxMessage inboxMessage = InboxMessage.Create(
                    envelope.MessageId,
                    envelope.Type,
                    DateTime.UtcNow);

            dbContext.BookingConfirmationNotifications.Add(notification);

            dbContext.InboxMessages.Add(inboxMessage);

            await dbContext.SaveChangesAsync(cancellationToken);

            LogBookingConfirmedReceived(
                logger,
                envelope.MessageId,
                payload.BookingId,
                payload.PropertyId);

            await channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogMessageProcessingFailed(
                logger,
                exception,
                eventArgs.DeliveryTag);

            await HandleFailedMessageAsync(
                channel,
                eventArgs,
                cancellationToken);
        }
    }

    private async Task HandleFailedMessageAsync(IChannel channel, BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        int retryCount = GetRetryCount(eventArgs);

        try
        {
            Dictionary<string, object?> headers = eventArgs.BasicProperties.Headers is null
                    ? new Dictionary<string, object?>()
                    : new Dictionary<string, object?>(eventArgs.BasicProperties.Headers);

            if (retryCount < _options.MaxRetryCount)
            {
                int nextRetryCount = retryCount + 1;

                headers[RetryCountHeader] = nextRetryCount;

                await publisher.PublishAsync(
                    BookingConfirmedTopology.ExchangeName,
                    BookingConfirmedTopology.RetryRoutingKey,
                    eventArgs.Body,
                    eventArgs.BasicProperties.MessageId ?? Guid.NewGuid().ToString(),
                    headers,
                    cancellationToken);

                LogMessageScheduledForRetry(
                    logger,
                    eventArgs.DeliveryTag,
                    nextRetryCount);
            }
            else
            {
                await publisher.PublishAsync(
                    BookingConfirmedTopology.ExchangeName,
                    BookingConfirmedTopology.DeadRoutingKey,
                    eventArgs.Body,
                    eventArgs.BasicProperties.MessageId ?? Guid.NewGuid().ToString(),
                    headers,
                    cancellationToken);

                LogMessageMovedToDeadLetter(logger, eventArgs.DeliveryTag);
            }

            await channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogRetryPublishFailed(
                logger,
                exception,
                eventArgs.DeliveryTag);

            await channel.BasicNackAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: true,
                cancellationToken: cancellationToken);
        }
    }

    #region LoggerMessages

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "RabbitMQ consumer started listening to queue {QueueName}.")]
    private static partial void LogConsumerStarted(
        ILogger logger,
        string queueName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Received BookingConfirmed message {MessageId}. BookingId: {BookingId}, PropertyId: {PropertyId}.")]
    private static partial void LogBookingConfirmedReceived(
        ILogger logger,
        Guid messageId,
        Guid bookingId,
        Guid propertyId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Failed to process RabbitMQ message with delivery tag {DeliveryTag}.")]
    private static partial void LogMessageProcessingFailed(
        ILogger logger,
        Exception exception,
        ulong deliveryTag);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "RabbitMQ consumer could not connect. It will retry.")]
    private static partial void LogConsumerConnectionFailed(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "BookingConfirmed message {MessageId} was already processed. Acknowledging duplicate.")]
    private static partial void LogMessageAlreadyProcessed(
        ILogger logger,
        Guid messageId);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "RabbitMQ message with delivery tag {DeliveryTag} was scheduled for retry {RetryCount}.")]
    private static partial void LogMessageScheduledForRetry(
        ILogger logger,
        ulong deliveryTag,
        int retryCount);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Warning,
        Message = "RabbitMQ message with delivery tag {DeliveryTag} exceeded the retry limit and was moved to the dead-letter queue.")]
    private static partial void LogMessageMovedToDeadLetter(
        ILogger logger,
        ulong deliveryTag);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "Failed to route RabbitMQ message with delivery tag {DeliveryTag} to retry or dead-letter queue.")]
    private static partial void LogRetryPublishFailed(
        ILogger logger,
        Exception exception,
        ulong deliveryTag);

    #endregion LoggerMessages
}
