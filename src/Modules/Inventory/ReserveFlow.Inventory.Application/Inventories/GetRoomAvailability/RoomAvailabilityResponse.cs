namespace ReserveFlow.Inventory.Application.Inventories.GetRoomAvailability
{
    public class RoomAvailabilityResponse
    {
        public Guid RoomTypeId { get; init; }
        public DateOnly CheckIn { get; init; }
        public DateOnly CheckOut { get; init; }
        public int RequestedRooms { get; init; }
        public bool IsAvailable { get; init; }
        public int MaxAvailableRooms { get; init; }
    }
}
