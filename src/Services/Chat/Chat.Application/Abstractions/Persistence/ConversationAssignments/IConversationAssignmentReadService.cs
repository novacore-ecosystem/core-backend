namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;

public interface IConversationAssignmentReadService
{
    Task<ConversationAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The conversation's currently active (not yet released) assignment, if any - a conversation has at most one at a time.</summary>
    Task<ConversationAssignment?> GetActiveByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
}
