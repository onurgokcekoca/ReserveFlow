using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;

namespace ReserveFlow.Search.Infrastructure.Messaging
{
    internal sealed class SearchMessagingTopology
    {
        public const string ExchangeName = "catalog.events";
        public const string MainQueueName = "search-catalog-projection";
        public const string MainRoutingKey = "property.catalog.changed";
        public const string RetryQueueuName = "search-catalog-projection-retry";
        public const string RetryRoutingKey = "property.catalog.changed.retry";
        public const string DLQ = "search-catalog-projection-dead-letter";
        public const string DLQRoutingKey = "property.catalog.changed.dead";

        private readonly IRabbitMqConnectionProvider _connectionProvider;
        private readonly SearchMessagingOptions _options;

        public SearchMessagingTopology(IRabbitMqConnectionProvider connectionProvider, IOptions<SearchMessagingOptions> options)
        {
            _connectionProvider = connectionProvider;
            _options = options.Value;
        }

        public async Task EnsureAsync(CancellationToken cancellationToken = default)
        {
            IConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

            await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: MainQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: MainQueueName,
                exchange: ExchangeName,
                routingKey: MainRoutingKey,
                cancellationToken: cancellationToken);

            var retryArguments = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = _options.RetryDelayMilliseconds,

                ["x-dead-letter-exchange"] = ExchangeName,

                ["x-dead-letter-routing-key"] = MainRoutingKey
            };

            await channel.QueueDeclareAsync(
                queue: RetryQueueuName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: RetryQueueuName,
                exchange: ExchangeName,
                routingKey: RetryRoutingKey,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: DLQ,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: DLQ,
                exchange: ExchangeName,
                routingKey: DLQRoutingKey,
                cancellationToken: cancellationToken);
        }
    }
}
