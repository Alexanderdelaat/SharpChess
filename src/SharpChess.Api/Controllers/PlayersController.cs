using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpChess.Api.Contracts.Players;
using SharpChess.Application.Auth.Constants;
using SharpChess.Application.Players;

namespace SharpChess.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{ApplicationRoles.User},{ApplicationRoles.Admin}")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlayerResponse>>> GetPlayers(CancellationToken cancellationToken)
    {
        IReadOnlyList<PlayerSummary> players = await _playerService.GetPlayersAsync(cancellationToken);

        return Ok(players.Select(player => new PlayerResponse(
            Id: player.Id,
            Username: player.Username,
            LastOnlineAt: player.LastOnlineAt,
            Rating: player.Rating,
            Status: player.Status)));
    }
}
