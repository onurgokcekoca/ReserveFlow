using Microsoft.EntityFrameworkCore;
using ReserveFlow.Inventory.Application.Inventories;
using ReserveFlow.Inventory.Domain.Inventories;

namespace ReserveFlow.Inventory.Infrastructure.Persistence.Repositories
{
    internal sealed class RoomInventoryReadRepository(InventoryDbContext inventoryDbContext) : IRoomInventoryReadRepository
    {
        public async Task<List<RoomInventory>> GetByRoomTypeAndDateRangeAsync(
             Guid roomTypeId,
             DateOnly checkIn,
             DateOnly checkOut,
             CancellationToken cancellationToken)
        {
            return await inventoryDbContext.RoomInventories
                .Where(ri => ri.RoomTypeId == roomTypeId && ri.Date >= checkIn && ri.Date < checkOut)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
