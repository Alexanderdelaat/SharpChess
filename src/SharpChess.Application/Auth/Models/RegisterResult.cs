namespace SharpChess.Application.Auth.Models;

public record RegisterResult(
    string Id,
    string Username,
    string Email
);
