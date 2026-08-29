using NovaCore.Notification.DbMigrator.Hosting;
using NovaCore.Notification.DbMigrator.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = ConfigurationEngine.BuildConfiguration(environmentName);

await using var provider = ConfigurationEngine.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Notification.DbMigrator starting (environment: {Environment})", environmentName);

    await HangfireDbProvisioner.RunAsync(configuration, logger);

    logger.LogInformation("[SUCCESS] Notification.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Notification.DbMigrator failed: {Message}", ex.Message);
    return 1;
}
