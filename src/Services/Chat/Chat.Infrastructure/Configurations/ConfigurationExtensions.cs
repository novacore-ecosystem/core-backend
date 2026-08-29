using System.Reflection;

using NovaCore.BuildingBlock.Infrastructure.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Chat.Infrastructure.Configurations;

/// <summary>Binds and validates every local ISetting in this assembly.</summary>
public static class ConfigurationExtensions
{
    public static IServiceCollection AddChatConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSettings(configuration, Assembly.GetExecutingAssembly());
        return services;
    }
}
