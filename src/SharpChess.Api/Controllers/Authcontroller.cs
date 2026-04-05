using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharpChess.Api.Contracts.Auth;
using SharpChess.Application.Auth.Commands.Register;
using SharpChess.Application.Auth.Commands.Login;


namespace SharpChess.Api.Controllers;
/// <summary>
/// Beheert authenticatie-gerelateerde endpoints zoals registreren en inloggen.
/// </summary>
/// 
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

/// <summary>
/// 
/// </summary>
/// <param name="mediator"></param>
    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }


    /// <summary>
    /// Registreert een nieuwe gebruiker met gebruikersnaam, e-mailadres en wachtwoord.
    /// </summary>
    /// <param name="request">De registratiegegevens van de gebruiker.</param>
    /// <returns>
    /// Een succesvolle response met de aangemaakte gebruiker, of een foutresponse als de registratie mislukt.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        RegisterCommand command = new RegisterCommand(
            Username: request.Username,
            Email: request.Email,
            Password: request.Password,
            ConfirmPassword: request.ConfirmPassword);

        Result<RegisterResult> result = await _mediator.Send(command);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(error => error.Message));

        return Ok(new RegisterResponse(
            Id: result.Value.Id,
            Username: result.Value.Username,
            Email: result.Value.Email));
    }

/// <summary>
/// Logt de gebuiker in.
/// </summary>
/// <param name="request"></param>
/// Een succesvolle loginpoging of een foutresponse als het mislukt.
/// <returns></returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        LoginCommand command = new LoginCommand(
            Username: request.Username,
            Password: request.Password);

        Result<LoginResult> result = await _mediator.Send(command);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(error => error.Message));

        return Ok(new LoginResponse(
            Token: result.Value.Token));
    }
}