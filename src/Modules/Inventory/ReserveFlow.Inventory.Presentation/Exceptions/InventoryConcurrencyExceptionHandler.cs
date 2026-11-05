using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReserveFlow.Inventory.Application.Exceptions;

namespace ReserveFlow.Inventory.Presentation.Exceptions
{
    internal sealed class InventoryConcurrencyExceptionHandler
    : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not InventoryConcurrencyException)
            {
                return false;
            }

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Inventory.ConcurrencyConflict",
                    Detail = exception.Message
                },
                cancellationToken);

            return true;
        }
    }
}
