using RabbitMQ.Client;

namespace ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;

internal sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly SemaphoreSlim _publishLock = new(1, 1);

    private IChannel? _channel;

    public RabbitMqPublisher(IRabbitMqConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task PublishAsync(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        string messageId,
        IReadOnlyDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default)
    {
        await _publishLock.WaitAsync(cancellationToken);

        try
        {
            IChannel channel = await GetChannelAsync(cancellationToken);

            var properties = new BasicProperties
            {
                MessageId = messageId,
                ContentType = "application/json",
                Persistent = true,
                Headers = headers is null
                    ? null
                    : new Dictionary<string, object?>(headers)
            };

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        catch
        {
            await ResetChannelAsync();
            throw;
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ResetChannelAsync();
        _publishLock.Dispose();
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await ResetChannelAsync();

        IConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        var options = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        _channel = await connection.CreateChannelAsync(options, cancellationToken);

        return _channel;
    }

    private async Task ResetChannelAsync()
    {
        if (_channel is null)
        {
            return;
        }

        await _channel.DisposeAsync();
        _channel = null;
    }
}
