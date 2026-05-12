using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IAuthPasswordValidator, PasswordValidator>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
