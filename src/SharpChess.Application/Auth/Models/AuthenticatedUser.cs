namespace SharpChess.Application.Auth.Models;

public sealed record AuthenticatedUser(
    string Id,
    string Username,
    string Email,
    string Role);
