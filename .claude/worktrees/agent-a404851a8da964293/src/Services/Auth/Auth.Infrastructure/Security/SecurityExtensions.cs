using NovaCore.Auth.Infrastructure.Security.Jwt;
using NovaCore.Auth.Infrastructure.Security.RefreshTokens;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Auth.Infrastructure.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services
            .AddJwtServices()
            .AddRefreshTokens();

        return services;
    }
}
