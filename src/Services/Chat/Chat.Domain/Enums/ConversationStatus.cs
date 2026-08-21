namespace NovaCore.Chat.Domain.Enums;

public enum ConversationStatus : byte
{
    Queued = 1,
    Open = 2,
    Pending = 3,
    Closed = 4,
}
