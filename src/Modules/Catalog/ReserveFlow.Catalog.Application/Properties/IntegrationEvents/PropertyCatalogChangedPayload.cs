namespace ReserveFlow.Catalog.Application.Properties.IntegrationEvents;

public sealed record PropertyCatalogChangedPayload(
    Guid PropertyId,
    string Name,
    string? Description,
    string Country,
    string City,
    string AddressLine,
    string PropertyType,
    Guid ProviderId,
    IReadOnlyList<RoomTypeSnapshotPayload> RoomTypes);
