namespace SharpChess.Application.Auth.Models;

public sealed record GeneratedRefreshToken(
    string PlainTextToken,
    string TokenHash,
    DateTime ExpiresAtUtc);
