using RabbitMQ.Client;
using ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;

namespace ReserveFlow.Catalog.Infrastructure.Messaging
{
    internal sealed class CatalogMessagingInitializer
    {
        private readonly IRabbitMqConnectionProvider _connectionProvider;

        public CatalogMessagingInitializer(IRabbitMqConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

            await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: CatalogMessagingTopology.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);
        }
    }
}
