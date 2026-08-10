using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;
using NovaCore.BuildingBlock.Saga.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;
using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Persistence.Contexts.ProductCatalogs.Read;
using NovaCore.Order.Persistence.Contexts.ProductCatalogs.Write;
using NovaCore.Order.Persistence.Contexts.Orders.Read;
using NovaCore.Order.Persistence.Contexts.Orders.Write;
using NovaCore.Order.Persistence.Engine;
using NovaCore.Order.Persistence.Engine.UnitOfWork;
using NovaCore.Order.Persistence.Reliability.Inbox;
using NovaCore.Order.Persistence.Reliability.Outbox;
using NovaCore.Order.Persistence.Reliability.Saga;

namespace NovaCore.Order.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
    {
        return builder.AddNpgsql();
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddApplicationServices()
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddAuditHierarchy()
            .AddSagaStore();

        return services;
    }

    // Singleton to match SagaOrchestrator's own lifetime (NovaCore.BuildingBlock.Saga.Extensions.SagaExtensions)
    // - EfSagaStore opens its own DI scope per call rather than taking OrderDbContext directly, see
    // its remarks.
    private static IServiceCollection AddSagaStore(this IServiceCollection services)
    {
        services.AddSingleton<ISagaStore, EfSagaStore>();
        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<OrderDbContext>(connectionString);

        return services;
    }

    // Order aggregate: Order is the root, OrderItem/OrderOwner belong to it via OrderId.
    // ProductCatalog is a separate local read-model (kept in sync from Product's events),
    // not part of this aggregate, so it isn't registered here.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<OrderEntity>().IsRoot(x => x.Id);
            builder.Entity<OrderItem>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
            builder.Entity<OrderOwner>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
            builder.Entity<OrderPrice>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
            builder.Entity<OrderPayment>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
            builder.Entity<OrderCancellation>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
            builder.Entity<OrderTax>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
            builder.Entity<OrderTagDefinition>().IsRoot(x => x.Id);
            builder.Entity<ReturnOrder>().IsRoot(x => x.Id);
            builder.Entity<ReturnItem>()
                .BelongsTo<ReturnOrder>(x => x.ReturnOrderId);
            builder.Entity<ReturnReason>().IsRoot(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(OrderDbContext));
        return services;
    }

    // OrderRepo/ProductCatalogRepo both implement the generic IRepository<T> - Scrutor's
    // AsImplementedInterfaces() registers each concrete class against every interface it
    // implements (including the empty per-aggregate marker interfaces), so this one scan call
    // covers both. Read/Write services are registered explicitly since they're one-per-aggregate.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(OrderDbContext));

        services.AddScoped<IOrderReadService, OrderReadService>();
        services.AddScoped<IOrderWriteService, OrderWriteService>();

        services.AddScoped<IProductCatalogReadService, ProductCatalogReadService>();
        services.AddScoped<IProductCatalogWriteService, ProductCatalogWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddEfOutboxStore<OrderDbContext>()
            .AddEfInboxStore<OrderDbContext>()
            .AddEfDeadLetterQueryService<OrderDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
