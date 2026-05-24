namespace SharpChess.Api.Contracts.Players;

public record PlayerResponse(
    string Id,
    string Username,
    DateTime? LastOnlineAt,
    int Rating,
    string Status);
