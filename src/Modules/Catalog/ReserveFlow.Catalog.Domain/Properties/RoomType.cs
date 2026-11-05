using ReserveFlow.Shared.Kernel.Abstractions;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Kernel.ValueObjects;

namespace ReserveFlow.Catalog.Domain.Properties
{
    public sealed class RoomType : Entity<Guid>
    {
        private RoomType(Guid id, string name, int capacity, int totalRooms, Money basePrice) : base(id)
        {
            Name = name;
            Capacity = capacity;
            TotalRooms = totalRooms;
            BasePrice = basePrice;
        }

        private RoomType(
            Guid id,
            string name,
            int capacity,
            int totalRooms)
            : base(id)
        {
            Name = name;
            Capacity = capacity;
            TotalRooms = totalRooms;
            BasePrice = null!;
        }

        public string Name { get; private set; }
        public int Capacity { get; private set; }
        public int TotalRooms { get; private set; }
        public Money BasePrice { get; private set; }

        public static AppResult<RoomType> Create(string? name, int capacity, int totalRooms, Money? basePrice)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return AppResult.Failure<RoomType>(ResultError.Validation("RoomType.NameRequired", "Name cannot be empty."));
            }

            if (name.Trim().Length > 100)
            {
                return AppResult.Failure<RoomType>(ResultError.Validation("RoomType.NameTooLong", "Name cannot exceed 100 characters."));
            }

            if (capacity < 1)
            {
                return AppResult.Failure<RoomType>(ResultError.Validation("RoomType.InvalidCapacity", "Capacity must be a greater than 0."));
            }

            if (totalRooms < 1)
            {
                return AppResult.Failure<RoomType>(ResultError.Validation("RoomType.InvalidTotalRooms", "TotalRooms must be a greater than 0."));
            }

            if (basePrice == null)
            {
                return AppResult.Failure<RoomType>(ResultError.Validation("RoomType.BasePriceRequired", "BasePrice cannot be null."));
            }

            var id = Guid.CreateVersion7();

            return AppResult.Success(new RoomType(id, name.Trim(), capacity, totalRooms, basePrice));
        }
    }
}
