using Microsoft.EntityFrameworkCore;
using ReserveFlow.Inventory.Application.Inventories;
using ReserveFlow.Inventory.Domain.Inventories;

namespace ReserveFlow.Inventory.Infrastructure.Persistence.Repositories
{
    internal sealed class RoomInventoryRepository(InventoryDbContext inventoryDbContext) : IRoomInventoryRepository
    {
        public async Task AddAsync(RoomInventory roomInventory, CancellationToken cancellationToken)
        {
            await inventoryDbContext.RoomInventories.AddAsync(roomInventory, cancellationToken);
        }

        public async Task<RoomInventory?> GetByRoomTypeAndDateAsync(Guid roomTypeId, DateOnly date, CancellationToken cancellationToken)
        {
            return await inventoryDbContext.RoomInventories.FirstOrDefaultAsync(ri => ri.RoomTypeId == roomTypeId && ri.Date == date, cancellationToken);
        }

        public async Task<IReadOnlyCollection<RoomInventory>> GetByRoomTypeAndDateRangeAsync(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, CancellationToken cancellationToken)
        {
            return await inventoryDbContext.RoomInventories.Where(ri => ri.RoomTypeId == roomTypeId && ri.Date >= checkIn && ri.Date < checkOut).ToListAsync(cancellationToken);
        }
    }
}
