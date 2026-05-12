using FluentResults;

namespace SharpChess.Application.Auth.Services;

public sealed class PasswordValidator : IAuthPasswordValidator
{
    public Result Validate(string password)
    {
        string candidatePassword = string.IsNullOrWhiteSpace(password) ? string.Empty : password;
        List<Error> errors = [];

        if (candidatePassword.Length < PasswordRequirements.MinimumLength)
        {
            errors.Add(new Error(AuthErrorCodes.PasswordTooShort));
        }

        if (!candidatePassword.Any(char.IsUpper))
        {
            errors.Add(new Error(AuthErrorCodes.PasswordRequiresUppercase));
        }

        if (!candidatePassword.Any(char.IsLower))
        {
            errors.Add(new Error(AuthErrorCodes.PasswordRequiresLowercase));
        }

        if (!candidatePassword.Any(char.IsDigit))
        {
            errors.Add(new Error(AuthErrorCodes.PasswordRequiresDigit));
        }

        if (!candidatePassword.Any(ch => !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch)))
        {
            errors.Add(new Error(AuthErrorCodes.PasswordRequiresSpecialCharacter));
        }

        return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
    }
}
