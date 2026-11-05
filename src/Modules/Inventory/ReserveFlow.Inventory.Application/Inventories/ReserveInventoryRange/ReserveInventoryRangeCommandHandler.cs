using ReserveFlow.Inventory.Application.Abstractions;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.ReserveInventoryRange
{
    public class ReserveInventoryRangeCommandHandler : ICommandHandler<ReserveInventoryRangeCommand, AppResult>
    {
        private readonly IRoomInventoryRepository _roomInventoryRepository;
        private readonly IInventoryUnitOfWork _inventoryUnitOfWork;

        public ReserveInventoryRangeCommandHandler(IRoomInventoryRepository roomInventoryRepository, IInventoryUnitOfWork inventoryUnitOfWork)
        {
            _roomInventoryRepository = roomInventoryRepository;
            _inventoryUnitOfWork = inventoryUnitOfWork;
        }

        public async Task<AppResult> HandleAsync(ReserveInventoryRangeCommand command, CancellationToken cancellationToken)
        {
            if (command.checkIn > command.checkOut)
            {
                return AppResult.Failure(ResultError.Validation("Inventory.InvalidDateRange", "Checkin date can not be before checkout date"));
            }

            if (command.checkIn == command.checkOut)
            {
                return AppResult.Failure(ResultError.Validation("Inventory.IncompleteDateRange", "Checkin date can not be equal checkout date"));
            }

            if (command.rooms < 1)
            {
                return AppResult.Failure(ResultError.Validation("Inventory.InsufficientAvailability", "Rooms must be greater than 0"));
            }

            var roomInventory = await _roomInventoryRepository.GetByRoomTypeAndDateRangeAsync(command.roomTypeId, command.checkIn, command.checkOut, cancellationToken);

            var expectedNights = (command.checkOut.DayNumber - command.checkIn.DayNumber);

            if (expectedNights != roomInventory.Count)
            {
                return AppResult.Failure(ResultError.Conflict("Inventory.InsufficientAvailability", "Not enough nights available"));
            }

            var maxAvailableRooms = roomInventory.Min(r => r.AvailableRooms);

            if (maxAvailableRooms < command.rooms)
            {
                return AppResult.Failure(ResultError.Conflict("Inventory.InsufficientAvailability", "Not enough available rooms"));
            }

            foreach (var inventory in roomInventory)
            {
                var reserveResult = inventory.Reserve(command.rooms);
                if (!reserveResult.IsSuccess)
                {
                    return AppResult.Failure(ResultError.Conflict("RoomInventory.NotEnoughAvailableRooms", "Failed to reserve inventory"));
                }
            }

            await _inventoryUnitOfWork.SaveChangesAsync(cancellationToken);

            return AppResult.Success(true);
        }
    }
}
