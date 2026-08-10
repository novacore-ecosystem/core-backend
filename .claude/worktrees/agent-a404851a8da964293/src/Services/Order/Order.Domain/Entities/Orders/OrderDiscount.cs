using NovaCore.Order.Domain.Metadata;

namespace NovaCore.Order.Domain.Entities.Orders;

public sealed class OrderDiscount : BaseEntity<long>, IAuditable, ITenantEntity
{
    public Guid OrderId { get; private set; }
    public long? OrderItemId { get; private set; }
    public Guid? OrderShippingId { get; private set; }
    public DiscountTarget Target { get; private set; }
    public DiscountSource Source { get; private set; } = DiscountSource.Unknown;
    public string? SourceId { get; private set; }
    public string SourceCode { get; private set; } = string.Empty;
    public DiscountRule Rule { get; private set; } = default!;
    public Money AppliedAmount { get; private set; } = default!;
    public DiscountMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderDiscount() { }

    internal static OrderDiscount Create(
        Guid orderId,
        long? orderItemId,
        Guid? orderShippingId,
        DiscountTarget target,
        DiscountSource source,
        string? sourceId,
        string code,
        DiscountRule rule,
        DiscountMetadata? metadata = null)
    {
        return new OrderDiscount
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
            OrderShippingId = orderShippingId,
            Target = target,
            Source = source,
            SourceId = sourceId,
            SourceCode = code,
            Rule = rule,
            AppliedAmount = Money.Create(0),
            Metadata = metadata,
        };
    }

    public void Apply(Money appliedAmount)
    {
        AppliedAmount = appliedAmount;
    }
}
