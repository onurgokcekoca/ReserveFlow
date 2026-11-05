using ReserveFlow.Catalog.Application.Abstractions;
using ReserveFlow.Catalog.Infrastructure.Persistence;
using System.Text.Json;

namespace ReserveFlow.Catalog.Infrastructure.Outbox
{
    public class OutboxWriter(CatalogDbContext catalogDbContext) : IOutboxWriter
    {
        public async Task WriteAsync<T>(string type, T payload, DateTime occurredAtUtc, CancellationToken cancellationToken)
        {
            string payloadJson = JsonSerializer.Serialize(payload);
            var message = OutboxMessage.Create(type, payloadJson, occurredAtUtc);

            await catalogDbContext.OutboxMessages.AddAsync(message, cancellationToken);
        }
    }
}
