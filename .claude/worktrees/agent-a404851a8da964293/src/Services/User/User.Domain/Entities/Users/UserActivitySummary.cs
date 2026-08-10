namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 extension of User holding denormalized behavioral counters, fed by integration
/// events from Auth/Order rather than computed on read. FavoriteCategory is an observed stat
/// (the category the user buys from most) - distinct from UserPreference.FavoriteCategories,
/// which is the user's own explicit, self-curated list.
/// </summary>
public sealed class UserActivitySummary : BaseEntity, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastOrderAt { get; private set; }
    public DateTime? LastPurchaseAt { get; private set; }
    public int TotalLoginCount { get; private set; }
    public int TotalOrderCount { get; private set; }
    public decimal TotalSpentAmount { get; private set; }
    public Guid? FavoriteCategory { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserActivitySummary() { }

    public static UserActivitySummary Create(Guid userId)
    {
        return new UserActivitySummary
        {
            UserId = userId,
        };
    }

    // ============================================================================
    // Activity counters
    // Records login/order/purchase events by bumping the relevant counter and
    // timestamp. Additive only - counters never decrease, since they represent
    // a lifetime history, not a current-state snapshot.
    // ============================================================================

    #region Activity counters

    public void RecordLogin()
    {
        TotalLoginCount++;
        LastLoginAt = DateTime.UtcNow;
    }

    public void RecordOrder()
    {
        TotalOrderCount++;
        LastOrderAt = DateTime.UtcNow;
    }

    public void RecordPurchase(decimal amount)
    {
        if (amount < 0)
            throw ExceptionFactory.InvalidRange("Purchase amount cannot be negative.");

        TotalSpentAmount += amount;
        LastPurchaseAt = DateTime.UtcNow;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // The single observed favorite-category stat.
    // ============================================================================

    #region Details & lifecycle

    public void SetFavoriteCategory(Guid? categoryId)
    {
        FavoriteCategory = categoryId;
    }

    #endregion
}
