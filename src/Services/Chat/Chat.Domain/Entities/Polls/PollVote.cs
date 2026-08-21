namespace NovaCore.Chat.Domain.Entities.Polls;

/// <summary>A user's vote for one poll option - composite key (PollOptionId, UserId), so a MultipleChoice poll naturally allows several rows per user (one per option chosen). PollId is kept as a plain denormalized column per spec section 31's explicit property list. No vote history.</summary>
public sealed class PollVote : BaseEntity, ITenantEntity
{
    public Guid PollId { get; private set; }
    public Guid PollOptionId { get; private set; }
    public PollOption PollOption { get; private set; } = default!;
    public Guid UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PollVote() { }

    public static PollVote Create(Guid pollId, Guid pollOptionId, Guid userId)
    {
        return new PollVote
        {
            PollId = pollId,
            PollOptionId = pollOptionId,
            UserId = userId,
        };
    }
}
