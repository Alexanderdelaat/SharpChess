using FluentResults;
using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Auth.Errors;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Application.Account.Services;

public sealed class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthPasswordValidator _passwordValidator;

    public AccountService(IUserRepository userRepository, IAuthPasswordValidator passwordValidator)
    {
        _userRepository = userRepository;
        _passwordValidator = passwordValidator;
    }

    public Task<Result> UpdateEmailAsync(string userId, string newEmail, CancellationToken cancellationToken)
        => _userRepository.UpdateEmailAsync(userId, newEmail, cancellationToken);

    public Task<Result> UpdateUsernameAsync(string userId, string newUsername, CancellationToken cancellationToken)
        => _userRepository.UpdateUsernameAsync(userId, newUsername, cancellationToken);

    public async Task<Result> UpdatePasswordAsync(string userId, string currentPassword, string newPassword, string confirmNewPassword, CancellationToken cancellationToken)
    {
        if (newPassword != confirmNewPassword)
        {
            return Result.Fail(AuthErrorCodes.PasswordMismatch);
        }

        Result validation = _passwordValidator.Validate(newPassword);
        if (validation.IsFailed)
        {
            return validation;
        }

        return await _userRepository.UpdatePasswordAsync(userId, currentPassword, newPassword, cancellationToken);
    }
}
