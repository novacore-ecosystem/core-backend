namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;

public interface IConversationTransferRequestWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationTransferRequest request, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Stages only - AcceptHandoverInvitationHandler also releases/creates a ConversationAssignment in the same transaction.</summary>
    Task AcceptAsync(Guid id, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task RejectAsync(Guid id, CancellationToken ct = default);
}
