namespace ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;

public interface IRabbitMqPublisher
{
    Task PublishAsync(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        string messageId,
        IReadOnlyDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default);
}
