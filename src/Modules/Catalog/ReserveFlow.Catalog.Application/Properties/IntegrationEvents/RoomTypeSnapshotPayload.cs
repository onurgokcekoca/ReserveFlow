namespace ReserveFlow.Catalog.Application.Properties.IntegrationEvents;

public sealed record RoomTypeSnapshotPayload(
    Guid RoomTypeId,
    string Name,
    int Capacity);
