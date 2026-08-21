namespace NovaCore.Chat.Domain.Enums;

/// <summary>Named ConversationTaskPriority (spec calls it bare "TaskPriority") for symmetry with ConversationTaskStatus.</summary>
public enum ConversationTaskPriority : byte
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4,
}
