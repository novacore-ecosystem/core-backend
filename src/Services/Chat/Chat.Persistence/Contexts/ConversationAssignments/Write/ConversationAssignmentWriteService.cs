using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;
using NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Write;

public sealed class ConversationAssignmentWriteService(
    IConversationAssignmentRepository assignmentRepo,
    IUnitOfWork unitOfWork) : IConversationAssignmentWriteService
{
    public async Task CreateAsync(ConversationAssignment assignment, CancellationToken ct = default)
    {
        await assignmentRepo.AddAsync(assignment, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await assignmentRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
