using NovaCore.Promotion.Persistence.Engine;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Promotion.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations against the main application database. Promotion has no
/// seeder and no Hangfire storage.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
        await dbContext.Database.MigrateAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated");
    }
}
