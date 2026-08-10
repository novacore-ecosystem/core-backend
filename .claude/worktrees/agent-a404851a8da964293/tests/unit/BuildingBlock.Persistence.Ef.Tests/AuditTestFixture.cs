using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Persistence.Ef.Tests;

internal static class AuditTestFixture
{
    /// <summary>Order -> OrderItem -> OrderTax, plus an unrelated User root - the same shape used throughout the audit-trail deliverables.</summary>
    public static IAuditHierarchyRegistry BuildRegistry()
    {
        var services = new ServiceCollection();

        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<TestOrder>().IsRoot(x => x.Id);
            builder.Entity<TestOrderItem>().BelongsTo<TestOrder>(x => x.OrderId);
            builder.Entity<TestOrderTax>().BelongsTo<TestOrderItem>(x => x.OrderItemId);
            builder.Entity<TestUser>().IsRoot(x => x.Id);
        });

        return services.BuildServiceProvider().GetRequiredService<IAuditHierarchyRegistry>();
    }

    public static TestDbContext CreateContext(IAuditHierarchyRegistry registry)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditInterceptor(registry, new NullAuditMetadataProvider()))
            .Options;

        return new TestDbContext(options);
    }
}
