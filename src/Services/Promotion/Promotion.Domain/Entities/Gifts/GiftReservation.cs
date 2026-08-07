namespace NovaCore.Promotion.Domain.Entities.Gifts;

/// <summary>A temporary hold on a GiftItem's stock while checkout is in progress - Quantity reuses the shared BuildingBlock Value Object. Not navigated from GiftProgram, so construction is public.</summary>
public sealed class GiftReservation : BaseEntity<Guid>, IAuditable
{
    public Guid GiftItemId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public Quantity ReservedQuantity { get; private set; } = default!;
    public DateTime ReservedAt { get; private set; }

    public GiftItem GiftItem { get; private set; } = default!;
    public ICollection<GiftClaim> Claims { get; private set; } = [];

    private GiftReservation() { }

    public static GiftReservation Create(Guid giftItemId, Guid userId, Quantity reservedQuantity, Guid? orderId = null)
    {
        return new GiftReservation
        {
            Id = Guid.CreateVersion7(),
            GiftItemId = giftItemId,
            UserId = userId,
            OrderId = orderId,
            ReservedQuantity = reservedQuantity,
            ReservedAt = DateTime.UtcNow,
        };
    }
}
