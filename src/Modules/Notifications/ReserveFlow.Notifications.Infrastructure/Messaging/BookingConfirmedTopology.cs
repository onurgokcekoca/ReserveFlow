using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;

namespace ReserveFlow.Notifications.Infrastructure.Messaging;

internal sealed class BookingConfirmedTopology
{
    public const string ExchangeName = "booking.events";

    public const string EventType = "BookingConfirmed";

    public const string RoutingKey = "booking.confirmed";

    public const string RetryRoutingKey = "booking.confirmed.retry";

    public const string DeadRoutingKey = "booking.confirmed.dead";

    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly NotificationsMessagingOptions _options;

    public BookingConfirmedTopology(IRabbitMqConnectionProvider connectionProvider, IOptions<NotificationsMessagingOptions> options)
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
            queue: _options.BookingConfirmedQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.BookingConfirmedQueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey,
            cancellationToken: cancellationToken);

        var retryArguments =
            new Dictionary<string, object?>
            {
                ["x-message-ttl"] = _options.RetryDelayMilliseconds,

                ["x-dead-letter-exchange"] = ExchangeName,

                ["x-dead-letter-routing-key"] = RoutingKey
            };

        await channel.QueueDeclareAsync(
            queue: _options.BookingConfirmedRetryQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryArguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.BookingConfirmedRetryQueueName,
            exchange: ExchangeName,
            routingKey: RetryRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.BookingConfirmedDeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.BookingConfirmedDeadLetterQueueName,
            exchange: ExchangeName,
            routingKey: DeadRoutingKey,
            cancellationToken: cancellationToken);
    }
}
