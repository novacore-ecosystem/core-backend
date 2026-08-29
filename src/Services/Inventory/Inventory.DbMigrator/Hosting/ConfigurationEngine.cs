using NovaCore.Inventory.Persistence;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Inventory.DbMigrator.Hosting;

/// <summary>
/// Boots configuration, logging, and the persistence dependency graph that the migrator features run against.
/// </summary>
public static class ConfigurationEngine
{
    public static IConfiguration BuildConfiguration(string environmentName) =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

    public static ServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddPersistence(configuration);

        return services.BuildServiceProvider();
    }
}
