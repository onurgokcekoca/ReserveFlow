using Microsoft.EntityFrameworkCore;
using ReserveFlow.Catalog.Application.Properties;
using ReserveFlow.Catalog.Domain.Properties;

namespace ReserveFlow.Catalog.Infrastructure.Persistence.Repositories
{
    internal sealed class PropertyRepository(CatalogDbContext catalogDbContext) : IPropertyRepository
    {
        public async Task AddAsync(Property property, CancellationToken cancellationToken)
        {
            await catalogDbContext.Properties.AddAsync(property, cancellationToken);
        }

        public async Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken)
        {
            return await catalogDbContext.Properties.Include(p => p.RoomTypes).FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);
        }
    }
}
