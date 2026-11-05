using Microsoft.AspNetCore.Routing;
using ReserveFlow.Catalog.Presentation.Properties.AddRoomType;
using ReserveFlow.Catalog.Presentation.Properties.CreateProperty;
using ReserveFlow.Catalog.Presentation.Properties.GetProperty;
using ReserveFlow.Catalog.Presentation.Properties.SearchProperties;

namespace ReserveFlow.Catalog.Presentation;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        CreatePropertyEndpoint.MapEndpoint(app);
        AddRoomTypeEndpoint.MapEndpoint(app);
        GetPropertyEndpoint.MapEndpoint(app);
        SearchPropertiesEndpoint.MapEndpoint(app);

        return app;
    }
}
