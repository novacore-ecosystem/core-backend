using NovaCore.Shipping.DbMigrator.Hosting;
using NovaCore.Shipping.DbMigrator.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = ConfigurationEngine.BuildConfiguration(environmentName);

await using var provider = ConfigurationEngine.BuildServiceProvider(configuration);
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Shipping.DbMigrator starting (environment: {Environment})", environmentName);

    await MainDatabaseMigrator.RunAsync(provider, logger);

    logger.LogInformation("[SUCCESS] Shipping.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Shipping.DbMigrator failed: {Message}", ex.Message);
    return 1;
}
