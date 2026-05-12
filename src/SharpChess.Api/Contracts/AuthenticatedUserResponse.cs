namespace SharpChess.Api.Contracts.Auth;

public record AuthenticatedUserResponse(
    string Id,
    string Username,
    string Email,
    string Role);
