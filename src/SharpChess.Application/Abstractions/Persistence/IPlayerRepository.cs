using SharpChess.Application.Players;

namespace SharpChess.Application.Abstractions.Persistence;

public interface IPlayerRepository
{
    Task<IReadOnlyList<PlayerSummary>> GetPlayersAsync(CancellationToken cancellationToken);
}
