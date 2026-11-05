using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Abstractions
{
    public interface IInventoryReservationGateway
    {
        Task<AppResult> ReserveAsync(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, CancellationToken cancellationToken);

        Task<AppResult> ReleaseAsync(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, CancellationToken cancellationToken);
    }
}
