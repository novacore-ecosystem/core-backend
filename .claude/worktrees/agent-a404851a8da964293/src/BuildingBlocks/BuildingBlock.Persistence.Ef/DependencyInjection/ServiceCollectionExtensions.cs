using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Interceptors;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;
using NovaCore.BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<IServiceCollection, DbContextOptionsBuilder>? configure = null)
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        // Safe defaults so AuditInterceptor's DI dependencies always resolve, even for a service
        // that never calls ConfigureAuditHierarchy / registers a real IAuditMetadataProvider.
        // Both are plain-Add-wins-over-TryAdd-default, so registration order relative to
        // ConfigureAuditHierarchy (called separately, per service) doesn't matter.
        services.TryAddSingleton<IAuditHierarchyRegistry>(new AuditHierarchyRegistry(new Dictionary<Type, AuditEntityMetadata>()));
        services.TryAddScoped<IAuditMetadataProvider, NullAuditMetadataProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, AuditInterceptor>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, TimestampInterceptor>());

        // Entity Convention Tenant assignment - stateless, reads RequestContext.Current
        // directly (see TenantAssignmentInterceptor), so no DI-resolved request-identity service
        // is registered here at all.
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, TenantAssignmentInterceptor>());

        services.TryAddScoped(typeof(IRepository<>), typeof(GenericRepository<,>));
        services.TryAddScoped(typeof(IRepository<,>), typeof(EntityGenericRepository<,,>));

        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure();
            });

            options.UsePersistenceDefaults(serviceProvider);

            configure?.Invoke(services, options);
        });

        return services;
    }
}
