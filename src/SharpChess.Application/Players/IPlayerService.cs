namespace SharpChess.Application.Players;

public interface IPlayerService
{
    Task<IReadOnlyList<PlayerSummary>> GetPlayersAsync(CancellationToken cancellationToken);
}
