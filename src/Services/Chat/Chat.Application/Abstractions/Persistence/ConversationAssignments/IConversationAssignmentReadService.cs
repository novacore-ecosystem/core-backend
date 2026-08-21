namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;

public interface IConversationAssignmentReadService
{
    Task<ConversationAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
