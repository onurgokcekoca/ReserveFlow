namespace ReserveFlow.Catalog.Application.Properties.ReadServices
{
    public sealed record PropertyRoomTypeReadModel(
        Guid RoomTypeId,
        string Name,
        int Capacity,
        decimal BasePrice,
        string Currency);
}
