namespace SharpChess.Api.Contracts.Auth;

public record RegisterResponse(
    string Id,
    string Username,
    string Email);