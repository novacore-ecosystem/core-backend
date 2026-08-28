using System.Text.RegularExpressions;

using NovaCore.Product.Persistence;
using NovaCore.Product.Persistence.Engine;
using NovaCore.Product.Persistence.Storage.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

// SetBasePath anchors resolution to the executable's own directory rather than the process's
// current working directory, which Docker's WORKDIR/ENTRYPOINT combination doesn't guarantee.
// AddEnvironmentVariables maps ConnectionStrings__DefaultConnection / ConnectionStrings__Hangfire
// (Docker/Vault double underscore convention) onto the matching ConnectionStrings:* keys AddJsonFile
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

// AddPersistence registers ProductDbContext (Npgsql + snake_case naming) plus outbox/inbox,
// repositories and the Elasticsearch client - the same wiring Product.API used to do inline. The
// Elasticsearch client is a lazily-constructed singleton (see BuildingBlock.Search), so
// registering it here doesn't require Elasticsearch:Url to be configured for the migrator; it's
// never resolved.
services.AddPersistence(configuration);

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Product.DbMigrator starting (environment: {Environment})", environmentName);

    logger.LogInformation("[INFO] Migrating Main DB...");
    await using (var scope = provider.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await dbContext.Database.MigrateAsync();

        // ProductSeeder isn't DI-registered elsewhere (Product.API never seeded); it only depends
        // on ProductDbContext, so it's constructed directly rather than adding a registration
        // just for this.
        await new ProductSeeder(dbContext).SeedAsync();
    }
    logger.LogInformation("[SUCCESS] Main DB migrated and seeded");

    logger.LogInformation("[INFO] Checking Hangfire DB...");
    var hangfireConnectionString = configuration.GetConnectionString("Hangfire")
        ?? throw new InvalidOperationException("ConnectionStrings:Hangfire was not configured.");
    await EnsureHangfireDatabaseExistsAsync(hangfireConnectionString, logger);
    logger.LogInformation("[SUCCESS] Hangfire DB ready");

    logger.LogInformation("[SUCCESS] Product.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Product.DbMigrator failed: {Message}", ex.Message);
    return 1;
}

// Hangfire.PostgreSql provisions its own tables/schema on first use but never creates the target
// database itself - on a first-time deployment the API crashes at startup because product_hangfire_db
// doesn't exist yet. CREATE DATABASE can't run against the database being created, so this connects
// to the server's always-present 'postgres' maintenance database to check/create the real target.
static async Task EnsureHangfireDatabaseExistsAsync(string hangfireConnectionString, ILogger logger)
{
    var targetBuilder = new NpgsqlConnectionStringBuilder(hangfireConnectionString);
    var databaseName = targetBuilder.Database;

    if (string.IsNullOrWhiteSpace(databaseName))
    {
        throw new InvalidOperationException("ConnectionStrings:Hangfire has no Database specified.");
    }

    // CREATE DATABASE can't be parameterized, so the identifier is validated here and quoted
    // below to guard against injection from a malformed connection string.
    if (!Regex.IsMatch(databaseName, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
    {
        throw new InvalidOperationException(
            $"Hangfire database name '{databaseName}' is not a safe identifier.");
    }

    var maintenanceBuilder = new NpgsqlConnectionStringBuilder(hangfireConnectionString)
    {
        Database = "postgres"
    };

    await using var connection = new NpgsqlConnection(maintenanceBuilder.ConnectionString);
    await connection.OpenAsync();

    await using (var existsCommand = new NpgsqlCommand(
        "SELECT 1 FROM pg_database WHERE datname = @name", connection))
    {
        existsCommand.Parameters.AddWithValue("name", databaseName);

        if (await existsCommand.ExecuteScalarAsync() is not null)
        {
            logger.LogInformation("[INFO] Hangfire database '{Database}' already exists", databaseName);
            return;
        }
    }

    logger.LogInformation("[INFO] Hangfire database '{Database}' not found, creating...", databaseName);

    await using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
    await createCommand.ExecuteNonQueryAsync();

    logger.LogInformation("[INFO] Hangfire database '{Database}' created", databaseName);
}
