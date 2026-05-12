using FluentResults;
using MediatR;
using SharpChess.Application.Auth.Models;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        Result<AuthSessionResult> loginResult = await _authService.LoginAsync(
            command.Username,
            command.Password,
            cancellationToken);

        if (loginResult.IsFailed)
        {
            return Result.Fail(loginResult.Errors);
        }

        return Result.Ok(new LoginResult(Token: loginResult.Value.AccessToken));
    }
}
