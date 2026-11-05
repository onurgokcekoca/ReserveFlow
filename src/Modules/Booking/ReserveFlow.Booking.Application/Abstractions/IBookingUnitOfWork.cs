namespace ReserveFlow.Booking.Application.Abstractions
{
    public interface IBookingUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
