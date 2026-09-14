using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Dedicated, configuration-driven provisioning for the singleton Root account - replaces the
/// former SeedAuthData-based Root creation (the old AccountSeeder Root branch) and the former
/// per-permission RootPermissionGrantSeeder.
/// </summary>
/// <remarks>
/// <see cref="RootSetting.Id"/> is Root's sole identity - Username/Email/Password are
/// reconciled idempotently against it (create on a fresh install, update only changed fields
/// afterward), never used to detect which account is Root. Authorization comes from exactly one
/// direct PermissionGrant (ProviderName = User, ProviderKey = RootSetting.Id) for
/// Permissions.Root - never a Role/RoleGrant indirection (see Permissions.Common.cs's own
/// remarks) - so an account only needs that one grant for AccountAuthorizationGuard's HasRoot
/// check to bypass every rule. Requires PermissionCatalogSeeder to have already run (the
/// Permissions.Root definition must already exist).
/// </remarks>
public class RootAccountSeeder(
    AuthDbContext context,
    UserManager<Account> userManager,
    RootSetting rootSetting)
{
    public async Task SeedAsync()
    {
        await EnsureAccountAsync();
        await EnsureRootPermissionGrantAsync();
    }

    // ============================================================================
    // Account
    // Creates the Root account on a fresh install; on every later run, reconciles
    // only the fields that actually changed against RootSetting - never recreated,
    // never blindly overwritten.
    // ============================================================================

    #region Account

    private async Task EnsureAccountAsync()
    {
        var account = await userManager.FindByIdAsync(rootSetting.Id.ToString());

        if (account is null)
            await CreateAccountAsync();
        else
            await ReconcileAccountAsync(account);
    }

    private async Task CreateAccountAsync()
    {
        var account = Account.Create(rootSetting.Id, rootSetting.Username, Email.Create(rootSetting.Email));
        account.ConfirmEmail();

        // Display/ordering only - AccountAuthorizationGuard/HasRoot key off the direct
        // Permissions.Root grant, never Level (see Account.SetLevel's own doc comment).
        account.SetLevel(int.MaxValue);

        var result = await userManager.CreateAsync(account, rootSetting.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create the Root account: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    private async Task ReconcileAccountAsync(Account account)
    {
        if (!string.Equals(account.UserName, rootSetting.Username, StringComparison.Ordinal))
            await EnsureSucceededAsync(
                userManager.SetUserNameAsync(account, rootSetting.Username), "username");

        if (!string.Equals(account.Email, rootSetting.Email, StringComparison.Ordinal))
        {
            await EnsureSucceededAsync(
                userManager.SetEmailAsync(account, rootSetting.Email), "email");

            account.ConfirmEmail();
            await EnsureSucceededAsync(userManager.UpdateAsync(account), "email confirmation");
        }

        // The identity system's own comparison - never compare the configured plaintext against
        // the stored hash directly, and never reset the password unless it actually differs.
        var passwordMatches = await userManager.CheckPasswordAsync(account, rootSetting.Password);
        if (!passwordMatches)
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(account);
            await EnsureSucceededAsync(
                userManager.ResetPasswordAsync(account, resetToken, rootSetting.Password), "password");
        }
    }

    private static async Task EnsureSucceededAsync(Task<IdentityResult> resultTask, string fieldName)
    {
        var result = await resultTask;
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to update the Root account's {fieldName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    #endregion

    // ============================================================================
    // Permission grant (singleton enforcement)
    // The Root account holds exactly one direct PermissionGrant for Permissions.Root -
    // reconciled so the configured RootId is the only account that ever holds it; any
    // other account found holding it loses just that grant, not the account.
    // ============================================================================

    #region Permission grant (singleton enforcement)

    private async Task EnsureRootPermissionGrantAsync()
    {
        // Projected first (matches PermissionCatalogSeeder's proven Select(p => p.Key.Value)
        // shape) rather than filtered directly on p.Key.Value - EF cannot translate a WHERE
        // predicate that decomposes PermissionKey's converted column via a member access.
        var definitionsById = await context.PermissionDefinitions
            .Select(p => new { p.Id, Key = p.Key.Value })
            .ToListAsync();
        var rootDefinitionId = definitionsById.FirstOrDefault(p => p.Key == Permissions.Root)?.Id
            ?? throw new InvalidOperationException(
                $"The \"{Permissions.Root}\" permission definition must be seeded before Root account provisioning runs.");

        var rootProviderKey = rootSetting.Id.ToString();

        var existingGrants = await context.PermissionGrants
            .IgnoreQueryFilters()
            .Where(g => g.PermissionDefinitionId == rootDefinitionId && g.ProviderName == PermissionProviderName.User)
            .ToListAsync();

        if (!existingGrants.Any(g => g.ProviderKey == rootProviderKey))
            await context.PermissionGrants.AddAsync(
                PermissionGrant.Create(rootDefinitionId, PermissionProviderName.User, rootProviderKey));

        var staleGrants = existingGrants.Where(g => g.ProviderKey != rootProviderKey);
        context.PermissionGrants.RemoveRange(staleGrants);

        await context.SaveChangesAsync();
    }

    #endregion
}
