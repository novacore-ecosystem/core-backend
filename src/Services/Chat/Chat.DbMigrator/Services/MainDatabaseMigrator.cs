using NovaCore.Chat.Persistence.Engine;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Chat.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations against the main application database. Chat has no seeder
/// and no Hangfire storage.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await dbContext.Database.MigrateAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated");
    }
}
