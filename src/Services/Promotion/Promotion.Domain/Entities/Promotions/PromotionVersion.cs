namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>An append-only version snapshot marker for a Promotion, recorded whenever Promotion.IncrementVersion runs. No real snapshot payload yet - structural placeholder for the version history.</summary>
public sealed class PromotionVersion : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public int VersionNumber { get; private set; }
    public string? Note { get; private set; }

    public PromotionEntity Promotion { get; private set; } = default!;

    private PromotionVersion() { }

    /// <summary>Only Promotion may construct a PromotionVersion - see Promotion.AddVersion.</summary>
    internal static PromotionVersion Create(Guid promotionId, int versionNumber, string? note)
    {
        return new PromotionVersion
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            VersionNumber = versionNumber,
            Note = note,
        };
    }
}
