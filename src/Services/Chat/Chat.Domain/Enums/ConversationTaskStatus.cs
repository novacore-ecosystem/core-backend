namespace NovaCore.Chat.Domain.Enums;

/// <summary>Named ConversationTaskStatus (spec calls it bare "TaskStatus") to avoid shadowing System.Threading.Tasks.TaskStatus.</summary>
public enum ConversationTaskStatus : byte
{
    Todo = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
}
