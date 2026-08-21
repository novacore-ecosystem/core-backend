namespace NovaCore.Chat.Infrastructure.SignalR.Groups;

/// <summary>Mirrors Notification.Infrastructure's ActorGroups naming convention.</summary>
internal static class ConversationGroups
{
    public static string Conversation(Guid conversationId) => $"conversation:{conversationId}";
}
