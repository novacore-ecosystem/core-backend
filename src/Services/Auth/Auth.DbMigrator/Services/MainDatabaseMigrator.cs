using NovaCore.Auth.Persistence.Storage.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Auth.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations and seeds reference data (roles, permission catalog,
/// role-permissions, default admin account, tenant clients) on the main application database.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated and seeded");
    }
}
