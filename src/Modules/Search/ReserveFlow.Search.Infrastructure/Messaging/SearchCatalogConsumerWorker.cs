using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReserveFlow.Catalog.Application.Properties.IntegrationEvents;
using ReserveFlow.Search.Application.Common;
using ReserveFlow.Search.Application.Common.Models;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;
using System.Text.Json;

namespace ReserveFlow.Search.Infrastructure.Messaging
{
    internal sealed partial class SearchCatalogConsumerWorker(
        IRabbitMqConnectionProvider connectionProvider,
        IRabbitMqPublisher publisher,
        SearchMessagingTopology topology,
        IOptions<SearchMessagingOptions> options,
        ILogger<SearchCatalogConsumerWorker> logger,
        IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private const string RetryCountHeader = "x-retry-count";

        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

        private readonly SearchMessagingOptions _options = options.Value;

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
                queue: SearchMessagingTopology.MainQueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            LogConsumerStarted(logger, SearchMessagingTopology.MainQueueName);

            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();

                IPropertySearchRepository searchRepository = scope.ServiceProvider.GetRequiredService<IPropertySearchRepository>();

                IntegrationEventEnvelope envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(eventArgs.Body.Span)
                    ?? throw new JsonException("Integration event envelope could not be deserialized.");

                if (envelope.Type != CatalogIntegrationEventTypes.PropertyCatalogChanged)
                {
                    throw new InvalidOperationException($"Unsupported integration event type '{envelope.Type}'.");
                }

                PropertyCatalogChangedPayload payload = JsonSerializer.Deserialize<PropertyCatalogChangedPayload>(envelope.Payload.GetRawText())
                    ?? throw new JsonException("PropertyCatalogChanged payload could not be deserialized.");

                LogPropertyCatalogChangedReceived(
                    logger,
                    envelope.MessageId,
                    payload.PropertyId);

                var propertySearchDocument = new PropertySearchDocument
                {
                    PropertyId = payload.PropertyId,
                    Name = payload.Name,
                    Description = payload.Description ?? string.Empty,
                    Country = payload.Country,
                    City = payload.City,
                    AddressLine = payload.AddressLine,
                    PropertyType = payload.PropertyType,
                    ProviderId = payload.ProviderId,

                    MaxGuests = payload.RoomTypes.Count == 0 ? 0 : payload.RoomTypes.Max(r => r.Capacity),

                    RoomTypeIds = [.. payload.RoomTypes.Select(r => r.RoomTypeId)],

                    RoomTypeNames = [.. payload.RoomTypes.Select(r => r.Name)]
                };

                await searchRepository.IndexAsync(propertySearchDocument, cancellationToken);

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
                        SearchMessagingTopology.ExchangeName,
                        SearchMessagingTopology.RetryRoutingKey,
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
                        SearchMessagingTopology.ExchangeName,
                        SearchMessagingTopology.DLQRoutingKey,
                        eventArgs.Body,
                        eventArgs.BasicProperties.MessageId ?? Guid.NewGuid().ToString(),
                        headers,
                        cancellationToken);

                    LogMessageMovedToDeadLetter(
                        logger,
                        eventArgs.DeliveryTag);
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
            Message = "Received PropertyCatalogChanged message {MessageId}. PropertyId: {PropertyId}.")]
        private static partial void LogPropertyCatalogChangedReceived(
            ILogger logger,
            Guid messageId,
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
            Message = "PropertyCatalogChanged message {MessageId} was already processed. Acknowledging duplicate.")]
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
}
