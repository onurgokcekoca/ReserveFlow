namespace ReserveFlow.Catalog.Application.Abstractions.Caching;

public static class PropertyCacheKeys
{
    public static string Detail(Guid propertyId) => $"catalog:property:detail:v1:{propertyId:N}";
}
