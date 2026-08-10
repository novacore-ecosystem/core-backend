namespace NovaCore.Payment.Domain.Enums;

public enum WebhookDeliveryStatus : byte
{
    Pending = 1,
    Delivered = 2,
    Failed = 3,
    Retrying = 4,
}
