using System.Text;

namespace NovaCore.Chat.Application.Features.ConversationQueues.Queries.GetConversationQueue;

/// <summary>
/// Opaque keyset cursor over (EnqueuedAt, ConversationId) - the tiebreaker on ConversationId keeps
/// pagination stable when two items share the same EnqueuedAt tick. Local to this feature rather
/// than a shared cursor-encoding helper - promote it only if a second cursor-paginated feature
/// needs the identical shape.
/// </summary>
public static class QueueCursor
{
    public static string Encode(DateTime enqueuedAt, Guid conversationId) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{enqueuedAt.Ticks}:{conversationId}"));

    public static bool TryDecode(string? cursor, out long afterEnqueuedAtTicks, out Guid afterConversationId)
    {
        afterEnqueuedAtTicks = 0;
        afterConversationId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(cursor))
            return false;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = decoded.Split(':', 2);

            if (parts.Length != 2 || !long.TryParse(parts[0], out afterEnqueuedAtTicks) || !Guid.TryParse(parts[1], out afterConversationId))
                return false;

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
