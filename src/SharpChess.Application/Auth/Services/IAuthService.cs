using FluentResults;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Application.Auth.Services;

public interface IAuthService
{
    Task<Result<RegisterResult>> RegisterAsync(
        string username,
        string email,
        string password,
        string confirmPassword,
        CancellationToken cancellationToken);

    Task<Result<AuthSessionResult>> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken);

    Task<Result<AuthSessionResult>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken);

    Task LogoutAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken);
}
