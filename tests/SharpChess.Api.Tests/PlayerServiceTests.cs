using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Players;

namespace SharpChess.Api.Tests;

[TestClass]
public sealed class PlayerServiceTests
{
    [TestMethod]
    public async Task GetPlayersAsync_ReturnsRepositoryPlayers()
    {
        PlayerSummary[] expectedPlayers =
        [
            new("user-1", "alex", null, 1200, "Actief"),
            new("user-2", "sam", null, 1200, "Actief"),
        ];
        RecordingPlayerRepository playerRepository = new(expectedPlayers);
        PlayerService sut = new(playerRepository);

        IReadOnlyList<PlayerSummary> players = await sut.GetPlayersAsync(CancellationToken.None);

        CollectionAssert.AreEqual(expectedPlayers, players.ToArray());
        Assert.AreEqual(1, playerRepository.GetPlayersAsyncCallCount);
    }

    private sealed class RecordingPlayerRepository : IPlayerRepository
    {
        private readonly IReadOnlyList<PlayerSummary> _players;

        public RecordingPlayerRepository(IReadOnlyList<PlayerSummary> players)
        {
            _players = players;
        }

        public int GetPlayersAsyncCallCount { get; private set; }

        public Task<IReadOnlyList<PlayerSummary>> GetPlayersAsync(CancellationToken cancellationToken)
        {
            GetPlayersAsyncCallCount++;
            return Task.FromResult(_players);
        }
    }
}
