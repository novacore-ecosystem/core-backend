using NovaCore.User.Persistence.Engine;
using NovaCore.User.Persistence.Storage.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.User.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations, then runs UserSeeder against the main application database.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await dbContext.Database.MigrateAsync();
        await new UserSeeder(dbContext).SeedAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated and seeded");
    }
}
