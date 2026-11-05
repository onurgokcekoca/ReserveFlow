using ReserveFlow.Catalog.Domain.Properties;

namespace ReserveFlow.Catalog.Application.Properties
{
    public interface IPropertyRepository
    {
        Task AddAsync(Property property, CancellationToken cancellationToken);

        Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken);
    }
}
