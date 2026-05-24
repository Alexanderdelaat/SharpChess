using SharpChess.Application.Players.Models;

namespace SharpChess.Application.Abstractions.Persistence;

public interface IPlayerRepository
{
    Task<IReadOnlyList<PlayerSummary>> GetPlayersAsync(CancellationToken cancellationToken);
}
