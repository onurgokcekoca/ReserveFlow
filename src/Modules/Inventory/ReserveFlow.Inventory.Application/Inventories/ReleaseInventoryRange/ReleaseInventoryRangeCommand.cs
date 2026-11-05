using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.ReleaseInventoryRange
{
    public sealed record ReleaseInventoryRangeCommand(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms) : ICommand<AppResult>;
}
