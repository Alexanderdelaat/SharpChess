using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Players;

namespace SharpChess.Infrastructure.Identity;

public sealed class IdentityPlayerRepository : IPlayerRepository
{
    private const int DefaultRating = 1200;

    private readonly TimeProvider _timeProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityPlayerRepository(
        TimeProvider timeProvider,
        UserManager<ApplicationUser> userManager)
    {
        _timeProvider = timeProvider;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<PlayerSummary>> GetPlayersAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset utcNow = _timeProvider.GetUtcNow();

        return await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.UserName)
            .Select(user => new PlayerSummary(
                user.Id,
                user.UserName ?? string.Empty,
                null,
                DefaultRating,
                user.LockoutEnd.HasValue && user.LockoutEnd > utcNow ? "Niet actief" : "Actief"))
            .ToListAsync(cancellationToken);
    }
}
