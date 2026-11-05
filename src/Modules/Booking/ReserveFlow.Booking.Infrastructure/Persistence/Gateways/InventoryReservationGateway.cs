using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Inventory.Application.Inventories.ReleaseInventoryRange;
using ReserveFlow.Inventory.Application.Inventories.ReserveInventoryRange;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Infrastructure.Persistence.Gateways
{
    internal sealed class InventoryReservationGateway : IInventoryReservationGateway
    {
        private readonly ICommandHandler<ReleaseInventoryRangeCommand, AppResult> _releaseInventoryRangeCommandHandler;
        private readonly ICommandHandler<ReserveInventoryRangeCommand, AppResult> _reserveInventoryRangeCommandHandler;

        public InventoryReservationGateway(
            ICommandHandler<ReleaseInventoryRangeCommand, AppResult> releaseInventoryRangeCommandHandler,
            ICommandHandler<ReserveInventoryRangeCommand, AppResult> reserveInventoryRangeCommandHandler)
        {
            _releaseInventoryRangeCommandHandler = releaseInventoryRangeCommandHandler;
            _reserveInventoryRangeCommandHandler = reserveInventoryRangeCommandHandler;
        }

        public async Task<AppResult> ReleaseAsync(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, CancellationToken cancellationToken)
        {
            var command = new ReleaseInventoryRangeCommand(roomTypeId, checkIn, checkOut, rooms);
            return await _releaseInventoryRangeCommandHandler.HandleAsync(command, cancellationToken);
        }

        public async Task<AppResult> ReserveAsync(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, CancellationToken cancellationToken)
        {
            var command = new ReserveInventoryRangeCommand(roomTypeId, checkIn, checkOut, rooms);
            return await _reserveInventoryRangeCommandHandler.HandleAsync(command, cancellationToken);
        }
    }
}
