using NovaCore.Shipping.Persistence.Engine;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Shipping.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations against the main application database. Shipping has no
/// seeder and no Hangfire storage.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
        await dbContext.Database.MigrateAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated");
    }
}
