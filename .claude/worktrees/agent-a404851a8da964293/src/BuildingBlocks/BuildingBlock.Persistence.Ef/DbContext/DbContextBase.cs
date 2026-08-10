using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NovaCore.BuildingBlock.Persistence.Ef.DbContext;

/// <summary>
/// Shared base for every non-Identity EF DbContext in the solution. Centralizes the model/options
/// setup that would otherwise be duplicated in every service's DbContext: applying that context's
/// own IEntityTypeConfiguration classes, applying Outbox/Inbox configuration when applicable,
/// applying the Entity Convention (Tenant/Scope/SoftDelete/Idempotent), and suppressing the
/// "pending model changes" design-time warning.
///
/// Deliberately has no request-identity dependency of any kind - no ICurrentTenantService, no
/// HttpContext, no this.GetService() call for anything request-scoped. Request identity is read
/// exclusively from NovaCore.BuildingBlock.SharedKernel.Context.RequestContext.Current inside
/// the Entity Convention's query filters and TenantAssignmentInterceptor - never here. A DbContext
/// is a persistence component; it has no business knowing where TenantId or UserId come from.
///
/// AuthDbContext cannot inherit this - it must inherit IdentityDbContext instead - so it calls
/// the same ModelBuilderExtensions helpers this class uses, explicitly, from its own
/// OnConfiguring/OnModelCreating.
/// </summary>
public abstract class DbContextBase(DbContextOptions options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.SuppressPendingModelChangesWarning();
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyPersistenceConfigurations(GetType().Assembly);
        modelBuilder.ApplyOutboxInboxConfiguration(this);
        modelBuilder.ApplyEntityConventions();

        ConfigureModel(modelBuilder);
    }

    /// <summary>Hook for model configuration a derived context needs beyond assembly scanning + Outbox/Inbox.</summary>
    protected virtual void ConfigureModel(ModelBuilder modelBuilder)
    {
    }

    public bool IsDisableTimestamps { get; set; }
}
