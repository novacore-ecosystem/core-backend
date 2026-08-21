namespace NovaCore.Chat.Domain.Enums;

public enum ConversationScheduleStatus : byte
{
    Scheduled = 1,
    Executed = 2,
    Cancelled = 3,
    Failed = 4,
}
