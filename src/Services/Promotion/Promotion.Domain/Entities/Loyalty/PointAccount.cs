namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A user's point balance under a LoyaltyProgram - no earn/spend calculation lives here, see UpdateBalances. Balances reuse the shared BuildingBlock Quantity Value Object (non-negative counts, same as GiftInventory.AvailableQuantity).</summary>
public sealed class PointAccount : BaseEntity<Guid>, IAuditable
{
    public Guid UserId { get; private set; }
    public Guid ProgramId { get; private set; }
    public Quantity AvailablePoints { get; private set; } = default!;
    public Quantity PendingPoints { get; private set; } = default!;
    public Quantity ExpiredPoints { get; private set; } = default!;
    public Quantity LifetimeEarned { get; private set; } = default!;
    public Quantity LifetimeSpent { get; private set; } = default!;

    private PointAccount() { }

    /// <summary>Only LoyaltyProgram may construct a PointAccount - see LoyaltyProgram.AddAccount.</summary>
    internal static PointAccount Create(Guid programId, Guid userId)
    {
        var zero = Quantity.Create(0);

        return new PointAccount
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            UserId = userId,
            AvailablePoints = zero,
            PendingPoints = zero,
            ExpiredPoints = zero,
            LifetimeEarned = zero,
            LifetimeSpent = zero,
        };
    }

    /// <summary>Structural balance setters only - no earn/spend/expiry calculation lives here.</summary>
    public void UpdateBalances(Quantity availablePoints, Quantity pendingPoints, Quantity expiredPoints, Quantity lifetimeEarned, Quantity lifetimeSpent)
    {
        AvailablePoints = availablePoints;
        PendingPoints = pendingPoints;
        ExpiredPoints = expiredPoints;
        LifetimeEarned = lifetimeEarned;
        LifetimeSpent = lifetimeSpent;
    }
}
