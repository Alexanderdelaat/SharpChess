using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharpChess.Api.Contracts.Auth;
using SharpChess.Api.Security;
using SharpChess.Application.Auth.Constants;
using SharpChess.Application.Auth.Models;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Api.Controllers;
/// <summary>
/// Beheert authenticatie-gerelateerde endpoints zoals registreren en inloggen.
/// </summary>
/// 
[ApiController]
[Route("api/[controller]")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registreert een nieuwe gebruiker met gebruikersnaam, e-mailadres en wachtwoord.
    /// </summary>
    /// <param name="request">De registratiegegevens van de gebruiker.</param>
    /// <param name="cancellationToken">Token waarmee de aanvraag kan worden geannuleerd.</param>
    /// <returns>
    /// Een succesvolle response met de aangemaakte gebruiker, of een foutresponse als de registratie mislukt.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        Result<SharpChess.Application.Auth.Commands.Register.RegisterResult> result = await _authService.RegisterAsync(
            request.Username,
            request.Email,
            request.Password,
            request.ConfirmPassword,
            cancellationToken);

        if (result.IsFailed)
        {
            return CreateProblem(result, StatusCodes.Status400BadRequest, "Registratie mislukt.");
        }

        return Ok(new RegisterResponse(
            Id: result.Value.Id,
            Username: result.Value.Username,
            Email: result.Value.Email,
            Role: ApplicationRoles.User));
    }

    /// <summary>
    /// Logt de gebuiker in.
    /// </summary>
    /// <param name="request">De logingegevens van de gebruiker.</param>
    /// <param name="cancellationToken">Token waarmee de aanvraag kan worden geannuleerd.</param>
    /// <returns>Een succesvolle loginpoging of een foutresponse als het mislukt.</returns>
    [EnableRateLimiting("auth-login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        Result<AuthSessionResult> result = await _authService.LoginAsync(
            request.Username,
            request.Password,
            cancellationToken);

        if (result.IsFailed)
        {
            return CreateProblem(result, StatusCodes.Status401Unauthorized, "Login mislukt.");
        }

        return Ok(ToLoginResponse(result.Value));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        Result<AuthSessionResult> result = await _authService.RefreshAsync(request.RefreshToken, cancellationToken);

        if (result.IsFailed)
        {
            return CreateProblem(result, StatusCodes.Status401Unauthorized, "Sessie vernieuwen mislukt.");
        }

        return Ok(ToRefreshResponse(result.Value));
    }

    [Authorize(Roles = $"{ApplicationRoles.User},{ApplicationRoles.Admin}")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(userId, request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = $"{ApplicationRoles.User},{ApplicationRoles.Admin}")]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        string? userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Result<AuthenticatedUser> result = await _authService.GetCurrentUserAsync(userId, cancellationToken);

        if (result.IsFailed)
        {
            return CreateProblem(result, StatusCodes.Status401Unauthorized, "Gebruiker niet geauthenticeerd.");
        }

        return Ok(new CurrentUserResponse(
            Id: result.Value.Id,
            Username: result.Value.Username,
            Email: result.Value.Email,
            Role: result.Value.Role));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet("admin/ping")]
    public IActionResult AdminPing()
    {
        return Ok(new AdminAccessResponse("Admin toegang bevestigd."));
    }

    private static LoginResponse ToLoginResponse(AuthSessionResult session)
    {
        return new LoginResponse(
            AccessToken: session.AccessToken,
            AccessTokenExpiresAtUtc: session.AccessTokenExpiresAtUtc,
            RefreshToken: session.RefreshToken,
            RefreshTokenExpiresAtUtc: session.RefreshTokenExpiresAtUtc,
            User: ToUserResponse(session.User));
    }

    private static RefreshTokenResponse ToRefreshResponse(AuthSessionResult session)
    {
        return new RefreshTokenResponse(
            AccessToken: session.AccessToken,
            AccessTokenExpiresAtUtc: session.AccessTokenExpiresAtUtc,
            RefreshToken: session.RefreshToken,
            RefreshTokenExpiresAtUtc: session.RefreshTokenExpiresAtUtc,
            User: ToUserResponse(session.User));
    }

    private static AuthenticatedUserResponse ToUserResponse(AuthenticatedUser user)
    {
        return new AuthenticatedUserResponse(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            Role: user.Role);
    }

    private ObjectResult CreateProblem(ResultBase result, int statusCode, string title)
    {
        ValidationProblemDetails problemDetails = new(new Dictionary<string, string[]>
        {
            ["auth"] = result.Errors.Select(error => error.Message).Distinct().ToArray(),
        })
        {
            Status = statusCode,
            Title = title,
        };

        return StatusCode(statusCode, problemDetails);
    }
}
