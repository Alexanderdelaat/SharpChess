namespace SharpChess.Application.Players;

public sealed record PlayerSummary(
    string Id,
    string Username,
    DateTime? LastOnlineAt,
    int Rating,
    string Status);
