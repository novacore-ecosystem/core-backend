namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;

public interface IConversationAssignmentWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationAssignment assignment, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Stages only - callers that need this alongside another aggregate's write in the same transaction own the commit (see AcceptHandoverInvitationHandler).</summary>
    Task ReleaseAsync(Guid id, CancellationToken ct = default);
}
