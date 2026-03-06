using FluentResults;
using MediatR;

namespace SharpChess.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email, 
    string Password,
    string ConfirmPassword) : IRequest<Result<RegisterResult>>;

public record RegisterResult(
    string Id,
    string Username,
    string Email
);