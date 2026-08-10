using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

using NovaCore.Order.Persistence.Reliability.Saga;

namespace NovaCore.Order.Persistence.Engine;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderOwner> OrderOwners { get; set; } = null!;
    public DbSet<OrderCancellation> OrderCancellations { get; set; } = null!;
    public DbSet<OrderPayment> OrderPayments { get; set; } = null!;
    public DbSet<OrderPrice> OrderPrices { get; set; } = null!;
    public DbSet<OrderTax> OrderTaxes { get; set; } = null!;
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;
    public DbSet<OrderTag> OrderTags { get; set; } = null!;
    public DbSet<OrderTagDefinition> OrderTagDefinitions { get; set; } = null!;
    public DbSet<ReturnOrder> ReturnOrders { get; set; } = null!;
    public DbSet<ReturnReason> ReturnReasons { get; set; } = null!;
    public DbSet<ReturnStatusHistory> ReturnStatusHistories { get; set; } = null!;
    public DbSet<ProductCatalog> ProductCatalogs { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
    public DbSet<SagaExecutionRecordEntity> SagaExecutionRecords { get; set; } = null!;
}
