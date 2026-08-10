using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

namespace NovaCore.Payment.Persistence.Engine;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    // Core lifecycle
    public DbSet<PaymentIntent> PaymentIntents { get; set; } = null!;
    public DbSet<PaymentEntity> Payments { get; set; } = null!;
    public DbSet<PaymentItem> PaymentItems { get; set; } = null!;
    public DbSet<PaymentAttempt> PaymentAttempts { get; set; } = null!;
    public DbSet<Refund> Refunds { get; set; } = null!;
    public DbSet<RefundAttempt> RefundAttempts { get; set; } = null!;

    // Catalog
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
    public DbSet<PaymentGateway> PaymentGateways { get; set; } = null!;
    public DbSet<GatewayConfiguration> GatewayConfigurations { get; set; } = null!;
    public DbSet<GatewayStatusMapping> GatewayStatusMappings { get; set; } = null!;

    // Accounts & billing
    public DbSet<PaymentAccount> PaymentAccounts { get; set; } = null!;
    public DbSet<PaymentToken> PaymentTokens { get; set; } = null!;
    public DbSet<BillingProfile> BillingProfiles { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;

    // Operations
    public DbSet<Settlement> Settlements { get; set; } = null!;
    public DbSet<Reconciliation> Reconciliations { get; set; } = null!;
    public DbSet<Payout> Payouts { get; set; } = null!;
    public DbSet<PaymentFee> PaymentFees { get; set; } = null!;
    public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;
    public DbSet<PaymentEventLog> PaymentEventLogs { get; set; } = null!;
    public DbSet<PaymentAudit> PaymentAudits { get; set; } = null!;
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; } = null!;

    // Webhooks
    public DbSet<WebhookEvent> WebhookEvents { get; set; } = null!;
    public DbSet<WebhookDelivery> WebhookDeliveries { get; set; } = null!;

    // Scheduling & notifications
    public DbSet<PaymentSession> PaymentSessions { get; set; } = null!;
    public DbSet<ScheduledPayment> ScheduledPayments { get; set; } = null!;
    public DbSet<PaymentNotification> PaymentNotifications { get; set; } = null!;

    // Reliability
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
}
