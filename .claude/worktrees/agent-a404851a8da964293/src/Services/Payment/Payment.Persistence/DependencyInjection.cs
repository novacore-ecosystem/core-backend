using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.Payment.Application.Abstractions.Persistence.Payments;
using NovaCore.Payment.Application.Abstractions.Persistence.PaymentIntents;
using NovaCore.Payment.Application.Abstractions.Persistence.Refunds;
using NovaCore.Payment.Persistence.Contexts.Payments.Read;
using NovaCore.Payment.Persistence.Contexts.Payments.Write;
using NovaCore.Payment.Persistence.Contexts.PaymentIntents.Read;
using NovaCore.Payment.Persistence.Contexts.PaymentIntents.Write;
using NovaCore.Payment.Persistence.Contexts.Refunds.Read;
using NovaCore.Payment.Persistence.Contexts.Refunds.Write;
using NovaCore.Payment.Persistence.Engine;
using NovaCore.Payment.Persistence.Engine.UnitOfWork;
using NovaCore.Payment.Persistence.Reliability.Inbox;
using NovaCore.Payment.Persistence.Reliability.Outbox;
using NovaCore.Payment.Persistence.Storage.Seeders;

namespace NovaCore.Payment.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
        => builder.AddNpgsql();

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddApplicationServices()
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddAuditHierarchy()
            .AddSeeders();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<PaymentDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(PaymentDbContext));
        return services;
    }

    // PaymentRepo/PaymentIntentRepo/RefundRepo all implement the generic IRepository<T> -
    // Scrutor's AsImplementedInterfaces() registers each concrete class against every interface
    // it implements, so this one scan call covers all three. Read/Write services are registered
    // explicitly since they're one-per-aggregate. Only the core lifecycle slice (Payment,
    // PaymentIntent, Refund) has a repository/Read/Write service this phase - every other
    // aggregate has an EF configuration and DbSet but no CQRS yet, so no repository is needed
    // until its own CQRS work begins (see docs/services/payment-service.md, Planned phases).
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(PaymentDbContext));

        services.AddScoped<IPaymentReadService, PaymentReadService>();
        services.AddScoped<IPaymentWriteService, PaymentWriteService>();

        services.AddScoped<IPaymentIntentReadService, PaymentIntentReadService>();
        services.AddScoped<IPaymentIntentWriteService, PaymentIntentWriteService>();

        services.AddScoped<IRefundReadService, RefundReadService>();
        services.AddScoped<IRefundWriteService, RefundWriteService>();

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
            .AddEfOutboxStore<PaymentDbContext>()
            .AddEfInboxStore<PaymentDbContext>()
            .AddEfDeadLetterQueryService<PaymentDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    // Every aggregate with IAuditable gets a hierarchy entry so the AuditInterceptor can group
    // its changes under the right root, even before it has a repository/CQRS surface - cheap and
    // declarative, keeps the audit trail complete as each aggregate's write path is built out in
    // later phases.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            // Core lifecycle
            builder.Entity<PaymentIntent>().IsRoot(x => x.Id);
            builder.Entity<PaymentEntity>().IsRoot(x => x.Id);
            builder.Entity<PaymentItem>().BelongsTo<PaymentEntity>(x => x.PaymentId);
            builder.Entity<PaymentAttempt>().BelongsTo<PaymentEntity>(x => x.PaymentId);
            builder.Entity<Refund>().IsRoot(x => x.Id);
            builder.Entity<RefundAttempt>().BelongsTo<Refund>(x => x.RefundId);

            // Catalog
            builder.Entity<PaymentMethod>().IsRoot(x => x.Id);
            builder.Entity<PaymentGateway>().IsRoot(x => x.Id);
            builder.Entity<GatewayConfiguration>().BelongsTo<PaymentGateway>(x => x.GatewayId);
            builder.Entity<GatewayStatusMapping>().BelongsTo<PaymentGateway>(x => x.GatewayId);

            // Accounts & billing
            builder.Entity<PaymentAccount>().IsRoot(x => x.Id);
            builder.Entity<PaymentToken>().BelongsTo<PaymentAccount>(x => x.PaymentAccountId);
            builder.Entity<BillingProfile>().IsRoot(x => x.Id);
            builder.Entity<Invoice>().IsRoot(x => x.Id);

            // Operations
            builder.Entity<Settlement>().IsRoot(x => x.Id);
            builder.Entity<Reconciliation>().IsRoot(x => x.Id);
            builder.Entity<Payout>().IsRoot(x => x.Id);
            builder.Entity<PaymentFee>().IsRoot(x => x.Id);
            builder.Entity<ExchangeRate>().IsRoot(x => x.Id);
            builder.Entity<PaymentEventLog>().IsRoot(x => x.Id);
            builder.Entity<PaymentAudit>().IsRoot(x => x.Id);
            builder.Entity<IdempotencyRecord>().IsRoot(x => x.Id);

            // Webhooks
            builder.Entity<WebhookEvent>().IsRoot(x => x.Id);
            builder.Entity<WebhookDelivery>().IsRoot(x => x.Id);

            // Scheduling & notifications
            builder.Entity<PaymentSession>().IsRoot(x => x.Id);
            builder.Entity<ScheduledPayment>().IsRoot(x => x.Id);
            builder.Entity<PaymentNotification>().IsRoot(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        services.AddScoped<PaymentSeeder>();
        return services;
    }
}
