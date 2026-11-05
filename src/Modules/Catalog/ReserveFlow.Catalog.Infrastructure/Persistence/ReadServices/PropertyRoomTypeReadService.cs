using Microsoft.EntityFrameworkCore;
using ReserveFlow.Catalog.Application.Abstractions;
using ReserveFlow.Catalog.Application.Properties.ReadServices;

namespace ReserveFlow.Catalog.Infrastructure.Persistence.ReadServices
{
    internal sealed class PropertyRoomTypeReadService(CatalogDbContext dbContext) : IPropertyRoomTypeReadService
    {
        public async Task<PropertyRoomTypesReadModel?> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken)
        {
            var property = await dbContext.Properties
                .AsNoTracking()
                .Where(p => p.Id == propertyId)
                .Select(p => new PropertyRoomTypesReadModel(
                    p.Id,
                    p.RoomTypes
                        .Select(rt => new PropertyRoomTypeReadModel(
                            rt.Id,
                            rt.Name,
                            rt.Capacity,
                            rt.BasePrice.Amount,
                            rt.BasePrice.Currency))
                        .ToList()))
                .FirstOrDefaultAsync(cancellationToken);

            return property;
        }
    }
}
