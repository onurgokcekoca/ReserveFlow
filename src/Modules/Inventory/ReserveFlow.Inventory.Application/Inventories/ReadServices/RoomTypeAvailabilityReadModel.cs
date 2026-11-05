namespace ReserveFlow.Inventory.Application.Inventories.ReadServices
{
    public sealed record RoomTypeAvailabilityReadModel(Guid RoomTypeId, bool HasCompleteInventory, int MaxAvailableRooms);
}
