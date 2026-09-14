using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Domain.Seeders;
using NovaCore.BuildingBlock.Domain.ValueObjects;

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
/// Role assignment (the "Root" Role), never a direct per-account PermissionGrant: Permissions.Root
/// is Role-provider only (see Permissions.Common.cs's own remarks - "Providers stays Role-only...
/// it is a provisioning/DB-seed-only concern"), and RoleGrantSeeder already grants the "Root" Role
/// the single Permissions.Root permission, so an account only needs that one Role membership for
/// AccountAuthorizationGuard's HasRoot check to bypass every rule. Requires RoleSeeder,
/// PermissionCatalogSeeder, and RoleGrantSeeder to have already run (the "Root" Role and its
/// Permissions.Root grant must already exist).
/// </remarks>
public class RootAccountSeeder(
    AuthDbContext context,
    UserManager<Account> userManager,
    IAccountRoleAssignmentService accountRoleAssignmentService,
    RootSetting rootSetting)
{
    public async Task SeedAsync()
    {
        var rootRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == SeedAuthData.Roles.Root)
            ?? throw new InvalidOperationException(
                $"The \"{SeedAuthData.Roles.Root}\" Role must be seeded before Root account provisioning runs.");

        await EnsureAccountAsync();
        await ReconcileRoleAssignmentAsync(rootRole.Id);
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

        // Display/ordering only - AccountAuthorizationGuard/HasRoot key off the Root Role's
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
    // Role assignment (singleton enforcement)
    // The Root account holds exactly one Role assignment (the "Root" Role) -
    // reconciled so the configured RootId is the only account that ever holds it;
    // any other account found holding it loses just that Role, not the account.
    // ============================================================================

    #region Role assignment (singleton enforcement)

    private async Task ReconcileRoleAssignmentAsync(Guid rootRoleId)
    {
        await accountRoleAssignmentService.ReplaceRolesAsync(rootSetting.Id, [rootRoleId]);

        var staleAccountIds = await accountRoleAssignmentService.GetAccountIdsInRoleAsync(rootRoleId);
        foreach (var accountId in staleAccountIds.Where(id => id != rootSetting.Id))
            await accountRoleAssignmentService.RemoveRoleAsync(accountId, rootRoleId);
    }

    #endregion
}
