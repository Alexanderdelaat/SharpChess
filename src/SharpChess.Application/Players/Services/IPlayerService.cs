using SharpChess.Application.Players.Models;

namespace SharpChess.Application.Players.Services;

public interface IPlayerService
{
    Task<IReadOnlyList<PlayerSummary>> GetPlayersAsync(CancellationToken cancellationToken);
}
