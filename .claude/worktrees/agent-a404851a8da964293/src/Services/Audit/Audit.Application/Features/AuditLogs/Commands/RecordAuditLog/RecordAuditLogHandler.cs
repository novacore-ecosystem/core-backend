using NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;

using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.Audit.Application.Features.AuditLogs.Commands.RecordAuditLog;

public sealed class RecordAuditLogHandler(
    IAuditLogWriteService auditLogWriteService) : ICommandHandler<RecordAuditLogCommand, RecordAuditLogResponse>
{
    public async Task<RecordAuditLogResponse> Handle(RecordAuditLogCommand request, CancellationToken ct = default)
    {
        var entry = AuditLogEntry.Create(
            Guid.CreateVersion7(),
            request.RootEntityType,
            request.RootEntityId,
            request.Service,
            request.CorrelationId,
            MapNode(request.Root),
            MapMetadata(request.Metadata),
            request.OccurredAt);

        await auditLogWriteService.AddAsync(entry, ct);

        return new RecordAuditLogResponse(entry.Id);
    }

    private static AuditTrailNode MapNode(AuditNode node)
    {
        return AuditTrailNode.Create(
            node.NodeId,
            node.ParentNodeId,
            node.Depth,
            node.EntityType,
            node.EntityId,
            node.Action.ToString(),
            [.. node.Changes.Select(c => AuditTrailFieldChange.Create(c.PropertyName, c.OldValue, c.NewValue))],
            [.. node.Children.Select(MapNode)]);
    }

    private static AuditTrailMetadata? MapMetadata(AuditMetadata? metadata)
    {
        if (metadata is null)
            return null;

        return AuditTrailMetadata.Create(
            metadata.Actor,
            metadata.Service,
            metadata.ClientIp,
            metadata.UserAgent,
            metadata.BusinessAction,
            metadata.Reason,
            metadata.RequestPath,
            metadata.TraceId);
    }
}
