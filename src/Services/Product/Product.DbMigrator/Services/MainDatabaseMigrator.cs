using NovaCore.Product.Persistence.Engine;
using NovaCore.Product.Persistence.Storage.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Product.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations, then runs ProductSeeder against the main application database.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await dbContext.Database.MigrateAsync();
        await new ProductSeeder(dbContext).SeedAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated and seeded");
    }
}
