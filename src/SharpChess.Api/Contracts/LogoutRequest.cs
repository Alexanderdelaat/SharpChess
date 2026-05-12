namespace SharpChess.Api.Contracts.Auth;

public record LogoutRequest(
    string RefreshToken);
