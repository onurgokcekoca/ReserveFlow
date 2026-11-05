namespace ReserveFlow.Search.Application.AvailableRoomTypes
{
    public sealed class SearchAvailableRoomTypesResponse
    {
        public Guid PropertyId { get; init; }

        public DateOnly CheckIn { get; init; }

        public DateOnly CheckOut { get; init; }

        public int RequestedRooms { get; init; }

        public int Guests { get; init; }

        public IReadOnlyCollection<AvailableRoomTypeResponse> RoomTypes { get; init; } = [];
    }

    public sealed class AvailableRoomTypeResponse
    {
        public Guid RoomTypeId { get; init; }

        public string Name { get; init; } = string.Empty;

        public int Capacity { get; init; }

        public decimal BasePrice { get; init; }

        public string Currency { get; init; } = string.Empty;

        public int MaxAvailableRooms { get; init; }
    }
}
