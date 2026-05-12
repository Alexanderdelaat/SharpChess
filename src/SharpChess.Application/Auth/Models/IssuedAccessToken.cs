namespace SharpChess.Application.Auth.Models;

public sealed record IssuedAccessToken(
    string Token,
    DateTime ExpiresAtUtc);
