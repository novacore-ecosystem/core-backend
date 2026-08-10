namespace NovaCore.Payment.Domain.Enums;

public enum PaymentAccountType : byte
{
    Card = 1,
    Bank = 2,
    PayPal = 3,
    Wallet = 4,
    ApplePay = 5,
    GooglePay = 6,
}
