namespace NovaCore.Payment.Domain.Enums;

public enum WebhookStatus : byte
{
    Received = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4,
}
