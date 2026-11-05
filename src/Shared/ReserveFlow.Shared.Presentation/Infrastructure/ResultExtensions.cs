using Microsoft.AspNetCore.Http;
using ReserveFlow.Shared.Kernel.Results;

using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace ReserveFlow.Shared.Presentation.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblem(this ResultError error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return HttpResults.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Description);
    }
}
