using ReserveFlow.Catalog.Application.Abstractions.Caching;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Catalog.Application.Properties.GetProperty;

public class GetPropertyQueryHandler : IQueryHandler<GetPropertyQuery, AppResult<PropertyDetailsResponse>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IPropertyReadRepository _propertyReadRepository;
    private readonly ICacheService _cacheService;

    public GetPropertyQueryHandler(IPropertyReadRepository propertyReadRepository, ICacheService cacheService)
    {
        _propertyReadRepository = propertyReadRepository;
        _cacheService = cacheService;
    }

    public async Task<AppResult<PropertyDetailsResponse>> HandleAsync(GetPropertyQuery query, CancellationToken cancellationToken)
    {
        string cacheKey = PropertyCacheKeys.Detail(query.PropertyId);

        PropertyDetailsResponse? cachedProperty = await _cacheService.GetAsync<PropertyDetailsResponse>(cacheKey, cancellationToken);

        if (cachedProperty is not null)
        {
            return AppResult.Success(cachedProperty);
        }

        PropertyDetailsResponse? property = await _propertyReadRepository.GetByIdAsync(query.PropertyId, cancellationToken);

        if (property is null)
        {
            return AppResult.Failure<PropertyDetailsResponse>(ResultError.NotFound("Property.NotFound", "Property not found."));
        }

        await _cacheService.SetAsync(
            cacheKey,
            property,
            CacheTtl,
            cancellationToken);

        return AppResult.Success(property);
    }
}
