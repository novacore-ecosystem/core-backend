using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Enums;

using NovaCore.Order.Domain.Entities.Orders.Data;

namespace NovaCore.Order.Domain.Entities.Orders;

public sealed class Order : AggregateRoot<Guid>, IAuditable, ITenantEntity, IIdempotentEntity
{
    public OrderNumber OrderNumber { get; private set; } = default!;
    public OrderOwner Owner { get; private set; } = default!;
    public OrderShipping Shipping { get; private set; } = default!;
    public OrderPayment Payment { get; private set; } = default!;
    public OrderPrice Price { get; private set; } = default!;
    public ICollection<OrderItem> Items { get; private set; } = [];
    public ICollection<OrderDiscount> Discounts { get; private set; } = [];
    public ICollection<OrderTax> Taxes { get; private set; } = [];
    public ICollection<OrderTag> Tags { get; private set; } = [];
    public OrderStatus Status { get; private set; }
    public OrderCancellation? Cancellation { get; private set; }

    /// <summary>Computed pass-through to Cancellation - kept so existing read sites don't need to change to Order.Cancellation?.Reason.</summary>
    public string? CancellationReason => Cancellation?.Reason;
    public Money ShippingFee => Shipping.ShippingFee;

    /// <summary>Computed pass-throughs to Price - kept so existing read sites (queries, DTOs) don't all need to change to Order.Price.X.</summary>
    public Money Subtotal => Price.Subtotal;
    public Money GrandTotal => Price.GrandTotal;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public Guid? CreatedById { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Order() { }

    public static Order Create(string idempotencyKey, Guid? createdById = null)
    {
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            OrderNumber = OrderNumber.Create(),
            Status = OrderStatus.Pending,
            CreatedById = createdById,
            IdempotencyKey = idempotencyKey,
        };

        return order;
    }

    /// <summary>Builds the order plus its Owner/Shipping/Items in one call - see CreateOrderData's own remarks for why this exists.</summary>
    public static Order Create(CreateOrderData data)
    {
        var order = Create(data.IdempotencyKey, data.CreatedById);

        order.CreateOwner(data.Owner);
        order.CreateShipping(data.Shipping);
        order.CreatePayment();
        order.CreateItems(data.Items);
        order.CreateTaxes();
        order.CreatePrice();

        return order;
    }
    #endregion

    #region Order
    public void Accept()
    {
        if (Status != OrderStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot accept an order in {Status} status.");

        if (Items.Count == 0)
            throw new BadRequestException(MessageCode.InvalidOrderItems);

        Status = OrderStatus.Confirmed;
    }

    public void Process()
    {
        if (Status != OrderStatus.Confirmed)
            throw ExceptionFactory.InvalidStatus($"Cannot start processing an order in {Status} status.");

        if (Items.Count == 0)
            throw new BadRequestException(MessageCode.InvalidOrderItems);

        Status = OrderStatus.Processing;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Processing)
            throw ExceptionFactory.InvalidStatus($"Cannot complete an order in {Status} status.");

        if (Shipping.Status != ShippingStatus.Delivered)
            throw ExceptionFactory.InvalidStatus($"Cannot complete an order when shipping status is {Shipping.Status}.");

        Status = OrderStatus.Completed;
    }

    public void Cancel(
        string reason,
        Guid? cancelledByUserId = null,
        string? cancelledByName = null,
        string? comment = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A cancellation reason is required.");

        if (Status == OrderStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus("Order is already cancelled.");

        if (Status == OrderStatus.Completed)
            throw ExceptionFactory.InvalidStatus("Cannot cancel a completed order.");

        Shipping.Cancel();
        Status = OrderStatus.Cancelled;

        // Refund/payment rollback is only meaningful once money has actually moved - Payment
        // isn't wired to a real Payment Service yet, so PaymentStatus.Paid never happens today,
        // but the logic already reflects the real rule for when it does. Inventory rollback is
        // unconditional to match CancelOrderHandler's existing unconditional RestockAsync call.
        var alreadyPaid = Payment.PaymentStatus == PaymentStatus.Paid;
        Cancellation = OrderCancellation.Create(
            Id,
            reason,
            cancelledByUserId,
            cancelledByName,
            refundRequired: alreadyPaid,
            inventoryRollbackRequired: true,
            paymentRollbackRequired: alreadyPaid,
            comment);
    }
    #endregion

    #region OrderItem
    /// <summary>Builds this order's OrderItems from behavior data - only Order may construct an OrderItem (see OrderItem.Create being internal).</summary>
    public void CreateItems(IReadOnlyList<CreateOrderItemData> itemsData)
    {
        if (itemsData.Count == 0)
            throw ExceptionFactory.EmptyCollection("An order must contain at least one item.");

        if (Items.Count != 0)
            throw ExceptionFactory.InvalidState("The items in the order cannot be modified.");

        Items = [.. itemsData.Select(data =>
            OrderItem.Create(
                Id,
                data.LineNo,
                data.ProductId,
                data.VariationId,
                data.ProductName,
                data.VariationName,
                data.UnitPrice,
                data.Quantity,
                data.Type))];
    }
    #endregion

    #region Owner
    /// <summary>Builds this order's Owner from behavior data - only Order may construct an OrderOwner (see OrderOwner.Create being internal).</summary>
    public void CreateOwner(CreateOrderOwnerData data)
    {
        Owner = OrderOwner.Create(
            Id,
            data.OwnerId,
            data.OwnerName,
            data.OwnerEmail,
            data.OwnerPhone,
            data.IdempotencyKey);
    }

    public void UpdateOwnerInfo(
        string ownerName,
        Email ownerEmail,
        PhoneNumber ownerPhone,
        string idempotencyKey)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw ExceptionFactory.InvalidStatus(
                $"Cannot update owner information on an order in {Status} status.");

        Owner.UpdateContact(ownerName, ownerEmail, ownerPhone, idempotencyKey);
    }
    #endregion

    #region Shipping
    /// <summary>Builds this order's Shipping from behavior data - only Order may construct an OrderShipping (see OrderShipping.Create being internal).</summary>
    public void CreateShipping(CreateOrderShippingData data)
    {
        Shipping = OrderShipping.Create(
            Id,
            data.ReceiverName,
            data.ReceiverPhone,
            data.Address,
            data.ShippingMethod,
            data.Note,
            data.IdempotencyKey);
    }

    public void UpdateShippingInfo(
        string receiverName,
        PhoneNumber receiverPhone,
        string address,
        string idempotencyKey)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw ExceptionFactory.InvalidStatus($"Cannot update shipping information on an order in {Status} status.");

        Shipping.UpdateContact(receiverName, receiverPhone, address, idempotencyKey);
    }
    #endregion

    #region Payment
    /// <summary>Builds this order's Payment - only Order may construct an OrderPayment (see OrderPayment.Create being internal).</summary>
    internal void CreatePayment()
    {
        Payment = OrderPayment.Create(Id);
    }
    #endregion

    #region Tax
    /// <summary>
    /// Seeds this order's OrderTax rows. No tax calculation logic exists yet (the business
    /// currently supports a single country), so this seeds one zero-rate placeholder row -
    /// architecture supports multiple rows per order, wiring real rates is a later phase.
    /// </summary>
    internal void CreateTaxes()
    {
        Taxes =
        [
            OrderTax.Create(
                Id,
                TaxType.SalesTax,
                taxRate: 0m,
                taxAmount: Money.Create(0),
                taxName: "Sales Tax",
                countryCode: "US"),
        ];
    }
    #endregion

    #region Price
    /// <summary>
    /// Derives this order's Price from its Items/Shipping/Taxes - must run after CreateItems,
    /// CreateShipping and CreateTaxes. Only Subtotal/TaxAmount/ShippingFee reflect real data
    /// today; the remaining discount/fee buckets are zero until Promotion/Coupon/Fee logic exists
    /// (see OrderPrice's remarks).
    /// </summary>
    internal void CreatePrice()
    {
        var subtotal = Money.Create(Items.Sum(i => i.Subtotal.Value));
        var taxAmount = Money.Create(Taxes.Sum(t => t.TaxAmount.Value));
        var zero = Money.Create(0);

        Price = OrderPrice.Create(
            Id,
            subtotal,
            itemDiscount: zero,
            promotionDiscount: zero,
            couponDiscount: zero,
            taxAmount: taxAmount,
            shippingFee: Shipping.ShippingFee);
    }
    #endregion

    #region Tag
    public void AddTag(Guid tagId)
    {
        if (Tags.Any(t => t.TagId == tagId))
            throw ExceptionFactory.Duplicate("This tag is already assigned to the order.");

        Tags.Add(OrderTag.Create(Id, tagId));
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = Tags.FirstOrDefault(t => t.TagId == tagId)
            ?? throw ExceptionFactory.EntityNotFound<OrderTag>(tagId);

        Tags.Remove(tag);
    }
    #endregion

    #region Discount
    public void LoadDiscounts(IEnumerable<OrderDiscount> discounts)
    {
        if (discounts.Any(d => d.OrderId != Id))
            throw new BadRequestException("One or more order item discount are invalid.");

        Discounts = [.. discounts];
    }

    public void AddDiscount(OrderDiscount discount)
    {
        if (discount.OrderId != Id)
            throw new InvalidArgumentException(
                "Cannot add a discount to an order that does not match the discount's OrderId.");

        if (discount.Target != DiscountTarget.Order)
            throw new InvalidArgumentException(
                "Cannot add an OrderItem-targeted discount to the order itself - it must be added to the specific OrderItem instead.");

        Discounts.Add(discount);
    }

    public void AddRangeDiscounts(IEnumerable<OrderDiscount> discounts)
    {
        foreach (var discount in discounts)
        {
            AddDiscount(discount);
        }
    }
    #endregion

}
