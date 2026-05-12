using Microsoft.EntityFrameworkCore;
using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Infrastructure.Persistence;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly SharpChessDbContext _dbContext;

    public RefreshTokenRepository(SharpChessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshTokenRecord refreshToken, CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task<RefreshTokenRecord?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens.FirstOrDefaultAsync(
            token => token.TokenHash == tokenHash,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
