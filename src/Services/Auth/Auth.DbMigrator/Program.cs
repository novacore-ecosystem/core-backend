using System.Text.RegularExpressions;

using NovaCore.Auth.Persistence;
using NovaCore.Auth.Persistence.Storage.Seeders;

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

// AddPersistence registers AuthDbContext (Npgsql + snake_case naming), ASP.NET Core Identity
// (UserManager<Account>/RoleManager<Role>) and DatabaseSeeder itself - the same wiring
// Auth.API used to do inline. DatabaseSeeder.SeedAsync applies pending migrations
// (context.Database.MigrateAsync) and then seeds roles, permission catalog, role-permissions,
// a default admin account (idempotent - only if Users is empty) and tenant clients.
services.AddPersistence(configuration);

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Auth.DbMigrator starting (environment: {Environment})", environmentName);

    logger.LogInformation("[INFO] Migrating Main DB...");
    await using (var scope = provider.CreateAsyncScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    logger.LogInformation("[SUCCESS] Main DB migrated and seeded");

    logger.LogInformation("[INFO] Checking Hangfire DB...");
    var hangfireConnectionString = configuration.GetConnectionString("Hangfire")
        ?? throw new InvalidOperationException("ConnectionStrings:Hangfire was not configured.");
    await EnsureHangfireDatabaseExistsAsync(hangfireConnectionString, logger);
    logger.LogInformation("[SUCCESS] Hangfire DB ready");

    logger.LogInformation("[SUCCESS] Auth.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Auth.DbMigrator failed: {Message}", ex.Message);
    return 1;
}

// Hangfire.PostgreSql provisions its own tables/schema on first use but never creates the target
// database itself - on a first-time deployment the API crashes at startup because auth_hangfire_db
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
