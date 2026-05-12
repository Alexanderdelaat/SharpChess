using SharpChess.Application.Auth.Models;

namespace SharpChess.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshTokenRecord refreshToken, CancellationToken cancellationToken);
    Task<RefreshTokenRecord?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
