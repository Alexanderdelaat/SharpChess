using Microsoft.Extensions.DependencyInjection;
using SharpChess.Application.Account.Services;
using SharpChess.Application.Auth.Services;
using SharpChess.Application.Players.Services;

namespace SharpChess.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthPasswordValidator, PasswordValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPlayerService, PlayerService>();

        return services;
    }
}
