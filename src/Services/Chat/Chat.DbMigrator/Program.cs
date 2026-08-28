using NovaCore.Chat.Persistence;
using NovaCore.Chat.Persistence.Engine;

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

// AddPersistence registers ChatDbContext (Npgsql + snake_case naming) plus outbox/inbox and
// repositories - the same wiring Chat.API used to do inline. Chat has no seeder class and no
// Hangfire storage, so only migrations run here.
services.AddPersistence(configuration);

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Chat.DbMigrator starting (environment: {Environment})", environmentName);

    logger.LogInformation("[INFO] Migrating Main DB...");
    await using (var scope = provider.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    logger.LogInformation("[SUCCESS] Main DB migrated");

    logger.LogInformation("[SUCCESS] Chat.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Chat.DbMigrator failed: {Message}", ex.Message);
    return 1;
}
