using Microsoft.EntityFrameworkCore;
using ReserveFlow.Booking.Application.Bookings;
using ReserveFlow.Booking.Domain.Bookings;

namespace ReserveFlow.Booking.Infrastructure.Persistence.Repositories
{
    public class BookingRepository(BookingDbContext bookingDbContext) : IBookingRepository
    {
        public async Task AddAsync(BookingHold booking, CancellationToken cancellationToken)
        {
            await bookingDbContext.AddAsync(booking, cancellationToken);
        }

        public async Task<BookingHold?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            return await bookingDbContext.BookingHolds.FirstOrDefaultAsync(bh => bh.Id == bookingId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Guid>> GetExpiredActiveHoldIdsAsync(DateTime currentUtc, CancellationToken cancellationToken)
        {
            return await bookingDbContext.BookingHolds
                .AsNoTracking()
                .Where(bh => bh.Status == StatusType.Active && bh.ExpiresAtUtc <= currentUtc)
                .Select(bh => bh.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
