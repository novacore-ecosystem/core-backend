namespace NovaCore.Payment.Domain.Enums;

public enum PaymentMethodType : byte
{
    Card = 1,
    BankTransfer = 2,
    EWallet = 3,
    DigitalWallet = 4,
    Cod = 5,
    BankAccount = 6,
}
