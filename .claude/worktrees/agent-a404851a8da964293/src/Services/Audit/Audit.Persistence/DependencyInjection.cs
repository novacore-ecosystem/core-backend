using NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;
using NovaCore.Audit.Persistence.Contexts.AuditLogs.Read;
using NovaCore.Audit.Persistence.Contexts.AuditLogs.Repositories;
using NovaCore.Audit.Persistence.Contexts.AuditLogs.Write;
using NovaCore.Audit.Persistence.Engine;
using NovaCore.Audit.Persistence.Engine.UnitOfWork;
using NovaCore.Audit.Persistence.Inbox;
using NovaCore.Audit.Persistence.Outbox;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Mongo.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Mongo.Inbox;
using NovaCore.BuildingBlock.Persistence.Mongo.Outbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Audit.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("Connection string 'Mongo' not found.");
        var databaseName = configuration["Mongo:DatabaseName"]
            ?? throw new InvalidOperationException("Configuration 'Mongo:DatabaseName' not found.");

        services.AddPersistenceMongoContext<AuditMongoContext>(connectionString, databaseName);

        return services;
    }

    // Audit logs are append-only with a single custom read shape (SearchAsync), so there is
    // no generic IRepository<T> to Scrutor-scan for here - same reasoning as Inventory's
    // IInventoryTransactionRepository, registered explicitly rather than discovered.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogRepository, AuditLogRepo>();
        services.AddScoped<IAuditLogReadService, AuditLogReadService>();
        services.AddScoped<IAuditLogWriteService, AuditLogWriteService>();
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
            .AddMongoOutboxStore<AuditMongoContext>()
            .AddMongoInboxStore<AuditMongoContext>()
            .AddMongoDeadLetterQueryService<AuditMongoContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
