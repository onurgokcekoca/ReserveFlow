using Microsoft.EntityFrameworkCore;
using ReserveFlow.Catalog.Application.Properties;
using ReserveFlow.Catalog.Application.Properties.GetProperty;
using ReserveFlow.Catalog.Application.Properties.SearchProperties;
using ReserveFlow.Shared.Application.Pagination;

namespace ReserveFlow.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class PropertyReadRepository(CatalogDbContext catalogDbContext) : IPropertyReadRepository
{
    public async Task<PropertyDetailsResponse?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken)
    {
        return await catalogDbContext.Properties
            .AsNoTracking()
            .Where(property => property.Id == propertyId)
            .Select(property => new PropertyDetailsResponse
            {
                Id = property.Id,
                Name = property.Name,
                Description = property.Description,
                PropertyType = property.PropertyType.ToString(),
                ProviderId = property.ProviderId,

                Country = property.Location.Country,
                City = property.Location.City,
                AddressLine = property.Location.AddressLine,
                Latitude = property.Location.Latitude,
                Longitude = property.Location.Longitude,

                RoomTypes = property.RoomTypes
                    .Select(roomType => new RoomTypeResponse
                    {
                        Id = roomType.Id,
                        Name = roomType.Name,
                        Capacity = roomType.Capacity,
                        TotalRooms = roomType.TotalRooms,
                        BasePriceAmount = roomType.BasePrice.Amount,
                        Currency = roomType.BasePrice.Currency
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<PropertyListItemResponse>> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = catalogDbContext.Properties.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";

            query = query.Where(property =>
                EF.Functions.ILike(property.Name, pattern) ||
                EF.Functions.ILike(property.Location.City, pattern) ||
                EF.Functions.ILike(property.Location.Country, pattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var properties = await query
            .OrderBy(property => property.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(property => new PropertyListItemResponse
            {
                Id = property.Id,
                Name = property.Name,
                PropertyType = property.PropertyType,
                Country = property.Location.Country,
                City = property.Location.City,

                BasePriceFrom = property.RoomTypes
                    .OrderBy(roomType => roomType.BasePrice.Amount)
                    .Select(roomType => (decimal?)roomType.BasePrice.Amount)
                    .FirstOrDefault(),

                Currency = property.RoomTypes
                    .OrderBy(roomType => roomType.BasePrice.Amount)
                    .Select(roomType => roomType.BasePrice.Currency)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<PropertyListItemResponse>(
            properties,
            page,
            pageSize,
            totalCount,
            totalPages);
    }
}
