namespace SharpChess.Api.Contracts.Auth;

public record CurrentUserResponse(
    string Id,
    string Username,
    string Email,
    string Role);
