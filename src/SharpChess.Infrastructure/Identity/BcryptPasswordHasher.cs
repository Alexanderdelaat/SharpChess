using Microsoft.AspNetCore.Identity;

namespace SharpChess.Infrastructure.Identity;

public sealed class BcryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private const int WorkFactor = 12;

    public string HashPassword(ApplicationUser user, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user,
        string hashedPassword,
        string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        bool verified = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);

        if (!verified)
        {
            return PasswordVerificationResult.Failed;
        }

        return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor)
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Success;
    }
}
