using Microsoft.EntityFrameworkCore;
using ReserveFlow.Inventory.Application.Inventories.ReadServices;

namespace ReserveFlow.Inventory.Infrastructure.Persistence.ReadServices
{
    internal sealed class RoomAvailabilityReadService(InventoryDbContext dbContext) : IRoomAvailabilityReadService
    {
        public async Task<IReadOnlyCollection<RoomTypeAvailabilityReadModel>>
            GetAvailabilityAsync(
                IReadOnlyCollection<Guid> roomTypeIds,
                DateOnly checkIn,
                DateOnly checkOut,
                CancellationToken cancellationToken)
        {
            var ids = roomTypeIds.Distinct().ToArray();

            var inventories = await dbContext.RoomInventories
                .AsNoTracking()
                .Where(ri =>
                    ids.Contains(ri.RoomTypeId) &&
                    ri.Date >= checkIn &&
                    ri.Date < checkOut)
                .ToListAsync(cancellationToken);

            var expectedNights = checkOut.DayNumber - checkIn.DayNumber;

            var result = new List<RoomTypeAvailabilityReadModel>();

            foreach (var roomTypeId in ids)
            {
                var roomInventories = inventories.Where(x => x.RoomTypeId == roomTypeId).ToList();

                var hasCompleteInventory = roomInventories.Count == expectedNights;

                var maxAvailableRooms = hasCompleteInventory ? roomInventories.Min(x => x.AvailableRooms) : 0;

                result.Add(new RoomTypeAvailabilityReadModel(
                        roomTypeId,
                        hasCompleteInventory,
                        maxAvailableRooms));
            }

            return result;
        }
    }
}
