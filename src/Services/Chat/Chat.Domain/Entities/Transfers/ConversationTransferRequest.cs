namespace NovaCore.Chat.Domain.Entities.Transfers;

/// <summary>Explicit handover request between two users - the conversation's owner only changes once this is Accepted (see spec section 17), not at request creation.</summary>
public sealed class ConversationTransferRequest : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public ConversationTransferStatus Status { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationTransferRequest() { }

    public static ConversationTransferRequest Create(
        Guid conversationId,
        Guid fromUserId,
        Guid toUserId,
        ChatMetadata? metadata = null)
    {
        if (fromUserId == toUserId)
            throw ExceptionFactory.InvalidState("Cannot transfer a conversation to the same user.");

        return new ConversationTransferRequest
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Status = ConversationTransferStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            Metadata = metadata,
        };
    }

    public void Accept()
    {
        EnsurePending();

        Status = ConversationTransferStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        EnsurePending();

        Status = ConversationTransferStatus.Rejected;
        RespondedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        EnsurePending();

        Status = ConversationTransferStatus.Cancelled;
        RespondedAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (Status != ConversationTransferStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot respond to a transfer request in {Status} status.");
    }
}
