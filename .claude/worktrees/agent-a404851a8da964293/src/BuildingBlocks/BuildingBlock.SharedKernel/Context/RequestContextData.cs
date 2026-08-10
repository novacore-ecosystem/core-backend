namespace NovaCore.BuildingBlock.SharedKernel.Context;

/// <summary>
/// Immutable snapshot of the current request's identity, extracted once by
/// RequestContextMiddleware. Every field is either read straight off the JWT/headers or generated
/// when absent (CorrelationId) - nothing here is looked up lazily. Anonymous requests (login,
/// refresh token, health checks, public endpoints) simply carry null UserId/TenantId and an empty
/// ScopeIds.
///
/// ScopeIds is a collection, not a single value, because a user's token already carries every
/// Scope they're allowed to see - already expanded (including descendants) at token-issuance
/// time. Business services never compute a Scope hierarchy themselves; they only ever check
/// membership in this set (see docs/reference/tenant-convention.md's Scope Convention).
/// </summary>
public sealed class RequestContextData
{
    public Guid? UserId { get; init; }

    public Guid? TenantId { get; init; }

    public IReadOnlyCollection<Guid> ScopeIds { get; init; } = [];

    public required string CorrelationId { get; init; }

    public string? IdempotencyKey { get; init; }

    public string? Locale { get; init; }
}
