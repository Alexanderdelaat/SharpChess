using FluentResults;
using MediatR;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Application.Auth.Commands.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly IAuthService _authService;

    public RegisterUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<RegisterResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(
            command.Username,
            command.Email,
            command.Password,
            command.ConfirmPassword,
            cancellationToken);
    }
}
