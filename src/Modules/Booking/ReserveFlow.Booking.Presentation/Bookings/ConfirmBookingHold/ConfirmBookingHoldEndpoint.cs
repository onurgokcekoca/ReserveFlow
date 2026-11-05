using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Booking.Application.Bookings.ConfirmBookingHold;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Booking.Presentation.Bookings.ConfirmBookingHold
{
    public static class ConfirmBookingHoldEndpoint
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/booking/holds/{bookingId}/confirm", async (
                [FromRoute]
                Guid bookingId,
                [FromServices]
                ICommandHandler<ConfirmBookingHoldCommand, AppResult> handler,
                CancellationToken cancellationToken
                ) =>
            {
                var command = new ConfirmBookingHoldCommand(bookingId);

                var result = await handler.HandleAsync(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }

                return result.Error.ToProblem();
            })
                .WithTags("Booking")
                .WithName("ConfirmBookingHold");
        }
    }
}
