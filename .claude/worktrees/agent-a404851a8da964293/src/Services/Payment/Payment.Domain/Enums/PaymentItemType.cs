namespace NovaCore.Payment.Domain.Enums;

public enum PaymentItemType : byte
{
    Product = 1,
    Shipping = 2,
    Tax = 3,
    Discount = 4,
    Insurance = 5,
    Fee = 6,
    Tip = 7,
}
