namespace SharpChess.Api.Contracts.Auth;
public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword);