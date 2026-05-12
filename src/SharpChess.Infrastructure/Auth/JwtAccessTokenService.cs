using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using SharpChess.Application.Abstractions.Authentication;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Infrastructure.Auth;

public sealed class JwtAccessTokenService : IAccessTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly TimeProvider _timeProvider;

    public JwtAccessTokenService(
        IOptions<JwtOptions> jwtOptions,
        TimeProvider timeProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _timeProvider = timeProvider;
    }

    public IssuedAccessToken CreateToken(AuthenticatedUser user)
    {
        DateTime utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        DateTime expiresAtUtc = utcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        ];

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        SigningCredentials signingCredentials = new(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        string serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new IssuedAccessToken(serializedToken, expiresAtUtc);
    }
}
