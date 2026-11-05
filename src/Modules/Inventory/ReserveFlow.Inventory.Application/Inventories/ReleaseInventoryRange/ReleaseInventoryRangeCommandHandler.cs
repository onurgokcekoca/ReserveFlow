using ReserveFlow.Inventory.Application.Abstractions;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.ReleaseInventoryRange
{
    public sealed class ReleaseInventoryRangeCommandHandler : ICommandHandler<ReleaseInventoryRangeCommand, AppResult>
    {
        private readonly IRoomInventoryRepository _roomInventoryRepository;
        private readonly IInventoryUnitOfWork _inventoryUnitOfWork;

        public ReleaseInventoryRangeCommandHandler(IRoomInventoryRepository roomInventoryRepository, IInventoryUnitOfWork inventoryUnitOfWork)
        {
            _roomInventoryRepository = roomInventoryRepository;
            _inventoryUnitOfWork = inventoryUnitOfWork;
        }

        public async Task<AppResult> HandleAsync(ReleaseInventoryRangeCommand command, CancellationToken cancellationToken)
        {
            if (command.checkIn >= command.checkOut)
            {
                return AppResult.Failure(ResultError.Validation("Inventory.InvalidDateRange", "Check-in date must be before check-out date."));
            }

            if (command.rooms <= 0)
            {
                return AppResult.Failure(ResultError.Validation("Inventory.InvalidRooms", "Rooms must be greater than zero."));
            }

            var roomInventory = await _roomInventoryRepository.GetByRoomTypeAndDateRangeAsync(
                    command.roomTypeId,
                    command.checkIn,
                    command.checkOut,
                    cancellationToken);

            var expectedNights = command.checkOut.DayNumber - command.checkIn.DayNumber;

            if (roomInventory.Count != expectedNights)
            {
                return AppResult.Failure(ResultError.Conflict("Inventory.IncompleteDateRange", "Inventory is not available for the complete date range."));
            }

            var cannotRelease = roomInventory.Any(inventory => inventory.AvailableRooms + command.rooms > inventory.TotalRooms);

            if (cannotRelease)
            {
                return AppResult.Failure(ResultError.Conflict("Inventory.ReleaseExceedsTotalRooms", "Released rooms would exceed total room capacity."));
            }

            foreach (var inventory in roomInventory)
            {
                var releaseResult = inventory.Release(command.rooms);

                if (!releaseResult.IsSuccess)
                {
                    return AppResult.Failure(releaseResult.Error);
                }
            }

            await _inventoryUnitOfWork.SaveChangesAsync(cancellationToken);

            return AppResult.Success();
        }
    }
}
