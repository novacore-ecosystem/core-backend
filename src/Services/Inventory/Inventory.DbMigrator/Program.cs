using NovaCore.Inventory.DbMigrator.Hosting;
using NovaCore.Inventory.DbMigrator.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = ConfigurationEngine.BuildConfiguration(environmentName);

await using var provider = ConfigurationEngine.BuildServiceProvider(configuration);
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Inventory.DbMigrator starting (environment: {Environment})", environmentName);

    await MainDatabaseMigrator.RunAsync(provider, logger);
    await HangfireDbProvisioner.RunAsync(configuration, logger);

    logger.LogInformation("[SUCCESS] Inventory.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Inventory.DbMigrator failed: {Message}", ex.Message);
    return 1;
}
