using ReserveFlow.Inventory.Application.Abstractions;
using ReserveFlow.Inventory.Domain.Inventories;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.CreateInventoryRange
{
    internal sealed class CreateInventoryRangeCommandHandler : ICommandHandler<CreateInventoryRangeCommand, AppResult<Guid>>
    {
        private readonly IRoomInventoryRepository _roomInventoryRepository;
        private readonly IInventoryUnitOfWork _inventoryUnitOfWork;

        public CreateInventoryRangeCommandHandler(IRoomInventoryRepository roomInventory, IInventoryUnitOfWork inventoryUnitOfWork)
        {
            _roomInventoryRepository = roomInventory;
            _inventoryUnitOfWork = inventoryUnitOfWork;
        }

        public async Task<AppResult<Guid>> HandleAsync(CreateInventoryRangeCommand command, CancellationToken cancellationToken)
        {
            if (command.startDate > command.endDate)
            {
                return AppResult.Failure<Guid>(ResultError.Validation("Inventory.InvalidDateRange", "Start date cannot be later than end date."));
            }

            for (var date = command.startDate; date <= command.endDate; date = date.AddDays(1))
            {
                var existingInventory = await _roomInventoryRepository.GetByRoomTypeAndDateAsync(command.roomTypeId, date, cancellationToken);

                if (existingInventory is not null)
                {
                    return AppResult.Failure<Guid>(ResultError.Conflict("Inventory.AlreadyExists", $"Inventory already exists for {date}."));
                }
            }

            for (var date = command.startDate; date <= command.endDate; date = date.AddDays(1))
            {
                var inventoryResult = RoomInventory.Create(command.roomTypeId, date, command.totalRooms, command.totalRooms);

                if (inventoryResult.IsFailure)
                {
                    return AppResult.Failure<Guid>(inventoryResult.Error);
                }

                await _roomInventoryRepository.AddAsync(inventoryResult.Value, cancellationToken);
            }

            await _inventoryUnitOfWork.SaveChangesAsync(cancellationToken);

            return AppResult.Success(command.roomTypeId);
        }
    }
}
