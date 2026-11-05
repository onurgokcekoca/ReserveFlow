using ReserveFlow.Shared.Kernel.Abstractions;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Domain.Inventories
{
    public sealed class RoomInventory : AggregateRoot<Guid>
    {
        private RoomInventory(Guid id, Guid roomTypeId, DateOnly date, int totalRooms, int availableRooms) : base(id)
        {
            RoomTypeId = roomTypeId;
            Date = date;
            TotalRooms = totalRooms;
            AvailableRooms = availableRooms;
        }

        public Guid RoomTypeId { get; }

        public DateOnly Date { get; }

        public int TotalRooms { get; }

        public int AvailableRooms { get; private set; }
        public int Version { get; private set; } = 1;

        public static AppResult<RoomInventory> Create(Guid roomTypeId, DateOnly date, int totalRooms, int availableRooms)
        {
            if (roomTypeId == Guid.Empty)
            {
                return AppResult.Failure<RoomInventory>(ResultError.Validation("RoomInventory.RoomTypeIdRequired", "Room type ID cannot be empty."));
            }

            if (totalRooms <= 0)
            {
                return AppResult.Failure<RoomInventory>(ResultError.Validation("RoomInventory.InvalidTotalRooms", "Total rooms must be greater than zero."));
            }

            if (availableRooms < 0)
            {
                return AppResult.Failure<RoomInventory>(ResultError.Validation("RoomInventory.InvalidAvailableRooms", "Available rooms cannot be negative."));
            }

            if (availableRooms > totalRooms)
            {
                return AppResult.Failure<RoomInventory>(ResultError.Validation("RoomInventory.AvailableRoomsExceedTotal", "Available rooms cannot exceed total rooms."));
            }

            Guid id = Guid.CreateVersion7();

            return AppResult.Success(new RoomInventory(id, roomTypeId, date, totalRooms, availableRooms));
        }

        public AppResult Reserve(int rooms)
        {
            if (rooms <= 0)
            {
                return AppResult.Failure(ResultError.Validation("RoomInventory.InvalidRooms", "Number of rooms to reserve cannot be negative."));
            }

            if (rooms > AvailableRooms)
            {
                return AppResult.Failure(ResultError.Conflict("RoomInventory.NotEnoughAvailableRooms", "Not enough available rooms to reserve the requested number."));
            }

            AvailableRooms -= rooms;
            Version++;
            return AppResult.Success();
        }

        public AppResult Release(int rooms)
        {
            if (rooms <= 0)
            {
                return AppResult.Failure(ResultError.Validation("RoomInventory.InvalidRooms", "Number of rooms to release must be greater than zero."));
            }

            if (AvailableRooms + rooms > TotalRooms)
            {
                return AppResult.Failure(ResultError.Conflict("RoomInventory.ReleaseExceedsTotalRooms", "Released rooms would exceed total room capacity."));
            }

            AvailableRooms += rooms;
            Version++;

            return AppResult.Success();
        }
    }
}
