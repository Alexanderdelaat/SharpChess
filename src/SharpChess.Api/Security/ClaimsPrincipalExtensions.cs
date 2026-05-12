using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SharpChess.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
