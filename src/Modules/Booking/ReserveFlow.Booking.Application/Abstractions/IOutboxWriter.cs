namespace ReserveFlow.Booking.Application.Abstractions
{
    public interface IOutboxWriter
    {
        Task WriteAsync<T>(string type, T payload, DateTime occurredAtUtc, CancellationToken cancellationToken);
    }
}
