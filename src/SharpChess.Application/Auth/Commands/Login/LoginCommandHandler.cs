using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace SharpChess.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly UserManager<IdentityUser> _userManager;

    public LoginCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        IdentityUser? user = await _userManager.FindByNameAsync(command.Username);

        if (user is null)
            return Result.Fail(AuthErrorCodes.InvalidCredentials);

        bool passwordCorrect = await _userManager.CheckPasswordAsync(user, command.Password);

        if (!passwordCorrect)
            return Result.Fail(AuthErrorCodes.InvalidCredentials);

        string token = "";

        return Result.Ok(new LoginResult(Token: token));
    }
}