using NovaCore.BuildingBlock.Domain.Attributes;

namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Base type for every entity in the platform. Carries only what every entity genuinely needs
/// (timestamps) - ownership/lifecycle capabilities (Tenant, Scope, SoftDelete, Idempotent) are
/// deliberately not here. An entity opts into each one individually via the matching capability
/// interface (ITenantEntity, IScopeEntity, ...); the Entity Convention (see
/// NovaCore.BuildingBlock.Persistence.Ef) detects those interfaces during model creation and
/// applies the required EF mapping/indexing/query-filtering automatically, so no entity ever
/// hand-configures them.
/// </summary>
public abstract class BaseEntity : IEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [AuditIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Track()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public abstract class BaseEntity<T> : BaseEntity, IEntity<T>
{
    public T Id { get; init; } = default!;
}
