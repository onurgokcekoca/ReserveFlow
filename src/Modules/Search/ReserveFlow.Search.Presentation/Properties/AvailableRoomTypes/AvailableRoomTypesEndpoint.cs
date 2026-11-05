using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Search.Application.AvailableRoomTypes;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Search.Presentation.Properties.AvailableRoomTypes
{
    public static class AvailableRoomTypesEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/properties/{propertyId}/available-room-types",
                async (
                    [FromRoute]
                    Guid propertyId,
                    [FromQuery]
                    DateOnly checkIn,
                    [FromQuery]
                    DateOnly checkOut,
                    [FromQuery]
                    int rooms,
                    [FromQuery]
                    int guests,
                    [FromServices]
                    IQueryHandler<SearchAvailableRoomTypesQuery, AppResult<SearchAvailableRoomTypesResponse>> handler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new SearchAvailableRoomTypesQuery(propertyId, checkIn, checkOut, rooms, guests);

                    var result = await handler.HandleAsync(query, cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : result.Error.ToProblem();
                })
                .WithTags("Search")
                .WithName("SearchAvailableRoomTypes");
        }
    }
}
