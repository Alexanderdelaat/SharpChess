using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace SharpChess.Api.Controllers;

public static class AuthProblemDetailsExtensions
{
    public static ObjectResult CreateAuthProblem(this ControllerBase controller, ResultBase result, int statusCode, string title)
    {
        ValidationProblemDetails problemDetails = new(new Dictionary<string, string[]>
        {
            ["auth"] = result.Errors.Select(error => error.Message).Distinct().ToArray(),
        })
        {
            Status = statusCode,
            Title = title,
        };

        return controller.StatusCode(statusCode, problemDetails);
    }
}
