using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Booking.Infrastructure.Persistence;
using System.Text.Json;

namespace ReserveFlow.Booking.Infrastructure.Outbox;

public sealed class OutboxWriter(BookingDbContext bookingDbContext) : IOutboxWriter
{
    public async Task WriteAsync<T>(
        string type,
        T payload,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
    {
        string payloadJson = JsonSerializer.Serialize(payload);

        var message = OutboxMessage.Create(type, payloadJson, occurredAtUtc);

        await bookingDbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
