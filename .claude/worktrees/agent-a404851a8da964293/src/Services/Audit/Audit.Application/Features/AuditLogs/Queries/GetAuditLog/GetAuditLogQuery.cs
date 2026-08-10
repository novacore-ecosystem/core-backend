using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.Audit.Application.Features.AuditLogs.Queries.GetAuditLog;

public sealed record GetAuditLogQuery(Guid AuditLogId) : IQuery<GetAuditLogResponse>;

/// <summary>Reuses the Contract-shaped AuditNode/AuditMetadata for the response too - symmetric with RecordAuditLogCommand's input shape, avoids a third parallel tree type just for reads.</summary>
public sealed record GetAuditLogResponse(
    Guid Id,
    string RootEntityType,
    string RootEntityId,
    string Service,
    string CorrelationId,
    AuditNode Root,
    AuditMetadata? Metadata,
    DateTime Timestamp,
    DateTime ReceivedAt,
    /// <summary>
    /// Display-time-only enrichment of Metadata.Actor via User's gRPC GetUser - never persisted,
    /// never part of the shared AuditMetadata contract (which stays write-side/publisher-neutral).
    /// Null whenever Actor isn't a resolvable UserId or the lookup fails - fail-open, never blocks
    /// this read. See docs/tasks/2026-07-28/Task15_first-grpc-consumer.md.
    /// </summary>
    string? ActorDisplayName = null);
