using FluentResults;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<Result<AuthenticatedUser>> CreateAsync(
        string username,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken);

    Task<AuthenticatedUser?> FindByIdAsync(string userId, CancellationToken cancellationToken);
    Task<AuthenticatedUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken);
    Task EnsureRoleExistsAsync(string role, CancellationToken cancellationToken);
}
