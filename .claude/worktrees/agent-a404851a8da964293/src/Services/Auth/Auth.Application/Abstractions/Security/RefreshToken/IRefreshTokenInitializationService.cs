namespace NovaCore.Auth.Application.Abstractions.Services;

/// <summary>
/// Initializes the refresh token cache on application startup.
/// Loads all active tokens from database into Redis cache for fast access.
/// </summary>
public interface IRefreshTokenInitializationService
{
    /// <summary>
    /// Initialize the refresh token cache on application startup.
    /// Loads all active (non-expired, non-revoked) tokens from database into Redis.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <remarks>
    /// Called once during application startup after database seeding.
    /// Ensures cache is populated before any requests arrive.
    /// </remarks>
    Task InitializeAsync(CancellationToken ct = default);
}
