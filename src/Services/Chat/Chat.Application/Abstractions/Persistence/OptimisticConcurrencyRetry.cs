using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Chat.Application.Abstractions.Persistence;

/// <summary>
/// Handles optimistic concurrency conflicts (xmin version mismatches) with automatic retry.
/// Wraps business logic that may encounter transient conflicts due to concurrent updates - e.g.
/// SendMessage's read-Conversation.LastMessageSequence-then-write race under concurrent sends.
/// Per-service utility (not shared kernel), mirrors Inventory.Application's own copy.
/// </summary>
public sealed class OptimisticConcurrencyRetry(IAppLogger<OptimisticConcurrencyRetry> logger)
{
    private const int DefaultMaxRetries = 3;

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
