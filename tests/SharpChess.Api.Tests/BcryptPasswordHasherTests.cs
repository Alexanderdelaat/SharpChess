using Microsoft.AspNetCore.Identity;
using SharpChess.Infrastructure.Identity;

namespace SharpChess.Api.Tests;

[TestClass]
public sealed class BcryptPasswordHasherTests
{
    private static readonly ApplicationUser User = new();
    private readonly BcryptPasswordHasher _hasher = new();

    [TestMethod]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
    {
        string hash = _hasher.HashPassword(User, "StrongPassword1!");

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
    }

    [TestMethod]
    public void HashPassword_ValidPassword_DoesNotReturnPlainTextPassword()
    {
        const string password = "StrongPassword1!";

        string hash = _hasher.HashPassword(User, password);

        Assert.AreNotEqual(password, hash);
    }

    [TestMethod]
    public void VerifyHashedPassword_CorrectPassword_ReturnsSuccess()
    {
        const string password = "StrongPassword1!";
        string hash = _hasher.HashPassword(User, password);

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(User, hash, password);

        Assert.AreEqual(PasswordVerificationResult.Success, result);
    }

    [TestMethod]
    public void VerifyHashedPassword_WrongPassword_ReturnsFailed()
    {
        string hash = _hasher.HashPassword(User, "StrongPassword1!");

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(User, hash, "WrongPassword1!");

        Assert.AreEqual(PasswordVerificationResult.Failed, result);
    }

    [TestMethod]
    public void HashPassword_SamePasswordTwice_ReturnsDifferentHashes()
    {
        const string password = "StrongPassword1!";

        string firstHash = _hasher.HashPassword(User, password);
        string secondHash = _hasher.HashPassword(User, password);

        Assert.AreNotEqual(firstHash, secondHash);
    }

    [TestMethod]
    public void HashPassword_NullPassword_ThrowsArgumentNullException()
    {
        string password = null!;

        Assert.Throws<ArgumentNullException>(() => _hasher.HashPassword(User, password));
    }

    [TestMethod]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _hasher.HashPassword(User, string.Empty));
    }

    [TestMethod]
    public void HashPassword_WhitespacePassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _hasher.HashPassword(User, "   "));
    }
}
