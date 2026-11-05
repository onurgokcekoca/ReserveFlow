namespace ReserveFlow.Search.Application.Common.Models;

public sealed class PropertySearchDocument
{
    public Guid PropertyId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Country { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string AddressLine { get; init; } = string.Empty;

    public string PropertyType { get; init; } = string.Empty;

    public Guid ProviderId { get; init; }

    public int MaxGuests { get; init; }

    public IReadOnlyCollection<Guid> RoomTypeIds { get; init; } = [];

    public IReadOnlyCollection<string> RoomTypeNames { get; init; } = [];
}
