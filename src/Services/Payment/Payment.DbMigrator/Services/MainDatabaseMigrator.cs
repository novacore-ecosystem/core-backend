using NovaCore.Payment.Persistence.Engine;
using NovaCore.Payment.Persistence.Storage.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.Payment.DbMigrator.Services;

/// <summary>
/// Applies pending EF Core migrations, then runs PaymentSeeder (payment method/gateway reference
/// catalog) against the main application database. No Hangfire storage for Payment.
/// </summary>
public static class MainDatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider provider, ILogger logger)
    {
        logger.LogInformation("[INFO] Migrating Main DB...");

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<PaymentSeeder>();
        await seeder.SeedAsync();

        logger.LogInformation("[SUCCESS] Main DB migrated and seeded");
    }
}
