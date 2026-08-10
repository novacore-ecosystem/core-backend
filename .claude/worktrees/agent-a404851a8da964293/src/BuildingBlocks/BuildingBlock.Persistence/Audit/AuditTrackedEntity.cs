using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.BuildingBlock.Persistence.Audit;

/// <summary>
/// Provider-agnostic snapshot of one tracked, hierarchy-registered entity instance, as extracted
/// by a specific provider (EF today - see NovaCore.BuildingBlock.Persistence.Ef's AuditInterceptor). This
/// is the sole input to <see cref="AuditGraphBuilder"/>; a future Dapper/Mongo provider only
/// needs to produce this same shape to reuse the exact same graph-building logic.
/// </summary>
public sealed record AuditTrackedEntity(
    Type EntityType,
    string EntityId,
    string? ParentEntityId,
    bool HasChanges,
    AuditAction Action,
    IReadOnlyCollection<AuditFieldChange> Changes);
