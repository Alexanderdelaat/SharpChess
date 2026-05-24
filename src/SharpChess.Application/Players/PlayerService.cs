using SharpChess.Application.Abstractions.Persistence;

namespace SharpChess.Application.Players;

public sealed class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public Task<IReadOnlyList<PlayerSummary>> GetPlayersAsync(CancellationToken cancellationToken)
    {
        return _playerRepository.GetPlayersAsync(cancellationToken);
    }
}
