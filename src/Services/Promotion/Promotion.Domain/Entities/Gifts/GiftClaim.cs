namespace NovaCore.Promotion.Domain.Entities.Gifts;

/// <summary>A record of a GiftReservation being claimed - not navigated from GiftProgram, so construction is public.</summary>
public sealed class GiftClaim : BaseEntity<Guid>, IAuditable
{
    public Guid ReservationId { get; private set; }
    public DateTime ClaimedAt { get; private set; }

    public GiftReservation Reservation { get; private set; } = default!;

    private GiftClaim() { }

    public static GiftClaim Create(Guid reservationId)
    {
        return new GiftClaim
        {
            Id = Guid.CreateVersion7(),
            ReservationId = reservationId,
            ClaimedAt = DateTime.UtcNow,
        };
    }
}
