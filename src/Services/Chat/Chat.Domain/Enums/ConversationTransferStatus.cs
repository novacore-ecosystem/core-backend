namespace NovaCore.Chat.Domain.Enums;

public enum ConversationTransferStatus : byte
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Cancelled = 4,
}
