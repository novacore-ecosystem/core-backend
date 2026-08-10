using NovaCore.Auth.Application.Abstractions.Auth;

namespace NovaCore.Auth.Infrastructure.Caching;

/// <summary>
/// Decorator for IAuthService that adds transparent caching for role-related queries.
/// Follows the decorator pattern to maintain the same interface without intrusive changes.
/// </summary>
public sealed class CachedAuthServiceDecorator(
    IAuthService innerAuthService,
    RoleCacheService roleCacheService) : IAuthService
{
    /// <summary>Gets or creates cached user roles.</summary>
    public async Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var cached = await roleCacheService.GetAsync(userId, ct);
        if (cached != null)
            return cached;

        var roles = await innerAuthService.GetUserRolesAsync(userId, ct);
        await roleCacheService.SetAsync(userId, roles, ct);

        return roles;
    }

    /// <summary>Checks if user has role using cached data when available.</summary>
    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var roles = await GetUserRolesAsync(userId, ct);
        return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Pass-through to inner service (no caching for user lookups yet).</summary>
    public async Task<Account?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await innerAuthService.GetUserByIdAsync(userId, ct);
    }

    /// <summary>Pass-through to inner service (no caching for email lookups yet).</summary>
    public async Task<Account?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await innerAuthService.GetUserByEmailAsync(email, ct);
    }

    /// <summary>Pass-through to inner service (no caching for credentials validation).</summary>
    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        return await innerAuthService.ValidateCredentialsAsync(email, password, ct);
    }

    /// <summary>Delegates to inner service and invalidates cache for new user.</summary>
    public async Task<Account?> CreateUserAsync(string email, string username, string password, CancellationToken ct = default)
    {
        var user = await innerAuthService.CreateUserAsync(email, username, password, ct);
        // No roles to cache for new user, but infrastructure is ready
        return user;
    }

    /// <summary>Pass-through to inner service (no roles to cache for a brand new account).</summary>
    public async Task<Account?> CreateUserAsync(Guid id, string email, string username, string password, CancellationToken ct = default)
    {
        return await innerAuthService.CreateUserAsync(id, email, username, password, ct);
    }

    /// <summary>Pass-through to inner service.</summary>
    public async Task<bool> UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        return await innerAuthService.UpdatePasswordAsync(userId, currentPassword, newPassword, ct);
    }

    /// <summary>Pass-through to inner service.</summary>
    public async Task<bool> ConfirmEmailAsync(Guid userId, CancellationToken ct = default)
    {
        return await innerAuthService.ConfirmEmailAsync(userId, ct);
    }

    /// <summary>Delegates to inner service and invalidates user's cached roles.</summary>
    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await innerAuthService.DeleteUserAsync(userId, ct);
        if (result)
            await roleCacheService.RemoveAsync(userId, ct);
        return result;
    }

    /// <summary>Delegates to inner service and invalidates cached roles so the next read picks up the new role.</summary>
    public async Task<bool> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var result = await innerAuthService.AssignRoleAsync(userId, role, ct);
        if (result)
            await roleCacheService.RemoveAsync(userId, ct);
        return result;
    }
}
