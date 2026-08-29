using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Audit.DbMigrator.Hosting;

/// <summary>
/// Boots configuration and logging for the migrator's standalone features. Audit's own data lives
/// in MongoDB (schemaless, no seed data), so no persistence dependency graph is registered here.
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

    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));

        return services.BuildServiceProvider();
    }
}
