namespace NovaCore.Promotion.Application.Abstractions.Persistence;

/// <summary>
/// Handles optimistic concurrency conflicts (xmin version mismatches) with automatic retry.
/// Wraps business logic that may encounter transient conflicts due to concurrent updates. Same
/// shape as Inventory.Application's own local helper of the same name (not shared via
/// BuildingBlock, so each service that needs it clones it) - used by RedeemCouponHandler.
/// </summary>
public sealed class OptimisticConcurrencyRetry(IAppLogger<OptimisticConcurrencyRetry> logger)
{
    private const int DefaultMaxRetries = 3;

    /// <summary>
    /// Executes operation with automatic retry on optimistic concurrency conflicts.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxRetries = DefaultMaxRetries,
        CancellationToken ct = default)
    {
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation(ct);
            }
            catch (ConflictException) when (attempt < maxRetries)
            {
                logger.Warning(
                    "Optimistic concurrency conflict detected (attempt {Attempt}/{Max}), retrying",
                    attempt, maxRetries);
            }
        }

        throw new ConflictException("Resource is being updated concurrently. Please retry.");
    }
}
