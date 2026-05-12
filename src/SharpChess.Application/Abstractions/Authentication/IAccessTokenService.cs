using SharpChess.Application.Auth.Models;

namespace SharpChess.Application.Abstractions.Authentication;

public interface IAccessTokenService
{
    IssuedAccessToken CreateToken(AuthenticatedUser user);
}
