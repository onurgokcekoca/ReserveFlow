using Microsoft.EntityFrameworkCore;
using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Booking.Application.Exceptions;
using ReserveFlow.Booking.Domain.Bookings;
using ReserveFlow.Booking.Infrastructure.Outbox;

namespace ReserveFlow.Booking.Infrastructure.Persistence
{
    public sealed class BookingDbContext : DbContext, IBookingUnitOfWork
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {
        }

        public DbSet<BookingHold> BookingHolds => Set<BookingHold>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new BookingHoldConcurrencyException("Booking hold was modified by another transaction.", ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("booking");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
