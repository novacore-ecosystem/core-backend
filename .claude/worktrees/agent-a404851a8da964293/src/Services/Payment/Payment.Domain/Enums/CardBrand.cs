namespace NovaCore.Payment.Domain.Enums;

public enum CardBrand : byte
{
    Unknown = 0,
    Visa = 1,
    MasterCard = 2,
    Amex = 3,
    Jcb = 4,
    UnionPay = 5,
    Discover = 6,
}
