using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Api.Extensions;

public static class ProblemExtensions
{
    public static ActionResult ToActionResult(this ControllerBase controller, List<Error> errors)
    {
        Result<object?> response = errors.Count == 0
            ? Error.Unexpected("Unexpected", "Unexpected error.")
            : errors;

        return controller.StatusCode(GetStatusCode(response.Errors), response);
    }

    public static Microsoft.AspNetCore.Http.IResult ToProblem(this HttpContext httpContext, Exception exception, bool includeExceptionDetails)
    {
        var error = Error.Unexpected(
            code: "InternalServerError",
            description: includeExceptionDetails
                ? exception.Message
                : "An unexpected error occurred while processing the request.");

        Result<object?> response = error;
        return Results.Json(response, statusCode: StatusCodes.Status500InternalServerError);
    }

    private static int GetStatusCode(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return StatusCodes.Status500InternalServerError;
        }

        if (errors.All(error => error.Type == ErrorKind.Validation))
        {
            return StatusCodes.Status400BadRequest;
        }

        return errors[0].Type switch
        {
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
