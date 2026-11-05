using Microsoft.AspNetCore.Routing;
using ReserveFlow.Inventory.Presentation.Inventories.Availability;
using ReserveFlow.Inventory.Presentation.Inventories.GetRoomAvailability;
using ReserveFlow.Inventory.Presentation.Inventories.ReleaseInventoryRange;
using ReserveFlow.Inventory.Presentation.Inventories.ReserveInventoryRange;

namespace ReserveFlow.Inventory.Presentation
{
    public static class InventoryEndpoints
    {
        public static IEndpointRouteBuilder MapInventoryEndpoints(
            this IEndpointRouteBuilder app)
        {
            CreateInventoryRangeEndpoint.MapEndpoint(app);
            GetRoomAvailabilityEndpoint.MapEndpoint(app);
            ReserveInventoryRangeEndpoint.MapEndpoints(app);
            ReleaseInventoryRangeEndpoint.MapEndpoints(app);

            return app;
        }
    }
}
