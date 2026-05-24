using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SharpChess.Application.Account;
using SharpChess.Application.Auth.Services;
using SharpChess.Application.Players;

namespace SharpChess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IAuthPasswordValidator, PasswordValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPlayerService, PlayerService>();

        return services;
    }
}
