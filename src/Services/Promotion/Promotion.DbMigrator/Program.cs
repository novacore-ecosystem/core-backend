using NovaCore.Promotion.Persistence;
using NovaCore.Promotion.Persistence.Engine;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

// SetBasePath anchors resolution to the executable's own directory rather than the process's
// current working directory, which Docker's WORKDIR/ENTRYPOINT combination doesn't guarantee.
// AddEnvironmentVariables maps ConnectionStrings__DefaultConnection (Docker/Vault double
// underscore convention) onto the matching ConnectionStrings:DefaultConnection key AddJsonFile
// uses, and is added last so it wins over both JSON files.
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddLogging(logging => logging
    .AddConsole()
    .SetMinimumLevel(LogLevel.Information));

// AddPersistence registers PromotionDbContext (Npgsql + snake_case naming) plus outbox/inbox,
// repositories and the Elasticsearch client - the same wiring Promotion.API used to do inline.
// The Elasticsearch client is a lazily-constructed singleton (see BuildingBlock.Search), so
// registering it here doesn't require Elasticsearch:Url to be configured for the migrator; it's
// never resolved. Promotion has no seeder class (Storage/Seeders is empty), so only migrations
// run here. No Hangfire storage for Promotion either.
services.AddPersistence(configuration);

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Promotion.DbMigrator starting (environment: {Environment})", environmentName);

    logger.LogInformation("[INFO] Migrating Main DB...");
    await using (var scope = provider.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    logger.LogInformation("[SUCCESS] Main DB migrated");

    logger.LogInformation("[SUCCESS] Promotion.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Promotion.DbMigrator failed: {Message}", ex.Message);
    return 1;
}
