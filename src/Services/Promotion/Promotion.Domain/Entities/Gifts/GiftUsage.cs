namespace NovaCore.Promotion.Domain.Entities.Gifts;

/// <summary>A record of a GiftItem actually being used/fulfilled on an order - not navigated from GiftProgram, so construction is public.</summary>
public sealed class GiftUsage : BaseEntity<Guid>, IAuditable
{
    public Guid GiftItemId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public DateTime UsedAt { get; private set; }

    public GiftItem GiftItem { get; private set; } = default!;

    private GiftUsage() { }

    public static GiftUsage Create(Guid giftItemId, Guid userId, Guid? orderId)
    {
        return new GiftUsage
        {
            Id = Guid.CreateVersion7(),
            GiftItemId = giftItemId,
            UserId = userId,
            OrderId = orderId,
            UsedAt = DateTime.UtcNow,
        };
    }
}
