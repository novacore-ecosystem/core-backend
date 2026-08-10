using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

public class DatabaseSeeder(
    AuthDbContext context,
    UserManager<Account> userManager,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            logger.LogInformation("Starting database initialization...");

            await ApplyMigrationsAsync();
            await SeedRolesAsync();
            await SeedAccountsAsync();

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization");
            throw;
        }
    }

    private async Task ApplyMigrationsAsync()
    {
        logger.LogInformation("Applying pending migrations...");
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully");
        }
        else
        {
            logger.LogInformation("No pending migrations");
        }
    }

    private async Task SeedRolesAsync()
    {
        logger.LogInformation("Seeding roles...");
        var roleSeeder = new RoleSeeder(context);
        await roleSeeder.SeedAsync();
        logger.LogInformation("Roles seeded successfully");
    }

    private async Task SeedAccountsAsync()
    {
        logger.LogInformation("Seeding accounts...");
        var accountSeeder = new AccountSeeder(context, userManager);
        await accountSeeder.SeedAsync();
        logger.LogInformation("Accounts seeded successfully");
    }
}
