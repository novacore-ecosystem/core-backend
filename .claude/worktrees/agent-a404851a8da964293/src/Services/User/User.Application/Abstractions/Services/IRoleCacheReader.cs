namespace NovaCore.User.Application.Abstractions.Services;

/// <summary>
/// Read-only access to user roles cached by the Auth service. User service does not
/// own role data - it only reads whatever Auth has cached under the shared Roles
/// cache key. Returns an empty list (not an error) when nothing is cached.
/// </summary>
public interface IRoleCacheReader
{
    Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
}
