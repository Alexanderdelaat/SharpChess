namespace SharpChess.Application.Auth.Commands.Register;
public record RegisterResult(
    string Id,
    string Username,
    string Email
);