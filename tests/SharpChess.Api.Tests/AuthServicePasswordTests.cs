using FluentResults;
using SharpChess.Application.Abstractions.Authentication;
using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Auth;
using SharpChess.Application.Auth.Commands.Register;
using SharpChess.Application.Auth.Constants;
using SharpChess.Application.Auth.Models;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Api.Tests;

[TestClass]
public sealed class AuthServicePasswordTests
{
    [TestMethod]
    public async Task RegisterAsync_PasswordMismatch_ReturnsPasswordMismatchError()
    {
        RecordingUserRepository userRepository = new();
        AuthService sut = CreateSut(userRepository);

        Result<RegisterResult> result = await sut.RegisterAsync(
            "alex",
            "alex@example.com",
            "Abcdef1!Ghij",
            "Different1!Abc",
            CancellationToken.None);

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.AreEquivalent(
            new[] { AuthErrorCodes.PasswordMismatch },
            GetErrorMessages(result));
    }

    [TestMethod]
    public async Task RegisterAsync_InvalidPassword_ReturnsClearValidationErrors()
    {
        RecordingUserRepository userRepository = new();
        AuthService sut = CreateSut(userRepository);

        Result<RegisterResult> result = await sut.RegisterAsync(
            "alex",
            "alex@example.com",
            "lowercasepass!",
            "lowercasepass!",
            CancellationToken.None);

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.AreEquivalent(
            new[]
            {
                AuthErrorCodes.PasswordRequiresUppercase,
                AuthErrorCodes.PasswordRequiresDigit,
            },
            GetErrorMessages(result));
    }

    [TestMethod]
    public async Task RegisterAsync_InvalidPassword_DoesNotCreateUser()
    {
        RecordingUserRepository userRepository = new();
        AuthService sut = CreateSut(userRepository);

        await sut.RegisterAsync(
            "alex",
            "alex@example.com",
            "lowercasepass!",
            "lowercasepass!",
            CancellationToken.None);

        Assert.AreEqual(0, userRepository.CreateAsyncCallCount);
    }

    [TestMethod]
    public async Task RegisterAsync_ValidPassword_ReturnsResultWithoutPlainTextPassword()
    {
        const string password = "StrongPassword1!";
        RecordingUserRepository userRepository = new()
        {
            CreateResult = Result.Ok(new AuthenticatedUser(
                Id: "user-1",
                Username: "alex",
                Email: "alex@example.com",
                Role: ApplicationRoles.User)),
        };
        AuthService sut = CreateSut(userRepository);

        Result<RegisterResult> result = await sut.RegisterAsync(
            "alex",
            "alex@example.com",
            password,
            password,
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("user-1", result.Value.Id);
        Assert.AreEqual("alex", result.Value.Username);
        Assert.AreEqual("alex@example.com", result.Value.Email);
        Assert.IsFalse(
            typeof(RegisterResult).GetProperties()
                .Any(property => property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase)));
        Assert.IsFalse(result.Value.ToString()!.Contains(password, StringComparison.Ordinal));
    }

    private static AuthService CreateSut(IUserRepository userRepository)
    {
        return new AuthService(
            new UnexpectedAccessTokenService(),
            new PasswordValidator(),
            new UnexpectedRefreshTokenFactory(),
            new UnexpectedRefreshTokenRepository(),
            userRepository,
            TimeProvider.System);
    }

    private static string[] GetErrorMessages(ResultBase result)
    {
        return result.Errors.Select(error => error.Message).ToArray();
    }

    private sealed class RecordingUserRepository : IUserRepository
    {
        public int CreateAsyncCallCount { get; private set; }
        public Result<AuthenticatedUser> CreateResult { get; set; } = Result.Ok(new AuthenticatedUser(
            Id: "user-1",
            Username: "alex",
            Email: "alex@example.com",
            Role: ApplicationRoles.User));

        public Task<Result<AuthenticatedUser>> CreateAsync(
            string username,
            string email,
            string password,
            string role,
            CancellationToken cancellationToken)
        {
            CreateAsyncCallCount++;
            return Task.FromResult(CreateResult);
        }

        public Task<AuthenticatedUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("FindByIdAsync should not be called during registration password tests.");
        }

        public Task<AuthenticatedUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("FindByUsernameAsync should not be called during registration password tests.");
        }

        public Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("CheckPasswordAsync should not be called during registration password tests.");
        }

        public Task EnsureRoleExistsAsync(string role, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<Result> UpdateEmailAsync(string userId, string newEmail, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("UpdateEmailAsync should not be called during registration password tests.");
        }

        public Task<Result> UpdateUsernameAsync(string userId, string newUsername, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("UpdateUsernameAsync should not be called during registration password tests.");
        }

        public Task<Result> UpdatePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("UpdatePasswordAsync should not be called during registration password tests.");
        }
    }

    private sealed class UnexpectedAccessTokenService : IAccessTokenService
    {
        public IssuedAccessToken CreateToken(AuthenticatedUser user)
        {
            throw new AssertFailedException("CreateToken should not be called during registration password tests.");
        }
    }

    private sealed class UnexpectedRefreshTokenFactory : IRefreshTokenFactory
    {
        public GeneratedRefreshToken Create()
        {
            throw new AssertFailedException("Create should not be called during registration password tests.");
        }
    }

    private sealed class UnexpectedRefreshTokenRepository : IRefreshTokenRepository
    {
        public Task AddAsync(RefreshTokenRecord refreshToken, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("AddAsync should not be called during registration password tests.");
        }

        public Task<RefreshTokenRecord?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            throw new AssertFailedException("FindByTokenHashAsync should not be called during registration password tests.");
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new AssertFailedException("SaveChangesAsync should not be called during registration password tests.");
        }
    }
}
