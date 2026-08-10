namespace NovaCore.Payment.Domain.Enums;

public enum GatewayType : byte
{
    Stripe = 1,
    PayPal = 2,
    VNPay = 3,
    MoMo = 4,
    Adyen = 5,
    Manual = 6,
}
