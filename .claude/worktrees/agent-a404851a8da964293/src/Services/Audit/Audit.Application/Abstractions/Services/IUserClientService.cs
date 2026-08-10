namespace NovaCore.Audit.Application.Abstractions.Services;

public sealed record ActorProfile(Guid UserId, string DisplayName);

/// <summary>
/// Thin gRPC client abstraction for enriching an audit entry's raw Actor (a UserId string) with
/// a human-readable display name for the Audit Trail UI - additive, read-only enrichment.
/// Audit's persisted AuditTrailMetadata.Actor is never changed by this; it stays whatever was
/// captured at write time. See docs/tasks/2026-07-28/Task15_first-grpc-consumer.md.
/// </summary>
public interface IUserClientService
{
    /// <summary>Null if the id doesn't resolve to a real user (fail-open - never blocks an audit-log read).</summary>
    Task<ActorProfile?> GetActorAsync(Guid userId, CancellationToken ct = default);
}
