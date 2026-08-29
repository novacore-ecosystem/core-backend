using NovaCore.Product.Persistence;
using NovaCore.Product.Persistence.Engine;
using NovaCore.Product.Persistence.Storage.Seeders;

using NovaCore.BuildingBlock.Persistence.Ef.Provisioning;

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
    logger.LogInformation("[INFO] Product.DbMigrator starting (environment: {Environment})", environmentName);

    await MigrateAndSeedMainDatabaseAsync(provider, logger);
    await ProvisionHangfireDatabaseAsync(configuration, logger);

    logger.LogInformation("[SUCCESS] Product.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Product.DbMigrator failed: {Message}", ex.Message);
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

// Applies pending EF Core migrations, then runs ProductSeeder against the main application database.
static async Task MigrateAndSeedMainDatabaseAsync(IServiceProvider provider, ILogger logger)
{
    logger.LogInformation("[INFO] Migrating Main DB...");

    await using var scope = provider.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await dbContext.Database.MigrateAsync();
    await new ProductSeeder(dbContext).SeedAsync();

    logger.LogInformation("[SUCCESS] Main DB migrated and seeded");
}

// Provisions the Hangfire storage database if it doesn't already exist.
static async Task ProvisionHangfireDatabaseAsync(IConfiguration configuration, ILogger logger)
{
    logger.LogInformation("[INFO] Checking Hangfire DB...");

    var hangfireConnectionString = configuration.GetConnectionString("Hangfire")
        ?? throw new InvalidOperationException("ConnectionStrings:Hangfire was not configured.");
    await DatabaseProvisioner.EnsureDatabaseExistsAsync(hangfireConnectionString, logger);

    logger.LogInformation("[SUCCESS] Hangfire DB ready");
}
