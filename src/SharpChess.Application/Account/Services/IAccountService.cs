using FluentResults;

namespace SharpChess.Application.Account.Services;

public interface IAccountService
{
    Task<Result> UpdateEmailAsync(string userId, string newEmail, CancellationToken cancellationToken);
    Task<Result> UpdateUsernameAsync(string userId, string newUsername, CancellationToken cancellationToken);
    Task<Result> UpdatePasswordAsync(string userId, string currentPassword, string newPassword, string confirmNewPassword, CancellationToken cancellationToken);
}
