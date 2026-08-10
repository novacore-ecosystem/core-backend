using NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;
using NovaCore.Audit.Application.Abstractions.Services;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.Audit.Application.Features.AuditLogs.Queries.GetAuditLog;

public sealed class GetAuditLogHandler(
    IAuditLogReadService auditLogReadService,
    IUserClientService userClientService,
    IAppLogger<GetAuditLogHandler> logger) : IQueryHandler<GetAuditLogQuery, GetAuditLogResponse>
{
    public async Task<GetAuditLogResponse> Handle(GetAuditLogQuery request, CancellationToken ct = default)
    {
        var entry = await auditLogReadService.GetByIdAsync(request.AuditLogId, ct)
            ?? throw new NotFoundException("AuditLog", request.AuditLogId);

        var actorDisplayName = await TryResolveActorDisplayNameAsync(entry.Metadata?.Actor, ct);

        return new GetAuditLogResponse(
            entry.Id,
            entry.RootEntityType,
            entry.RootEntityId,
            entry.Service,
            entry.CorrelationId,
            MapNode(entry.Root),
            MapMetadata(entry.Metadata),
            entry.Timestamp,
            entry.ReceivedAt,
            actorDisplayName);
    }

    // Fail-open: an Actor that isn't a resolvable UserId (e.g. the username fallback
    // HttpAuditMetadataProvider uses when unauthenticated) or a transient User-service outage
    // must never fail this read - the audit entry itself is always returned regardless. Mirrors
    // Product Search's fail-open Inventory-enrichment pattern.
    private async Task<string?> TryResolveActorDisplayNameAsync(string? actor, CancellationToken ct)
    {
        if (!Guid.TryParse(actor, out var actorUserId))
            return null;

        try
        {
            var profile = await userClientService.GetActorAsync(actorUserId, ct);
            return profile?.DisplayName;
        }
        catch (Exception ex)
        {
            logger.Warning("Failed to resolve actor display name for UserId {ActorUserId} - continuing without it: {ErrorMessage}", actorUserId, ex.Message);
            return null;
        }
    }

    private static AuditNode MapNode(AuditTrailNode node)
    {
        return new AuditNode(
            node.NodeId,
            node.ParentNodeId,
            node.Depth,
            node.EntityType,
            node.EntityId,
            Enum.Parse<AuditAction>(node.Action),
            [.. node.Changes.Select(c => new AuditFieldChange(c.PropertyName, c.OldValue, c.NewValue))],
            [.. node.Children.Select(MapNode)]);
    }

    private static AuditMetadata? MapMetadata(AuditTrailMetadata? metadata)
    {
        if (metadata is null)
            return null;

        return new AuditMetadata
        {
            Actor = metadata.Actor,
            Service = metadata.Service,
            ClientIp = metadata.ClientIp,
            UserAgent = metadata.UserAgent,
            BusinessAction = metadata.BusinessAction,
            Reason = metadata.Reason,
            RequestPath = metadata.RequestPath,
            TraceId = metadata.TraceId,
        };
    }
}
