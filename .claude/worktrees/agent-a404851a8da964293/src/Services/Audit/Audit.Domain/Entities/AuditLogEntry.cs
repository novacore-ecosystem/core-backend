using NovaCore.BuildingBlock.Domain.Abstractions;

namespace NovaCore.Audit.Domain.Entities;

/// <summary>
/// One recorded Aggregate Root audit graph, consumed from the project's single
/// AuditIntegrationEvent. Immutable append-only record - no aggregate-root ceremony, no mutation
/// methods, same shape as Inventory's InventoryTransaction. RootEntityType/RootEntityId/Service
/// stay flat, top-level fields (indexed - see scripts/mongodb/init-mongo.js) even though they're
/// duplicated inside Root, so a query never needs to descend into the nested document just to
/// filter by aggregate identity or originating service.
/// </summary>
public sealed class AuditLogEntry : BaseEntity<Guid>
{
    public string RootEntityType { get; private set; } = string.Empty;
    public string RootEntityId { get; private set; } = string.Empty;
    public string Service { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public AuditTrailNode Root { get; private set; } = null!;
    public AuditTrailMetadata? Metadata { get; private set; }
    public DateTime Timestamp { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    private AuditLogEntry() { }

    public static AuditLogEntry Create(
        Guid id,
        string rootEntityType,
        string rootEntityId,
        string service,
        string correlationId,
        AuditTrailNode root,
        AuditTrailMetadata? metadata,
        DateTime timestamp)
    {
        return new AuditLogEntry
        {
            Id = id,
            RootEntityType = rootEntityType,
            RootEntityId = rootEntityId,
            Service = service,
            CorrelationId = correlationId,
            Root = root,
            Metadata = metadata,
            Timestamp = timestamp,
            ReceivedAt = DateTime.UtcNow,
        };
    }
}
