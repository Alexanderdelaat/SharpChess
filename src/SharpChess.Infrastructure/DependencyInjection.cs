using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpChess.Application.Abstractions.Authentication;
using SharpChess.Application.Abstractions.Persistence;
using SharpChess.Application.Auth.Services;
using SharpChess.Infrastructure.Auth;
using SharpChess.Infrastructure.Identity;
using SharpChess.Infrastructure.Persistence;

namespace SharpChess.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SharpChessDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey) && options.SigningKey.Length >= 32,
                "JWT signing key moet minimaal 32 tekens bevatten.")
            .ValidateOnStart();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = PasswordRequirements.MinimumLength;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SharpChessDbContext>();

        services.AddScoped<IPasswordHasher<ApplicationUser>, BcryptPasswordHasher>();
        services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordValidator<ApplicationUser>, IdentityPasswordValidatorAdapter>();
        services.AddScoped<IUserRepository, IdentityUserRepository>();
        services.AddScoped<IPlayerRepository, IdentityPlayerRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
        services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();

        return services;
    }
}
