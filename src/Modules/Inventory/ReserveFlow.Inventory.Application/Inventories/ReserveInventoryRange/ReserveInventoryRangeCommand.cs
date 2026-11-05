using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.ReserveInventoryRange
{
    public sealed record ReserveInventoryRangeCommand(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms) : ICommand<AppResult>;
}
