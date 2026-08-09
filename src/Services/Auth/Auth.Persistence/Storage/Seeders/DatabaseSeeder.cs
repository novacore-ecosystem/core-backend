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
            await SeedPermissionCatalogAsync();
            await SeedRolePermissionsAsync();
            await SeedAccountsAsync();
            await SeedTenantClientsAsync();

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

    private async Task SeedPermissionCatalogAsync()
    {
        logger.LogInformation("Seeding permission catalog...");
        var permissionCatalogSeeder = new PermissionCatalogSeeder(context);
        await permissionCatalogSeeder.SeedAsync();
        logger.LogInformation("Permission catalog seeded successfully");
    }

    private async Task SeedRolePermissionsAsync()
    {
        logger.LogInformation("Seeding role permissions...");
        var rolePermissionSeeder = new RolePermissionSeeder(context);
        await rolePermissionSeeder.SeedAsync();
        logger.LogInformation("Role permissions seeded successfully");
    }

    private async Task SeedAccountsAsync()
    {
        logger.LogInformation("Seeding accounts...");
        var accountSeeder = new AccountSeeder(context, userManager);
        await accountSeeder.SeedAsync();
        logger.LogInformation("Accounts seeded successfully");
    }

    private async Task SeedTenantClientsAsync()
    {
        logger.LogInformation("Seeding tenant clients...");
        var tenantClientSeeder = new TenantClientSeeder(context);
        var rootClient = await tenantClientSeeder.SeedAsync();
        if (rootClient is not null)
        {
            logger.LogWarning(
                "Seeded Root TenantClient public key: {PublicKey}. Save this - it is not shown again and there is no API to retrieve it.",
                rootClient.PublicKey.Value);
        }
        logger.LogInformation("Tenant clients seeded successfully");
    }
}
