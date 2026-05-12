using Microsoft.AspNetCore.Identity;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Infrastructure.Identity;

public sealed class IdentityPasswordValidatorAdapter : IPasswordValidator<ApplicationUser>
{
    private readonly IAuthPasswordValidator _passwordValidator;

    public IdentityPasswordValidatorAdapter(IAuthPasswordValidator passwordValidator)
    {
        _passwordValidator = passwordValidator;
    }

    public Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager,
        ApplicationUser user,
        string? password)
    {
        string candidatePassword = password ?? string.Empty;
        FluentResults.Result validationResult = _passwordValidator.Validate(candidatePassword);

        if (validationResult.IsSuccess)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        IdentityError[] errors = validationResult.Errors
            .Select(error => new IdentityError
            {
                Code = "PasswordPolicy",
                Description = error.Message,
            })
            .ToArray();

        return Task.FromResult(IdentityResult.Failed(errors));
    }
}
