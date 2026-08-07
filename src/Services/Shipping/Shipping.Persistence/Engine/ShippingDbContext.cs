using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

namespace NovaCore.Shipping.Persistence.Engine;

public sealed class ShippingDbContext(DbContextOptions<ShippingDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    // Shipment aggregate - the logistics intention
    public DbSet<Shipment> Shipments { get; set; } = null!;
    public DbSet<ShipmentItem> ShipmentItems { get; set; } = null!;
    public DbSet<ShipmentEvent> ShipmentEvents { get; set; } = null!;
    public DbSet<Package> Packages { get; set; } = null!;
    public DbSet<PackageItem> PackageItems { get; set; } = null!;

    // Transportation aggregate - one execution attempt, and the only provider binding point
    public DbSet<Transportation> Transportations { get; set; } = null!;
    public DbSet<TransportationAssignment> TransportationAssignments { get; set; } = null!;
    public DbSet<TransportationTracking> TransportationTrackings { get; set; } = null!;
    public DbSet<TransportationProof> TransportationProofs { get; set; } = null!;
    public DbSet<TransportationCost> TransportationCosts { get; set; } = null!;
    public DbSet<TransportationCostRule> TransportationCostRules { get; set; } = null!;

    // Capacity - who and what can carry a Transportation
    public DbSet<ShippingProvider> ShippingProviders { get; set; } = null!;
    public DbSet<ShippingProviderProfile> ShippingProviderProfiles { get; set; } = null!;
    public DbSet<TransportationPerson> TransportationPeople { get; set; } = null!;
    public DbSet<TransportationVehicle> TransportationVehicles { get; set; } = null!;

    // User-level address knowledge
    public DbSet<ShippingProfile> ShippingProfiles { get; set; } = null!;
    public DbSet<VerifiedShippingAddress> VerifiedShippingAddresses { get; set; } = null!;

    // Endpoints of the physical journey
    public DbSet<Pickup> Pickups { get; set; } = null!;
    public DbSet<Delivery> Deliveries { get; set; } = null!;
    public DbSet<ReturnShipment> ReturnShipments { get; set; } = null!;

    // External carrier connectivity (stored only - nothing calls a carrier API yet)
    public DbSet<CarrierIntegration> CarrierIntegrations { get; set; } = null!;

    // Reliability
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
}
