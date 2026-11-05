namespace ReserveFlow.Catalog.Application.Properties.ReadServices
{
    public sealed record PropertyRoomTypesReadModel(Guid PropertyId, IReadOnlyCollection<PropertyRoomTypeReadModel> RoomTypes);
}
