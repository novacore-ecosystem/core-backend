namespace NovaCore.Payment.Domain.Enums;

public enum PaymentIntentStatus : byte
{
    Created = 1,
    RequiresPaymentMethod = 2,
    RequiresConfirmation = 3,
    Processing = 4,
    Succeeded = 5,
    Canceled = 6,
    Expired = 7,
}
