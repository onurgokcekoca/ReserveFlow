using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Inventory.Application.Inventories.GetRoomAvailability;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Inventory.Presentation.Inventories.GetRoomAvailability
{
    internal static class GetRoomAvailabilityEndpoint
    {
        internal static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/inventory/room-types/{roomTypeId}/availability",
                async (
                    [FromRoute]
                    Guid roomTypeId,
                    [FromQuery]
                    DateOnly checkIn,
                    [FromQuery]
                    DateOnly checkOut,
                    [FromQuery]
                    int rooms,
                    [FromServices]
                    IQueryHandler<GetRoomAvailabilityQuery, AppResult<RoomAvailabilityResponse>> handler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetRoomAvailabilityQuery(roomTypeId, checkIn, checkOut, rooms);
                    var result = await handler.HandleAsync(query, cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.Ok(result.Value);
                    }

                    return result.Error.ToProblem();
                })
                .WithName("GetRoomAvailability")
                .WithTags("Inventory");
        }
    }
}
