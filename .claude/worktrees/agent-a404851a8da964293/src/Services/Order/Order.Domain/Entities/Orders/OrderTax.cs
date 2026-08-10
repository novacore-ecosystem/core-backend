namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// One jurisdiction's tax line applied to an Order - replaces the old single Tax value object so
/// multiple taxes can apply to the same order later without a schema change. Exactly one row
/// exists today (seeded from the business's single supported country at Order.Create time);
/// OrderPrice.TaxAmount is the denormalized sum of these rows, same relationship
/// Order.Price.GrandTotal has over OrderItem rows.
/// </summary>
public sealed class OrderTax : BaseEntity<long>, IAuditable, ITenantEntity
{
    public Guid OrderId { get; private set; }
    public TaxType TaxType { get; private set; }
    public decimal TaxRate { get; private set; }
    public Money TaxAmount { get; private set; } = default!;
    public string TaxName { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderTax() { }

    /// <summary>Only Order may construct its OrderTax rows - same reasoning as OrderDiscount.Create being internal.</summary>
    internal static OrderTax Create(
        Guid orderId,
        TaxType taxType,
        decimal taxRate,
        Money taxAmount,
        string taxName,
        string countryCode)
    {
        return new OrderTax
        {
            OrderId = orderId,
            TaxType = taxType,
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            TaxName = taxName,
            CountryCode = countryCode,
        };
    }
}
