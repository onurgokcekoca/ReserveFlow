using ReserveFlow.Inventory.Domain.Inventories;

namespace ReserveFlow.Inventory.Application.Inventories
{
    public interface IRoomInventoryReadRepository
    {
        Task<List<RoomInventory>> GetByRoomTypeAndDateRangeAsync(
            Guid roomTypeId,
            DateOnly checkIn,
            DateOnly checkOut,
            CancellationToken cancellationToken);
    }
}
