using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

// SetBasePath anchors resolution to the executable's own directory rather than the process's
// current working directory, which Docker's WORKDIR/ENTRYPOINT combination doesn't guarantee.
// AddEnvironmentVariables maps ConnectionStrings__Hangfire (Docker/Vault double underscore
// convention) onto the matching ConnectionStrings:Hangfire key AddJsonFile uses, and is added
// last so it wins over both JSON files.
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

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Audit.DbMigrator starting (environment: {Environment})", environmentName);

    // Audit's own log data lives in MongoDB (schemaless - no EF Core migrations, and no seed
    // data: audit logs are pure write-only event records). Its collections/indexes are created
    // by scripts/mongodb/init-mongo.js and AuditMongoContext's constructor, not here - see
    // Audit.API/Program.cs. Audit does, however, consume Hangfire (Postgres-backed) for
    // background jobs, so that database still needs the same first-deploy provisioning every
    // other Hangfire-consuming service's migrator does.
    logger.LogInformation("[INFO] Checking Hangfire DB...");
    var hangfireConnectionString = configuration.GetConnectionString("Hangfire")
        ?? throw new InvalidOperationException("ConnectionStrings:Hangfire was not configured.");
    await EnsureHangfireDatabaseExistsAsync(hangfireConnectionString, logger);
    logger.LogInformation("[SUCCESS] Hangfire DB ready");

    logger.LogInformation("[SUCCESS] Audit.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Audit.DbMigrator failed: {Message}", ex.Message);
    return 1;
}

// Hangfire.PostgreSql provisions its own tables/schema on first use but never creates the target
// database itself - on a first-time deployment the API crashes at startup because audit_hangfire_db
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
