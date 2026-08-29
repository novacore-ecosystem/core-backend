using NovaCore.Payment.Persistence;
using NovaCore.Payment.Persistence.Engine;
using NovaCore.Payment.Persistence.Storage.Seeders;

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
    logger.LogInformation("[INFO] Payment.DbMigrator starting (environment: {Environment})", environmentName);

    await MigrateAndSeedMainDatabaseAsync(provider, logger);

    logger.LogInformation("[SUCCESS] Payment.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Payment.DbMigrator failed: {Message}", ex.Message);
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

// Applies pending EF Core migrations, then runs PaymentSeeder (payment method/gateway reference
// catalog) against the main application database. No Hangfire storage for Payment.
static async Task MigrateAndSeedMainDatabaseAsync(IServiceProvider provider, ILogger logger)
{
    logger.LogInformation("[INFO] Migrating Main DB...");

    await using var scope = provider.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<PaymentSeeder>();
    await seeder.SeedAsync();

    logger.LogInformation("[SUCCESS] Main DB migrated and seeded");
}
