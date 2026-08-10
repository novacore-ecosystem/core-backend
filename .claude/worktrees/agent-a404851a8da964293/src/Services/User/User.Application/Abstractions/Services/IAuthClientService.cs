namespace NovaCore.User.Application.Abstractions.Services;

/// <summary>
/// Validates email existence against the Auth service via gRPC.
/// </summary>
public interface IAuthClientService
{
    /// <summary>
    /// Checks if an email already exists in the Auth service.
    /// </summary>
    /// <returns>True if email exists in Auth; otherwise false.</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Get user roles by user id, refreshing cache if not exists
    /// </summary>
    /// <returns>User roles list</returns>
    Task<string[]> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
}
