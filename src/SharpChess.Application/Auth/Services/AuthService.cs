using FluentResults;
using SharpChess.Application.Abstractions.Authentication;
using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Auth.Constants;
using SharpChess.Application.Auth.Errors;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Application.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly IAuthPasswordValidator _passwordValidator;
    private readonly IRefreshTokenFactory _refreshTokenFactory;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly TimeProvider _timeProvider;

    public AuthService(
        IAccessTokenService accessTokenService,
        IAuthPasswordValidator passwordValidator,
        IRefreshTokenFactory refreshTokenFactory,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        TimeProvider timeProvider)
    {
        _accessTokenService = accessTokenService;
        _passwordValidator = passwordValidator;
        _refreshTokenFactory = refreshTokenFactory;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<RegisterResult>> RegisterAsync(
        string username,
        string email,
        string password,
        string confirmPassword,
        CancellationToken cancellationToken)
    {
        if (password != confirmPassword)
        {
            return Result.Fail(AuthErrorCodes.PasswordMismatch);
        }

        Result passwordValidationResult = _passwordValidator.Validate(password);

        if (passwordValidationResult.IsFailed)
        {
            return Result.Fail(passwordValidationResult.Errors);
        }

        await _userRepository.EnsureRoleExistsAsync(ApplicationRoles.User, cancellationToken);

        Result<AuthenticatedUser> userCreationResult = await _userRepository.CreateAsync(
            username,
            email,
            password,
            ApplicationRoles.User,
            cancellationToken);

        if (userCreationResult.IsFailed)
        {
            return Result.Fail(userCreationResult.Errors);
        }

        AuthenticatedUser user = userCreationResult.Value;

        return Result.Ok(new RegisterResult(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email));
    }

    public async Task<Result<AuthSessionResult>> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        AuthenticatedUser? user = await _userRepository.FindByUsernameAsync(username, cancellationToken);

        if (user is null)
        {
            return Result.Fail(AuthErrorCodes.InvalidCredentials);
        }

        bool passwordMatches = await _userRepository.CheckPasswordAsync(user.Id, password, cancellationToken);

        if (!passwordMatches)
        {
            return Result.Fail(AuthErrorCodes.InvalidCredentials);
        }

        return await IssueSessionAsync(user, cancellationToken);
    }

    public async Task<Result<AuthSessionResult>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Fail(AuthErrorCodes.InvalidRefreshToken);
        }

        string refreshTokenHash = RefreshTokenRecord.Hash(refreshToken);
        RefreshTokenRecord? storedRefreshToken = await _refreshTokenRepository.FindByTokenHashAsync(
            refreshTokenHash,
            cancellationToken);

        DateTime utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (storedRefreshToken is null || !storedRefreshToken.IsActive(utcNow))
        {
            return Result.Fail(AuthErrorCodes.InvalidRefreshToken);
        }

        AuthenticatedUser? user = await _userRepository.FindByIdAsync(storedRefreshToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Fail(AuthErrorCodes.UserNotFound);
        }

        Result<AuthSessionResult> newSessionResult = await IssueSessionAsync(user, cancellationToken);

        if (newSessionResult.IsFailed)
        {
            return newSessionResult;
        }

        storedRefreshToken.Revoke(utcNow, RefreshTokenRecord.Hash(newSessionResult.Value.RefreshToken));
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return newSessionResult;
    }

    public async Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        AuthenticatedUser? user = await _userRepository.FindByIdAsync(userId, cancellationToken);

        return user is null
            ? Result.Fail(AuthErrorCodes.UserNotFound)
            : Result.Ok(user);
    }

    public async Task LogoutAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        string refreshTokenHash = RefreshTokenRecord.Hash(refreshToken);
        RefreshTokenRecord? storedRefreshToken = await _refreshTokenRepository.FindByTokenHashAsync(
            refreshTokenHash,
            cancellationToken);

        if (storedRefreshToken is null || storedRefreshToken.UserId != userId || storedRefreshToken.IsRevoked)
        {
            return;
        }

        storedRefreshToken.Revoke(_timeProvider.GetUtcNow().UtcDateTime);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Result<AuthSessionResult>> IssueSessionAsync(
        AuthenticatedUser user,
        CancellationToken cancellationToken)
    {
        IssuedAccessToken accessToken = _accessTokenService.CreateToken(user);
        GeneratedRefreshToken refreshToken = _refreshTokenFactory.Create();

        RefreshTokenRecord refreshTokenEntity = RefreshTokenRecord.Create(
            user.Id,
            refreshToken.TokenHash,
            refreshToken.ExpiresAtUtc,
            _timeProvider.GetUtcNow().UtcDateTime);

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(new AuthSessionResult(
            AccessToken: accessToken.Token,
            AccessTokenExpiresAtUtc: accessToken.ExpiresAtUtc,
            RefreshToken: refreshToken.PlainTextToken,
            RefreshTokenExpiresAtUtc: refreshToken.ExpiresAtUtc,
            User: user));
    }
}
