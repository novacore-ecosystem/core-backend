using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Domain.Seeders;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

public class AccountSeeder(AuthDbContext context, UserManager<Account> userManager, IOptions<RootSetting> rootSetting)
{
    public async Task SeedAsync()
    {
        if (await context.Users.AnyAsync())
            return;

        foreach (var (defaultId, username, email, password, roles) in SeedAuthData.Accounts.Default)
        {
            // Root's id is configuration-driven (RootSetting.RootId); every other seeded
            // account (none exist today, but SeedAuthData.Accounts.Default may grow) keeps its
            // catalog id as-is.
            var id = roles.Contains(SeedAuthData.Roles.Root) ? rootSetting.Value.RootId : defaultId;

            var account = Account.Create(username, Email.Create(email), AccountStatus.Active);
            account.Id = id;
            account.ConfirmEmail();

            // Root's Level is set for display/ordering consistency only - target-scope enforcement
            // (AccountAuthorizationGuard) never keys off Level for Root, it keys off the
            // Permissions.Root claim, so this value is never itself a bypass mechanism.
            if (roles.Contains(SeedAuthData.Roles.Root))
                account.SetLevel(int.MaxValue);

            var result = await userManager.CreateAsync(account, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create account '{username}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            foreach (var role in roles)
            {
                var roleResult = await userManager.AddToRoleAsync(account, role);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to assign role '{role}' to account '{username}'"
                    );
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
