namespace NovaCore.Order.Domain.Entities.Orders.Data;

/// <summary>
/// Everything Order.Create needs to build itself plus its Owner/Shipping/Items in one call - the
/// Application/Persistence boundary builds this (see NovaCore.Order.Persistence.Mapping.Orders.OrderMapper)
/// so Order.cs owns constructing its own child entities instead of receiving them pre-built.
/// </summary>
public sealed record CreateOrderData(
    string IdempotencyKey,
    Guid? CreatedById,
    CreateOrderOwnerData Owner,
    CreateOrderShippingData Shipping,
    IReadOnlyList<CreateOrderItemData> Items);

public sealed record CreateOrderOwnerData(
    Guid OwnerId,
    string OwnerName,
    Email OwnerEmail,
    PhoneNumber OwnerPhone,
    string? IdempotencyKey = null);

public sealed record CreateOrderShippingData(
    ShippingMethod ShippingMethod,
    string ReceiverName,
    PhoneNumber ReceiverPhone,
    string Address,
    string Note,
    string? IdempotencyKey = null);

public sealed record CreateOrderItemData(
    int LineNo,
    Guid ProductId,
    Guid VariationId,
    string ProductName,
    string VariationName,
    Money UnitPrice,
    Quantity Quantity,
    OrderItemType Type = OrderItemType.Normal);
