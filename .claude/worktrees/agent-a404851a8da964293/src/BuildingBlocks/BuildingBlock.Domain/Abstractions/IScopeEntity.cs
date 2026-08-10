namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Opt-in capability for entities owned by a Scope (Branch, Agency, Dealer, Region, ...) within a
/// Tenant. Implementing this interface is what makes the Entity Convention map/index/query-filter
/// ScopeId automatically - the query filter checks membership in the current request's
/// RequestContext.Current.ScopeIds (every Scope the user can access), not equality against a
/// single "current" Scope. Independent of ITenantEntity: an entity may implement one, the other,
/// both, or neither, depending on what actually owns it.
/// </summary>
public interface IScopeEntity
{
    Guid ScopeId { get; }

    /// <summary>Always called explicitly by the business code creating the entity - never
    /// automatically by an interceptor, unlike ITenantEntity.AssignTenant. A request's
    /// RequestContext carries every Scope the current user can access, not a single "current" one,
    /// so there is no ambient default to assign automatically; the caller must say which specific
    /// Scope the new entity belongs to. Should still be idempotent (no-op once already assigned),
    /// for consistency with every other capability's assignment method.</summary>
    void AssignScope(Guid scopeId);
}
