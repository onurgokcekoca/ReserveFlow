using ReserveFlow.Search.Application.Common.Models;

namespace ReserveFlow.Search.Application.Common;

public interface IPropertySearchRepository
{
    Task IndexAsync(
        PropertySearchDocument document,
        CancellationToken cancellationToken = default);

    Task<PropertySearchDocument?> GetAsync(
        Guid propertyId,
        CancellationToken cancellationToken = default);

    Task<PropertySearchResult> SearchAsync(
        PropertySearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
