namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Explicit many-to-many join entity between Order and OrderTagDefinition - Order and
/// OrderTagDefinition are independent aggregate roots, so this row (not a raw id collection) is
/// how Order references a tag without holding an object reference to another root. Same pattern
/// as NovaCore.User.Domain.Entities.Users.UserTagMapping, except - consistent with every other
/// Order child (OrderItem/OrderDiscount/OrderTax) - it has no navigation properties, only ids;
/// callers that need tag details join in the query layer.
/// </summary>
public sealed class OrderTag : BaseEntity, ITenantEntity
{
    public Guid OrderId { get; private set; }
    public Guid TagId { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderTag() { }

    /// <summary>Only Order may construct/remove its OrderTag rows - same reasoning as OrderDiscount.Create being internal.</summary>
    internal static OrderTag Create(Guid orderId, Guid tagId)
    {
        return new OrderTag
        {
            OrderId = orderId,
            TagId = tagId,
        };
    }
}
