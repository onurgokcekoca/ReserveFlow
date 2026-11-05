using ReserveFlow.Inventory.Domain.Inventories;

namespace ReserveFlow.Inventory.Application.Inventories
{
    public interface IRoomInventoryRepository
    {
        Task AddAsync(RoomInventory roomInventory, CancellationToken cancellationToken);

        Task<RoomInventory?> GetByRoomTypeAndDateAsync(Guid roomTypeId, DateOnly date, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<RoomInventory>> GetByRoomTypeAndDateRangeAsync(
            Guid roomTypeId,
            DateOnly checkIn,
            DateOnly checkOut,
            CancellationToken cancellationToken);
    }
}
