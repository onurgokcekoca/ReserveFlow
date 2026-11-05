using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReserveFlow.Catalog.Infrastructure.Messaging;
using ReserveFlow.Catalog.Infrastructure.Persistence;
using ReserveFlow.Shared.Application.Messaging;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace ReserveFlow.Catalog.Infrastructure.Outbox
{
    internal sealed partial class OutboxPublisherWorker : BackgroundService
    {
        private const int BatchSize = 20;

        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherWorker> _logger;

        public OutboxPublisherWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingMessagesAsync(stoppingToken);

                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    LogOutboxPublishingFailed(_logger, exception);

                    try
                    {
                        await Task.Delay(PollInterval, stoppingToken);
                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }

        private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

            CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            RabbitMqOutboxPublisher publisher = scope.ServiceProvider.GetRequiredService<RabbitMqOutboxPublisher>();

            var messages = await dbContext.OutboxMessages
                    .Where(x => x.ProcessedAtUtc == null)
                    .OrderBy(x => x.OccurredAtUtc)
                    .Take(BatchSize)
                    .ToListAsync(cancellationToken);

            foreach (OutboxMessage message in messages)
            {
                try
                {
                    using JsonDocument payloadDocument = JsonDocument.Parse(message.Payload);

                    var envelope = new IntegrationEventEnvelope(
                        message.Id,
                        message.Type,
                        message.OccurredAtUtc,
                        payloadDocument.RootElement.Clone());

                    await publisher.PublishAsync(envelope, cancellationToken);

                    message.MarkAsProcessed(DateTime.UtcNow);

                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (DbUpdateConcurrencyException)
                {
                    dbContext.Entry(message).State = EntityState.Detached;

                    LogOutboxMessageAlreadyHandled(_logger, message.Id);
                }
                catch (Exception exception)
                {
                    dbContext.Entry(message).State = EntityState.Detached;

                    LogOutboxMessagePublishFailed(
                        _logger,
                        exception,
                        message.Id);
                }
            }
        }

        #region LoggerMessages

        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "An error occurred while publishing Catalog outbox messages.")]
        private static partial void LogOutboxPublishingFailed(
            ILogger logger,
            Exception exception);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Warning,
            Message = "Catalog outbox message {MessageId} was already handled by another worker.")]
        private static partial void LogOutboxMessageAlreadyHandled(
            ILogger logger,
            Guid messageId);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Error,
            Message = "Failed to publish Catalog outbox message {MessageId}.")]
        private static partial void LogOutboxMessagePublishFailed(
            ILogger logger,
            Exception exception,
            Guid messageId);

        #endregion LoggerMessages
    }
}
