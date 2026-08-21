namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;

public interface IConversationAssignmentWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationAssignment assignment, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
