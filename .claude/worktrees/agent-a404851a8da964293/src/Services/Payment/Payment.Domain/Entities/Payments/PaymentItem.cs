namespace NovaCore.Payment.Domain.Entities.Payments;

/// <summary>Payment breakdown line (product, shipping, tax, discount, insurance, fee, tip). Own table/PK, FK back to Payment - only Payment may construct one.</summary>
public sealed class PaymentItem : BaseEntity<Guid>, IAuditable
{
    public Guid PaymentId { get; private set; }
    public PaymentItemType ItemType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = default!;
    public int Quantity { get; private set; } = 1;

    private PaymentItem() { }

    internal static PaymentItem Create(
        Guid paymentId,
        PaymentItemType itemType,
        string description,
        Money amount,
        int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw ExceptionFactory.RequiredField("Payment item description cannot be empty.");

        if (quantity < 1)
            throw ExceptionFactory.InvalidRange("Payment item quantity must be at least 1.");

        return new PaymentItem
        {
            Id = Guid.CreateVersion7(),
            PaymentId = paymentId,
            ItemType = itemType,
            Description = description.Trim(),
            Amount = amount,
            Quantity = quantity,
        };
    }
}
