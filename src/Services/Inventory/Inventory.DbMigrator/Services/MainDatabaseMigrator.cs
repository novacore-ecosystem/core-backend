using NovaCore.Inventory.Persistence.Engine;
using NovaCore.Inventory.Persistence.Storage.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Inventory.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations, then runs InventorySeeder against the main application database.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await dbContext.Database.MigrateAsync();
        await new InventorySeeder(dbContext).SeedAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated and seeded");
    }
}
