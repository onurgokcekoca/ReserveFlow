using ReserveFlow.Catalog.Application.Properties.GetProperty;
using ReserveFlow.Catalog.Application.Properties.SearchProperties;
using ReserveFlow.Shared.Application.Pagination;

namespace ReserveFlow.Catalog.Application.Properties
{
    public interface IPropertyReadRepository
    {
        Task<PropertyDetailsResponse?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken);

        Task<PagedResult<PropertyListItemResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    }
}
