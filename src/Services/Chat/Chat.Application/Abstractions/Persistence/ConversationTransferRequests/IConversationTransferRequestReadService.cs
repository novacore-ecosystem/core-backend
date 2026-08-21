namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;

public interface IConversationTransferRequestReadService
{
    Task<ConversationTransferRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Pending handover invitations addressed to the given user, most recently requested first. Not paginated - naturally small/bounded by active pending transfers.</summary>
    Task<IReadOnlyList<ConversationTransferRequest>> GetPendingForUserAsync(Guid toUserId, CancellationToken ct = default);
}
