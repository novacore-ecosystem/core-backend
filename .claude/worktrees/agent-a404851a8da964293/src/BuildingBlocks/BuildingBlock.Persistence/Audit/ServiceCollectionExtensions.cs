using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Persistence.Audit;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers this service's audit hierarchy - which types are Aggregate Roots, which belong
    /// to which parent. Built once here and registered as a singleton <see cref="IAuditHierarchyRegistry"/>;
    /// call-order relative to <c>AddPersistenceDbContext</c> doesn't matter, since the change-tracking
    /// pipeline only reads the registry lazily, per SaveChanges, long after DI composition finishes.
    /// </summary>
    public static IServiceCollection ConfigureAuditHierarchy(
        this IServiceCollection services,
        Action<IAuditHierarchyBuilder> configure)
    {
        var builder = new AuditHierarchyBuilder();
        configure(builder);

        services.AddSingleton<IAuditHierarchyRegistry>(new AuditHierarchyRegistry(builder.Build()));

        return services;
    }
}
