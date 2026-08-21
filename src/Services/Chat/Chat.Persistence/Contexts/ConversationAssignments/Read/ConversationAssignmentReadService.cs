using NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;
using NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Read;

public sealed class ConversationAssignmentReadService(IConversationAssignmentRepository assignmentRepo) : IConversationAssignmentReadService
{
    public async Task<ConversationAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await assignmentRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await assignmentRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<ConversationAssignment?> GetActiveByConversationIdAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await assignmentRepo.GetAsync(
            a => a.ConversationId == conversationId && a.Status == ConversationAssignmentStatus.Active,
            ct);
    }
}
