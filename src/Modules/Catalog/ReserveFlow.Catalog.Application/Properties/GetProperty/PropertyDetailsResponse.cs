namespace ReserveFlow.Catalog.Application.Properties.GetProperty;

public class PropertyDetailsResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? PropertyType { get; init; }
    public Guid ProviderId { get; init; }

    public string? Country { get; init; }
    public string? City { get; init; }
    public string? AddressLine { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public List<RoomTypeResponse> RoomTypes { get; init; } = [];
}
