namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Pricing breakdown split out from Order itself so the core order/status/items data isn't
/// coupled to every pricing column - same reasoning as OrderOwner. 1:1 with Order, sharing its
/// primary key (OrderId) - see OrderPriceConfig. ShippingFee is intentionally not a column here;
/// it stays a computed pass-through on Order to OrderShipping.ShippingFee (see Order.ShippingFee) so
/// it is never stored twice. Only Subtotal is currently derived from real data (sum of
/// OrderItem.Subtotal) - PromotionDiscount/CouponDiscount/ServiceFee/PlatformFee/
/// RoundingAdjustment are scaffolded at zero until the Promotion/Coupon/Fee calculation logic
/// they depend on exists (see the Order Service architecture refactor plan).
/// </summary>
public sealed class OrderPrice : BaseEntity, IAuditable, ITenantEntity
{
    public Guid OrderId { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public decimal ExchangeRate { get; private set; } = 1.0m;
    public Money Subtotal { get; private set; } = default!;
    public Money ItemDiscount { get; private set; } = default!;
    public Money PromotionDiscount { get; private set; } = default!;
    public Money CouponDiscount { get; private set; } = default!;
    public Money TaxAmount { get; private set; } = default!;
    public Money ServiceFee { get; private set; } = default!;
    public Money PlatformFee { get; private set; } = default!;
    public Money RoundingAdjustment { get; private set; } = default!;
    public Money GrandTotal { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderPrice() { }

    /// <summary>
    /// Only Order may construct/mutate its Price - same reasoning as OrderOwner.Create being
    /// internal. GrandTotal is derived here, not supplied by the caller, so it can never drift
    /// from its inputs.
    /// </summary>
    internal static OrderPrice Create(
        Guid orderId,
        Money subtotal,
        Money itemDiscount,
        Money promotionDiscount,
        Money couponDiscount,
        Money taxAmount,
        Money shippingFee,
        string currencyCode = "USD",
        decimal exchangeRate = 1.0m)
    {
        var serviceFee = Money.Create(0);
        var platformFee = Money.Create(0);
        var roundingAdjustment = Money.Create(0);

        var grandTotal = subtotal.Value
            - itemDiscount.Value
            - promotionDiscount.Value
            - couponDiscount.Value
            + taxAmount.Value
            + shippingFee.Value
            + serviceFee.Value
            + platformFee.Value
            + roundingAdjustment.Value;

        return new OrderPrice
        {
            OrderId = orderId,
            CurrencyCode = currencyCode,
            ExchangeRate = exchangeRate,
            Subtotal = subtotal,
            ItemDiscount = itemDiscount,
            PromotionDiscount = promotionDiscount,
            CouponDiscount = couponDiscount,
            TaxAmount = taxAmount,
            ServiceFee = serviceFee,
            PlatformFee = platformFee,
            RoundingAdjustment = roundingAdjustment,
            GrandTotal = Money.Create(grandTotal),
        };
    }
}
