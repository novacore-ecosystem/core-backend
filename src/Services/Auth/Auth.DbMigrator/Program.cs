using NovaCore.Auth.DbMigrator.Hosting;
using NovaCore.Auth.DbMigrator.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = ConfigurationEngine.BuildConfiguration(environmentName);

await using var provider = ConfigurationEngine.BuildServiceProvider(configuration);
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Auth.DbMigrator starting (environment: {Environment})", environmentName);

    await MainDatabaseMigrator.RunAsync(provider, logger);
    await HangfireDbProvisioner.RunAsync(configuration, logger);

    logger.LogInformation("[SUCCESS] Auth.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Auth.DbMigrator failed: {Message}", ex.Message);
    return 1;
}
