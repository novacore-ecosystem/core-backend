using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.Audit.Application.Features.AuditLogs.Commands.RecordAuditLog;

/// <summary>
/// Carries the Contract-shaped audit graph (AuditNode/AuditMetadata) as-is, rather than a third
/// parallel DTO layer - Application already depends on NovaCore.BuildingBlock.Contract (it's where
/// IIntegrationEvent itself lives), so this isn't a new dependency, just reusing it.
/// </summary>
public sealed record RecordAuditLogCommand(
    string RootEntityType,
    string RootEntityId,
    string Service,
    string CorrelationId,
    AuditNode Root,
    AuditMetadata? Metadata,
    DateTime OccurredAt) : ICommand<RecordAuditLogResponse>;

public sealed record RecordAuditLogResponse(Guid Id);
