using System.Linq.Expressions;
using System.Reflection;

using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.SharedKernel.Context;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.DbContext;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies all IEntityTypeConfiguration implementations found in the given assembly. Entity
    /// Conventions (Tenant/Scope/SoftDelete/Idempotent - see ApplyEntityConventions) are applied
    /// separately, since they operate on the whole model rather than one assembly's configs.
    /// </summary>
    public static ModelBuilder ApplyPersistenceConfigurations(
        this ModelBuilder modelBuilder,
        Assembly assembly)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        return modelBuilder;
    }

    /// <summary>
    /// Applies OutboxConfiguration/InboxConfiguration when <paramref name="context"/> implements
    /// the corresponding marker interface (IOutboxDbContext/IInboxDbContext). Those configs live
    /// in this project's own Outbox/Inbox namespaces, so ApplyPersistenceConfigurations - scoped
    /// to the derived DbContext's own assembly - never finds them on its own. Every context that
    /// needs Outbox and/or Inbox support must apply them explicitly one way or another, whether
    /// or not it can inherit DbContextBase (e.g. AuthDbContext, which must inherit
    /// IdentityDbContext instead and calls this the same way DbContextBase does internally).
    /// </summary>
    public static ModelBuilder ApplyOutboxInboxConfiguration(
        this ModelBuilder modelBuilder,
        Microsoft.EntityFrameworkCore.DbContext context)
    {
        if (context is IOutboxDbContext)
            modelBuilder.ApplyConfiguration(new OutboxConfiguration());

        if (context is IInboxDbContext)
            modelBuilder.ApplyConfiguration(new InboxConfiguration());

        return modelBuilder;
    }

    /// <summary>
    /// The Entity Convention: scans every entity type in the model exactly once and applies the
    /// matching EF mapping/indexing/query-filtering for each capability interface it implements
    /// (ITenantEntity, IScopeEntity, ISoftDeleteEntity, IIdempotentEntity) - no entity, and no
    /// per-service IEntityTypeConfiguration, ever configures these by hand. An entity opts in
    /// purely by implementing the interface; it may implement any combination of them.
    ///
    /// TenantId is compared against NovaCore.BuildingBlock.SharedKernel.Context.RequestContext.
    /// Current - the ambient, request-scoped identity initialized once by RequestContextMiddleware
    /// - never a DI-resolved service and never the DbContext instance itself. Falls back to
    /// Guid.Empty when no tenant is present on the current request (anonymous requests,
    /// background jobs, before token issuance emits this claim), which is also what
    /// TenantAssignmentInterceptor assigns in that same situation - so the filter and the
    /// assignment always agree.
    ///
    /// ScopeId, by contrast, is checked for membership in RequestContext.Current.ScopeIds - a
    /// *collection* of every Scope the current user can access, already expanded (descendants
    /// included) by the token issuer - never a single "current scope" to compare equality
    /// against. EF Core translates `ScopeIds.Contains(e.ScopeId)` into `WHERE ScopeId IN (...)`.
    /// Because there is no single current Scope, there is no automatic ScopeId assignment for new
    /// entities either - see TenantAssignmentInterceptor and IScopeEntity.AssignScope.
    ///
    /// EF Core only allows one HasQueryFilter per entity type, so filters from every applicable
    /// capability are ANDed together into a single predicate per entity rather than calling
    /// HasQueryFilter once per capability (which would silently overwrite, not combine).
    /// </summary>
    public static ModelBuilder ApplyEntityConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");
            Expression? filter = null;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(ITenantEntity.TenantId));
                filter = Combine(filter, EqualTo(parameter, nameof(ITenantEntity.TenantId), TenantComparand.Body));
            }

            if (typeof(IScopeEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(IScopeEntity.ScopeId));
                filter = Combine(filter, ScopeIdsContains(parameter));
            }

            if (typeof(ISoftDeleteEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(ISoftDeleteEntity.IsDeleted));
                var isDeleted = Expression.Property(parameter, nameof(ISoftDeleteEntity.IsDeleted));
                filter = Combine(filter, Expression.Not(isDeleted));
            }

            if (typeof(IIdempotentEntity).IsAssignableFrom(clrType))
                modelBuilder.Entity(clrType).HasIndex(nameof(IIdempotentEntity.IdempotencyKey));

            if (filter is not null)
                modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(filter, parameter));
        }

        return modelBuilder;
    }

    private static readonly Expression<Func<Guid>> TenantComparand = () => RequestContext.Current.TenantId ?? Guid.Empty;
    private static readonly Expression<Func<IReadOnlyCollection<Guid>>> ScopeIdsComparand = () => RequestContext.Current.ScopeIds;

    private static readonly MethodInfo ScopeIdsContainsMethod = typeof(Enumerable)
        .GetMethods()
        .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
        .MakeGenericMethod(typeof(Guid));

    private static Expression EqualTo(ParameterExpression parameter, string propertyName, Expression comparand) =>
        Expression.Equal(Expression.Property(parameter, propertyName), comparand);

    /// <summary>`RequestContext.Current.ScopeIds.Contains(e.ScopeId)` - built via
    /// Enumerable.Contains rather than a instance-method call, since IReadOnlyCollection&lt;T&gt;
    /// itself has no Contains member.</summary>
    private static Expression ScopeIdsContains(ParameterExpression parameter)
    {
        var scopeId = Expression.Property(parameter, nameof(IScopeEntity.ScopeId));

        return Expression.Call(ScopeIdsContainsMethod, ScopeIdsComparand.Body, scopeId);
    }

    private static Expression Combine(Expression? left, Expression right) =>
        left is null ? right : Expression.AndAlso(left, right);
}
