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

using NovaCore.Shipping.Application.Abstractions.Persistence.Shipments;
using NovaCore.Shipping.Application.Abstractions.Persistence.Transportations;
using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationCostRules;
using NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProviders;
using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationPeople;
using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationVehicles;
using NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProfiles;
using NovaCore.Shipping.Application.Abstractions.Persistence.VerifiedShippingAddresses;
using NovaCore.Shipping.Application.Abstractions.Persistence.Pickups;
using NovaCore.Shipping.Application.Abstractions.Persistence.Deliveries;
using NovaCore.Shipping.Application.Abstractions.Persistence.ReturnShipments;
using NovaCore.Shipping.Application.Abstractions.Persistence.CarrierIntegrations;
using NovaCore.Shipping.Persistence.Contexts.Shipments.Read;
using NovaCore.Shipping.Persistence.Contexts.Shipments.Write;
using NovaCore.Shipping.Persistence.Contexts.Transportations.Read;
using NovaCore.Shipping.Persistence.Contexts.Transportations.Write;
using NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Read;
using NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Write;
using NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Read;
using NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Write;
using NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Read;
using NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Write;
using NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Read;
using NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Write;
using NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Read;
using NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Write;
using NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Read;
using NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Write;
using NovaCore.Shipping.Persistence.Contexts.Pickups.Read;
using NovaCore.Shipping.Persistence.Contexts.Pickups.Write;
using NovaCore.Shipping.Persistence.Contexts.Deliveries.Read;
using NovaCore.Shipping.Persistence.Contexts.Deliveries.Write;
using NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Read;
using NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Write;
using NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Read;
using NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Write;
using NovaCore.Shipping.Persistence.Engine;
using NovaCore.Shipping.Persistence.Engine.UnitOfWork;
using NovaCore.Shipping.Persistence.Reliability.Inbox;
using NovaCore.Shipping.Persistence.Reliability.Outbox;

namespace NovaCore.Shipping.Persistence;

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
            .AddAuditHierarchy();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<ShippingDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(ShippingDbContext));
        return services;
    }

    // Every *Repo implements the generic IRepository<T> - Scrutor's AsImplementedInterfaces()
    // registers each concrete class against every interface it implements (including the
    // per-aggregate marker interfaces), so this one scan call covers all twelve. Read/Write
    // services are registered explicitly since they are one-per-aggregate.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(ShippingDbContext));

        services.AddScoped<IShipmentReadService, ShipmentReadService>();
        services.AddScoped<IShipmentWriteService, ShipmentWriteService>();

        services.AddScoped<ITransportationReadService, TransportationReadService>();
        services.AddScoped<ITransportationWriteService, TransportationWriteService>();

        services.AddScoped<ITransportationCostRuleReadService, TransportationCostRuleReadService>();
        services.AddScoped<ITransportationCostRuleWriteService, TransportationCostRuleWriteService>();

        services.AddScoped<IShippingProviderReadService, ShippingProviderReadService>();
        services.AddScoped<IShippingProviderWriteService, ShippingProviderWriteService>();

        services.AddScoped<ITransportationPersonReadService, TransportationPersonReadService>();
        services.AddScoped<ITransportationPersonWriteService, TransportationPersonWriteService>();

        services.AddScoped<ITransportationVehicleReadService, TransportationVehicleReadService>();
        services.AddScoped<ITransportationVehicleWriteService, TransportationVehicleWriteService>();

        services.AddScoped<IShippingProfileReadService, ShippingProfileReadService>();
        services.AddScoped<IShippingProfileWriteService, ShippingProfileWriteService>();

        services.AddScoped<IVerifiedShippingAddressReadService, VerifiedShippingAddressReadService>();
        services.AddScoped<IVerifiedShippingAddressWriteService, VerifiedShippingAddressWriteService>();

        services.AddScoped<IPickupReadService, PickupReadService>();
        services.AddScoped<IPickupWriteService, PickupWriteService>();

        services.AddScoped<IDeliveryReadService, DeliveryReadService>();
        services.AddScoped<IDeliveryWriteService, DeliveryWriteService>();

        services.AddScoped<IReturnShipmentReadService, ReturnShipmentReadService>();
        services.AddScoped<IReturnShipmentWriteService, ReturnShipmentWriteService>();

        services.AddScoped<ICarrierIntegrationReadService, CarrierIntegrationReadService>();
        services.AddScoped<ICarrierIntegrationWriteService, CarrierIntegrationWriteService>();

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
            .AddEfOutboxStore<ShippingDbContext>()
            .AddEfInboxStore<ShippingDbContext>()
            .AddEfDeadLetterQueryService<ShippingDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    // Only the three aggregates that actually own children need a hierarchy mapping; the
    // remaining nine are flat roots. Registering every IAuditable aggregate up front (even
    // before it has a write path) keeps the audit trail complete as later phases add CQRS -
    // same approach Payment's own AddAuditHierarchy documents.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            // Shipment aggregate
            builder.Entity<Shipment>().IsRoot(x => x.Id);
            builder.Entity<ShipmentItem>().BelongsTo<Shipment>(x => x.ShipmentId);
            builder.Entity<ShipmentEvent>().BelongsTo<Shipment>(x => x.ShipmentId);
            builder.Entity<Package>().BelongsTo<Shipment>(x => x.ShipmentId);

            // Transportation aggregate
            builder.Entity<Transportation>().IsRoot(x => x.Id);
            builder.Entity<TransportationAssignment>().BelongsTo<Transportation>(x => x.TransportationId);
            builder.Entity<TransportationTracking>().BelongsTo<Transportation>(x => x.TransportationId);
            builder.Entity<TransportationProof>().BelongsTo<Transportation>(x => x.TransportationId);
            builder.Entity<TransportationCost>().BelongsTo<Transportation>(x => x.TransportationId);

            // Provider aggregate
            builder.Entity<ShippingProvider>().IsRoot(x => x.Id);
            builder.Entity<ShippingProviderProfile>().BelongsTo<ShippingProvider>(x => x.ProviderId);

            // Flat roots
            builder.Entity<TransportationCostRule>().IsRoot(x => x.Id);
            builder.Entity<TransportationPerson>().IsRoot(x => x.Id);
            builder.Entity<TransportationVehicle>().IsRoot(x => x.Id);
            builder.Entity<ShippingProfile>().IsRoot(x => x.Id);
            builder.Entity<VerifiedShippingAddress>().IsRoot(x => x.Id);
            builder.Entity<Pickup>().IsRoot(x => x.Id);
            builder.Entity<Delivery>().IsRoot(x => x.Id);
            builder.Entity<ReturnShipment>().IsRoot(x => x.Id);
            builder.Entity<CarrierIntegration>().IsRoot(x => x.Id);
        });

        return services;
    }
}
