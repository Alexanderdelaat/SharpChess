namespace SharpChess.Api.Contracts.Auth;

public record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    AuthenticatedUserResponse User);
