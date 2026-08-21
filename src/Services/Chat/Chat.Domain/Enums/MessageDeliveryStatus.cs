namespace NovaCore.Chat.Domain.Enums;

public enum MessageDeliveryStatus : byte
{
    Pending = 1,
    Delivered = 2,
    Read = 3,
    Failed = 4,
}
