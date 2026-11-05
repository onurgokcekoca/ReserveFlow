using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.CreateInventoryRange
{
    public sealed record CreateInventoryRangeCommand(Guid roomTypeId, DateOnly startDate, DateOnly endDate, int totalRooms) : ICommand<AppResult<Guid>>;
}
