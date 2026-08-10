namespace NovaCore.Inventory.Persistence.Engine;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContextBase(options),
    IInboxDbContext,
    IOutboxDbContext
{
    // Aggregate Roots
    public DbSet<InventoryStock> InventoryStocks { get; set; } = null!;
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
    public DbSet<InventoryLot> InventoryLots { get; set; } = null!;
    public DbSet<InventoryReservation> InventoryReservations { get; set; } = null!;
    public DbSet<InventorySerial> InventorySerials { get; set; } = null!;
    public DbSet<InventoryCount> InventoryCounts { get; set; } = null!;
    public DbSet<InventoryDocument> InventoryDocuments { get; set; } = null!;
    public DbSet<Warehouse> Warehouses { get; set; } = null!;

    // Child Entities
    public DbSet<InventoryCountItem> InventoryCountItems { get; set; } = null!;
    public DbSet<InventoryDocumentItem> InventoryDocumentItems { get; set; } = null!;
    public DbSet<WarehouseZone> WarehouseZones { get; set; } = null!;

    // System Tables
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        base.ConfigureModel(modelBuilder);
    }
}
