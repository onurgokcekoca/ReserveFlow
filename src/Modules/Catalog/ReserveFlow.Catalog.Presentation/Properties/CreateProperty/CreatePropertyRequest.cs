using ReserveFlow.Catalog.Domain.Properties;

namespace ReserveFlow.Catalog.Presentation.Properties.CreateProperty;

public sealed record CreatePropertyRequest(
    string? Name,
    string? Description,
    string? Country,
    string? City,
    string? AddressLine,
    decimal? Latitude,
    decimal? Longitude,
    PropertyType PropertyType);
