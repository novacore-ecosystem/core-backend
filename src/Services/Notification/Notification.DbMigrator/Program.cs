using NovaCore.BuildingBlock.Persistence.Ef.Provisioning;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = BuildConfiguration(environmentName);

var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));

await using var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Notification.DbMigrator starting (environment: {Environment})", environmentName);

    // Notification's own data lives in MongoDB (schemaless, no seed data), so only Hangfire needs
    // provisioning here - see Notification.API/Program.cs for the Mongo side.
    await ProvisionHangfireDatabaseAsync(configuration, logger);

    logger.LogInformation("[SUCCESS] Notification.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Notification.DbMigrator failed: {Message}", ex.Message);
    return 1;
}

// Builds configuration from appsettings.json, an environment-specific override, and environment
// variables (Docker/Vault double-underscore convention), in that precedence order.
static IConfiguration BuildConfiguration(string environmentName) =>
    new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .Build();

// Provisions the Hangfire storage database if it doesn't already exist.
static async Task ProvisionHangfireDatabaseAsync(IConfiguration configuration, ILogger logger)
{
    logger.LogInformation("[INFO] Checking Hangfire DB...");

    var hangfireConnectionString = configuration.GetConnectionString("Hangfire")
        ?? throw new InvalidOperationException("ConnectionStrings:Hangfire was not configured.");
    await DatabaseProvisioner.EnsureDatabaseExistsAsync(hangfireConnectionString, logger);

    logger.LogInformation("[SUCCESS] Hangfire DB ready");
}
