using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace SharpChess.Application.Auth.Commands.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly UserManager<IdentityUser> _userManager;

public RegisterUserCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<RegisterResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (command.Password != command.ConfirmPassword)
        return  Result.Fail(AuthErrorCodes.PasswordMismatch);

        IdentityUser user = new IdentityUser
        {
            UserName = command.Username,
            Email = command.Email,
        };

        IdentityResult result = await _userManager.CreateAsync(user, command.Password);

       if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(identityError => new Error(identityError.Code)).ToList());

        return Result.Ok(new RegisterResult(
            Id: user.Id,
            Username: user.UserName!,
            Email: user.Email!));
    }

} 