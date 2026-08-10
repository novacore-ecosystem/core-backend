using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryCounts;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryLots;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryReservations;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventorySerials;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Persistence.Contexts.Inventories.Read;
using NovaCore.Inventory.Persistence.Contexts.Inventories.Write;
using NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Read;
using NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Repositories;
using NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Write;
using NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Read;
using NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Repositories;
using NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Write;
using NovaCore.Inventory.Persistence.Contexts.InventoryLots.Read;
using NovaCore.Inventory.Persistence.Contexts.InventoryLots.Repositories;
using NovaCore.Inventory.Persistence.Contexts.InventoryLots.Write;
using NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Read;
using NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Repositories;
using NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Write;
using NovaCore.Inventory.Persistence.Contexts.InventorySerials.Read;
using NovaCore.Inventory.Persistence.Contexts.InventorySerials.Repositories;
using NovaCore.Inventory.Persistence.Contexts.InventorySerials.Write;
using NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Read;
using NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;
using NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Write;
using NovaCore.Inventory.Persistence.Contexts.Warehouses.Read;
using NovaCore.Inventory.Persistence.Contexts.Warehouses.Write;
using NovaCore.Inventory.Persistence.Engine;
using NovaCore.Inventory.Persistence.Engine.UnitOfWork;
using NovaCore.Inventory.Persistence.Reliability.Inbox;
using NovaCore.Inventory.Persistence.Reliability.Outbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

namespace NovaCore.Inventory.Persistence;

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
            .AddAuditHierarchy();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<InventoryDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(InventoryDbContext));
        return services;
    }

    // Repositories are Scrutor-scanned via AddScopedByInterface for generic IRepository<>.
    // Specialized repositories and services are manually registered.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(InventoryDbContext));

        // Inventory
        services.AddScoped<IInventoryReadService, InventoryReadService>();
        services.AddScoped<IInventoryWriteService, InventoryWriteService>();

        // Warehouse
        services.AddScoped<IWarehouseReadService, WarehouseReadService>();
        services.AddScoped<IWarehouseWriteService, WarehouseWriteService>();

        // InventoryTransaction
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<IInventoryTransactionReadService, InventoryTransactionReadService>();
        services.AddScoped<IInventoryTransactionWriteService, InventoryTransactionWriteService>();

        // InventoryLot
        services.AddScoped<IInventoryLotRepository, InventoryLotRepository>();
        services.AddScoped<IInventoryLotReadService, InventoryLotReadService>();
        services.AddScoped<IInventoryLotWriteService, InventoryLotWriteService>();

        // InventoryReservation
        services.AddScoped<IInventoryReservationRepository, InventoryReservationRepository>();
        services.AddScoped<IInventoryReservationReadService, InventoryReservationReadService>();
        services.AddScoped<IInventoryReservationWriteService, InventoryReservationWriteService>();

        // InventorySerial
        services.AddScoped<IInventorySerialRepository, InventorySerialRepository>();
        services.AddScoped<IInventorySerialReadService, InventorySerialReadService>();
        services.AddScoped<IInventorySerialWriteService, InventorySerialWriteService>();

        // InventoryCount
        services.AddScoped<IInventoryCountRepository, InventoryCountRepository>();
        services.AddScoped<IInventoryCountReadService, InventoryCountReadService>();
        services.AddScoped<IInventoryCountWriteService, InventoryCountWriteService>();

        // InventoryDocument
        services.AddScoped<IInventoryDocumentRepository, InventoryDocumentRepository>();
        services.AddScoped<IInventoryDocumentReadService, InventoryDocumentReadService>();
        services.AddScoped<IInventoryDocumentWriteService, InventoryDocumentWriteService>();

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
            .AddEfOutboxStore<InventoryDbContext>()
            .AddEfInboxStore<InventoryDbContext>()
            .AddEfDeadLetterQueryService<InventoryDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    // InventoryStock (root), Warehouse (independent root - a physical location, not owned by
    // InventoryStock), InventoryTransaction (append-only log entry, belongs to the InventoryStock
    // record it happened against). InventoryLot/InventorySerial/InventoryReservation/
    // InventoryCount/InventoryDocument are all independent aggregates in their own right - they
    // reference InventoryStock/Warehouse ids but aren't owned by them. WarehouseZone,
    // InventoryCountItem and InventoryDocumentItem hold real business content (capacity/
    // environment settings, expected-vs-actual discrepancy quantities) an administrator would
    // expect change history for, so they're registered via BelongsTo despite being owned children.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<InventoryStock>().IsRoot(x => x.Id);
            builder.Entity<InventoryTransaction>()
                .BelongsTo<InventoryStock>(x => x.InventoryId);

            builder.Entity<Warehouse>().IsRoot(x => x.Id);
            builder.Entity<WarehouseZone>()
                .BelongsTo<Warehouse>(x => x.WarehouseId);

            builder.Entity<InventoryLot>().IsRoot(x => x.Id);
            builder.Entity<InventorySerial>().IsRoot(x => x.Id);
            builder.Entity<InventoryReservation>().IsRoot(x => x.Id);

            builder.Entity<InventoryCount>().IsRoot(x => x.Id);
            builder.Entity<InventoryCountItem>()
                .BelongsTo<InventoryCount>(x => x.InventoryCountId);

            builder.Entity<InventoryDocument>().IsRoot(x => x.Id);
            builder.Entity<InventoryDocumentItem>()
                .BelongsTo<InventoryDocument>(x => x.InventoryDocumentId);
        });

        return services;
    }
}
