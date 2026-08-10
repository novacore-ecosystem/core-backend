namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Opt-in capability for entities that should be soft-deleted instead of physically removed.
/// Implementing this interface makes the Entity Convention map IsDeleted/DeletedAt and register a
/// global `e => !e.IsDeleted` query filter automatically - no per-entity EF configuration
/// required. No entity implements this yet; the shape exists ahead of that work. Deliberately
/// independent of IAuditable/audit tracking, which remains its own concern.
/// </summary>
public interface ISoftDeleteEntity
{
    bool IsDeleted { get; }

    DateTime? DeletedAt { get; }

    /// <summary>Framework-only assignment point - must be idempotent, same reasoning as
    /// ITenantEntity.AssignTenant.</summary>
    void MarkDeleted();
}
