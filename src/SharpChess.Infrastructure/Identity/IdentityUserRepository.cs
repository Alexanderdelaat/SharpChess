using FluentResults;
using Microsoft.AspNetCore.Identity;
using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Auth;
using SharpChess.Application.Auth.Constants;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Infrastructure.Identity;

public sealed class IdentityUserRepository : IUserRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserRepository(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result<AuthenticatedUser>> CreateAsync(
        string username,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        ApplicationUser user = new()
        {
            UserName = username,
            Email = email,
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            return Result.Fail(MapIdentityErrors(createResult.Errors));
        }

        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(user, role);

        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result.Fail(MapIdentityErrors(addToRoleResult.Errors));
        }

        return Result.Ok(await MapAsync(user));
    }

    public async Task<AuthenticatedUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : await MapAsync(user);
    }

    public async Task<AuthenticatedUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByNameAsync(username);
        return user is null ? null : await MapAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task EnsureRoleExistsAsync(string role, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(role))
        {
            return;
        }

        await _roleManager.CreateAsync(new IdentityRole(role));
    }

    private async Task<AuthenticatedUser> MapAsync(ApplicationUser user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);
        string role = roles.FirstOrDefault() ?? ApplicationRoles.User;

        return new AuthenticatedUser(
            Id: user.Id,
            Username: user.UserName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Role: role);
    }

    private static List<Error> MapIdentityErrors(IEnumerable<IdentityError> errors)
    {
        return errors.Select(error => new Error(MapIdentityError(error))).ToList();
    }

    private static string MapIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            nameof(IdentityErrorDescriber.DuplicateEmail) => AuthErrorCodes.EmailAlreadyExists,
            nameof(IdentityErrorDescriber.DuplicateUserName) => AuthErrorCodes.UsernameAlreadyExists,
            nameof(IdentityErrorDescriber.InvalidEmail) => AuthErrorCodes.InvalidEmail,
            _ => error.Description,
        };
    }
}
