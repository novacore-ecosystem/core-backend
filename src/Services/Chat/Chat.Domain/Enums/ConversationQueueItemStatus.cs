namespace NovaCore.Chat.Domain.Enums;

public enum ConversationQueueItemStatus : byte
{
    Waiting = 1,
    Assigned = 2,
    Removed = 3,
}
