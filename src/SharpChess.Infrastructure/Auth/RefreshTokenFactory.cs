using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SharpChess.Application.Abstractions.Authentication;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Infrastructure.Auth;

public sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    private readonly JwtOptions _jwtOptions;
    private readonly TimeProvider _timeProvider;

    public RefreshTokenFactory(
        IOptions<JwtOptions> jwtOptions,
        TimeProvider timeProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _timeProvider = timeProvider;
    }

    public GeneratedRefreshToken Create()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(64);
        string plainTextToken = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
        DateTime expiresAtUtc = _timeProvider.GetUtcNow().UtcDateTime.AddDays(_jwtOptions.RefreshTokenDays);

        return new GeneratedRefreshToken(
            PlainTextToken: plainTextToken,
            TokenHash: RefreshTokenRecord.Hash(plainTextToken),
            ExpiresAtUtc: expiresAtUtc);
    }
}
