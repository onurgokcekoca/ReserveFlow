using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReserveFlow.Booking.Application.Exceptions;

namespace ReserveFlow.Booking.Presentation.Exceptions
{
    public class BookingHoldConcurrencyExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BookingHoldConcurrencyException)
            {
                return false;
            }
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Booking.ConcurrencyConflict",
                    Detail = exception.Message
                },
                cancellationToken);

            return true;
        }
    }
}
