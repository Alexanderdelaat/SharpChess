using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharpChess.Api.Contracts.Auth;
using SharpChess.Application.Auth.Commands.Register;

namespace SharpChess.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

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
}