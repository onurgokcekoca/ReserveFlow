using ReserveFlow.Booking.Domain.Bookings;

namespace ReserveFlow.Booking.Application.Bookings
{
    public interface IBookingRepository
    {
        Task AddAsync(BookingHold booking, CancellationToken cancellationToken);

        Task<BookingHold?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<Guid>> GetExpiredActiveHoldIdsAsync(DateTime currentUtc, CancellationToken cancellationToken);
    }
}
