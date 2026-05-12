using System.Security.Cryptography;
using System.Text;

namespace SharpChess.Application.Auth.Models;

public class RefreshTokenRecord
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    private RefreshTokenRecord()
    {
    }

    private RefreshTokenRecord(
        string userId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public static RefreshTokenRecord Create(
        string userId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
    {
        return new RefreshTokenRecord(userId, tokenHash, expiresAtUtc, createdAtUtc);
    }

    public bool IsActive(DateTime utcNow)
    {
        return !IsRevoked && ExpiresAtUtc > utcNow;
    }

    public void Revoke(DateTime revokedAtUtc, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    public static string Hash(string plainTextToken)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(plainTextToken);
        byte[] hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes);
    }
}
