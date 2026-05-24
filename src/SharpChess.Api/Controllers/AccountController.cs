using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpChess.Api.Contracts.Auth;
using SharpChess.Api.Security;
using SharpChess.Application.Account;
using SharpChess.Application.Auth.Constants;

namespace SharpChess.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{ApplicationRoles.User},{ApplicationRoles.Admin}")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService) => _accountService = accountService;

    [HttpPatch("email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        Result result = await _accountService.UpdateEmailAsync(userId, request.NewEmail, cancellationToken);
        return result.IsFailed
            ? CreateProblem(result, StatusCodes.Status400BadRequest, "E-mailadres wijzigen mislukt.")
            : NoContent();
    }

    [HttpPatch("username")]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        Result result = await _accountService.UpdateUsernameAsync(userId, request.NewUsername, cancellationToken);
        return result.IsFailed
            ? CreateProblem(result, StatusCodes.Status400BadRequest, "Gebruikersnaam wijzigen mislukt.")
            : NoContent();
    }

    [HttpPatch("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        Result result = await _accountService.UpdatePasswordAsync(
            userId, request.CurrentPassword, request.NewPassword, request.ConfirmNewPassword, cancellationToken);
        return result.IsFailed
            ? CreateProblem(result, StatusCodes.Status400BadRequest, "Wachtwoord wijzigen mislukt.")
            : NoContent();
    }

    private ObjectResult CreateProblem(ResultBase result, int statusCode, string title)
    {
        ValidationProblemDetails details = new(new Dictionary<string, string[]>
        {
            ["auth"] = result.Errors.Select(e => e.Message).Distinct().ToArray(),
        })
        {
            Status = statusCode,
            Title = title,
        };
        return StatusCode(statusCode, details);
    }
}
