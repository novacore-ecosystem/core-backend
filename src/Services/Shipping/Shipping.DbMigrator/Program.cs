using NovaCore.Shipping.Persistence;
using NovaCore.Shipping.Persistence.Engine;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = BuildConfiguration(environmentName);

var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddPersistence(configuration);

await using var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Shipping.DbMigrator starting (environment: {Environment})", environmentName);

    await MigrateMainDatabaseAsync(provider, logger);

    logger.LogInformation("[SUCCESS] Shipping.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Shipping.DbMigrator failed: {Message}", ex.Message);
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

// Applies pending EF Core migrations against the main application database. Shipping has no
// seeder and no Hangfire storage.
static async Task MigrateMainDatabaseAsync(IServiceProvider provider, ILogger logger)
{
    logger.LogInformation("[INFO] Migrating Main DB...");

    await using var scope = provider.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
    await dbContext.Database.MigrateAsync();

    logger.LogInformation("[SUCCESS] Main DB migrated");
}
