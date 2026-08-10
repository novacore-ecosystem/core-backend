using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Web.CurrentUser;

public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor()
            .AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }

    public static IServiceCollection AddCurrentLocale(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor()
            .AddScoped<ICurrentLocaleService, CurrentLocaleService>();
        return services;
    }
}
