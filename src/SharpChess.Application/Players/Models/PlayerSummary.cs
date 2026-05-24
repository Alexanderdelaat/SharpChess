namespace SharpChess.Application.Players.Models;

public sealed record PlayerSummary(
    string Id,
    string Username,
    DateTime? LastOnlineAt,
    int Rating,
    string Status);
