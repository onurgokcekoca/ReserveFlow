using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Catalog.Application.Properties.AddRoomType;
using ReserveFlow.Catalog.Application.Properties.CreateProperty;
using ReserveFlow.Catalog.Application.Properties.GetProperty;
using ReserveFlow.Catalog.Application.Properties.SearchProperties;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Application.Pagination;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreatePropertyCommand, AppResult<Guid>>, CreatePropertyCommandHandler>();

        services.AddScoped<ICommandHandler<AddRoomTypeCommand, AppResult<Guid>>, AddRoomTypeCommandHandler>();

        services.AddScoped<IQueryHandler<GetPropertyQuery, AppResult<PropertyDetailsResponse>>, GetPropertyQueryHandler>();

        services.AddScoped<IQueryHandler<SearchPropertiesQuery, PagedResult<PropertyListItemResponse>>, SearchPropertiesQueryHandler>();

        return services;
    }
}
