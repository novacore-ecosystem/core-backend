namespace NovaCore.Chat.Domain.Entities.Ratings;

/// <summary>
/// Post-close quality feedback for a conversation - independently constructible by ConversationId
/// (no back-collection on Conversation, same doctrine as Message/Assignment/etc.), at most one per
/// conversation (enforced by a unique index on ConversationId, see ConversationRatingConfig).
///
/// Row existence *is* the submitted flag: no row means the customer never rated the conversation;
/// a row with Stars = 0 and Review = null means the customer explicitly submitted a zero-star,
/// no-comment rating. There is no separate "IsSubmitted" column to keep in sync.
/// </summary>
public sealed class ConversationRating : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid RatedByUserId { get; private set; }
    public byte Stars { get; private set; }
    public string? Review { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationRating() { }

    public static ConversationRating Create(Guid conversationId, Guid ratedByUserId, byte stars, string? review = null)
    {
        ValidateStars(stars);

        return new ConversationRating
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            RatedByUserId = ratedByUserId,
            Stars = stars,
            Review = review,
        };
    }

    public static bool IsValidStars(byte stars) => stars <= 5;

    private static void ValidateStars(byte stars)
    {
        if (!IsValidStars(stars))
            throw ExceptionFactory.InvalidRange("Stars must be between 0 and 5.");
    }
}
