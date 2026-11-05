using ReserveFlow.Catalog.Domain.Properties;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Catalog.Application.Properties.CreateProperty
{
    public record CreatePropertyCommand(
        string? Name,
        string? Description,
        string? Country,
        string? City,
        string? AddressLine,
        decimal? Latitude,
        decimal? Longitude,
        PropertyType PropertyType
    ) : ICommand<AppResult<Guid>>
    { }
}
