using SharpChess.Application.Auth.Models;

namespace SharpChess.Application.Abstractions.Authentication;

public interface IRefreshTokenFactory
{
    GeneratedRefreshToken Create();
}
