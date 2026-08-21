namespace NovaCore.Chat.Domain.Enums;

public enum ConversationParticipantStatus : byte
{
    Invited = 1,
    Active = 2,
    Left = 3,
    Removed = 4,
}
