namespace SharpChess.Api.Contracts.Auth;

public record LoginResponse(
    string Id,
    string Username,
    string Email);