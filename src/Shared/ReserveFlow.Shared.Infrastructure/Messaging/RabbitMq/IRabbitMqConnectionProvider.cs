using RabbitMQ.Client;

namespace ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
