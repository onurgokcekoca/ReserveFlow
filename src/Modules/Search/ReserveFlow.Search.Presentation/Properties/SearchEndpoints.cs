using Microsoft.AspNetCore.Routing;
using ReserveFlow.Search.Presentation.Properties.AvailableRoomTypes;

namespace ReserveFlow.Search.Presentation.Properties
{
    public static class SearchEndpoints
    {
        public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
        {
            AvailableRoomTypesEndpoint.MapEndpoint(app);
            return app;
        }
    }
}
