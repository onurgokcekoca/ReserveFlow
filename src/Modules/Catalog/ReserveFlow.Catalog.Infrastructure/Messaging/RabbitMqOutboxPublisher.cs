using ReserveFlow.Catalog.Application.Properties.IntegrationEvents;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;
using System.Text.Json;

namespace ReserveFlow.Catalog.Infrastructure.Messaging
{
    internal sealed class RabbitMqOutboxPublisher
    {
        private readonly IRabbitMqPublisher _publisher;

        public RabbitMqOutboxPublisher(IRabbitMqPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default)
        {
            string routingKey = envelope.Type switch
            {
                CatalogIntegrationEventTypes.PropertyCatalogChanged => CatalogMessagingTopology.PropertyCatalogChangedRoutingKey,

                _ => throw new InvalidOperationException($"Unsupported Catalog outbox message type '{envelope.Type}'.")
            };

            byte[] body = JsonSerializer.SerializeToUtf8Bytes(envelope);

            await _publisher.PublishAsync(
                CatalogMessagingTopology.ExchangeName,
                routingKey,
                body,
                envelope.MessageId.ToString(),
                cancellationToken: cancellationToken);
        }
    }
}
