namespace SharpChess.Application.Auth.Models;

public sealed record AuthSessionResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    AuthenticatedUser User);
