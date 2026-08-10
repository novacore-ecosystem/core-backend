#pragma warning disable CS1591

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Infrastructure.Logging;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Infrastructure.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection AddAppLogger(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        return services;
    }
}
