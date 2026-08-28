using NovaCore.Payment.Persistence;
using NovaCore.Payment.Persistence.Engine;
using NovaCore.Payment.Persistence.Storage.Seeders;

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

// AddPersistence registers PaymentDbContext (Npgsql + snake_case naming), outbox/inbox,
// repositories and PaymentSeeder itself (via AddSeeders) - the same wiring Payment.API used to
// call from ApplicationPipeline.UseApplication's SeedDatabase step. Payment has no Hangfire
// storage (unlike Auth/User/Content/Product/Inventory), so there's no DB-provisioning step here.
services.AddPersistence(configuration);

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("[INFO] Payment.DbMigrator starting (environment: {Environment})", environmentName);

    logger.LogInformation("[INFO] Migrating Main DB...");
    await using (var scope = provider.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<PaymentSeeder>();
        await seeder.SeedAsync();
    }
    logger.LogInformation("[SUCCESS] Main DB migrated and seeded");

    logger.LogInformation("[SUCCESS] Payment.DbMigrator completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "[FAILURE] Payment.DbMigrator failed: {Message}", ex.Message);
    return 1;
}
