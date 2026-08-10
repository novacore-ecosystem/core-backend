using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.SharedKernel.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NovaCore.BuildingBlock.Persistence.Ef.Interceptors;

/// <summary>
/// Automatic Entity Convention assignment for every Added entity that implements ITenantEntity -
/// the only place TenantId is ever set (see ITenantEntity.AssignTenant, which this is the sole
/// caller of). Reads NovaCore.BuildingBlock.SharedKernel.Context.RequestContext.Current directly -
/// no DI dependency - so it stays a plain, stateless interceptor. Falls back to Guid.Empty when
/// the current request carries no tenant (matches the Entity Convention's query filter default in
/// ModelBuilderExtensions, so assignment and filtering never disagree). AssignTenant is idempotent
/// by construction, so an entity that needs an explicit tenant at construction time (e.g. Scope
/// itself) is never overwritten here.
///
/// Deliberately does not assign IScopeEntity.ScopeId. A request's RequestContext carries every
/// Scope the current user can access (RequestContext.Current.ScopeIds), not a single "current"
/// one, so there is no valid default to assign a new entity automatically - unlike Tenant, where a
/// request genuinely belongs to exactly one. Any entity implementing IScopeEntity must have its
/// ScopeId assigned explicitly by the business code creating it, the same way Scope.Create already
/// assigns its own TenantId explicitly.
/// </summary>
public sealed class TenantAssignmentInterceptor : ISaveChangesInterceptor
{
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Assign(eventData.Context);

        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        Assign(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private static void Assign(Microsoft.EntityFrameworkCore.DbContext? context)
    {
        if (context is null)
            return;

        var current = RequestContext.Current;

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            if (entry.Entity is ITenantEntity tenantEntity)
                tenantEntity.AssignTenant(current.TenantId ?? Guid.Empty);
        }
    }
}
