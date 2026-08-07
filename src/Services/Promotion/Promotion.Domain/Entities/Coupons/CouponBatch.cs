namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>
/// A generation/import batch that one or more Coupons/CouponCodes can reference via BatchId - not
/// owned by a single Coupon (a batch precedes and can spawn many coupons), so construction is
/// public, not internal, unlike Coupon's owned children.
/// </summary>
public sealed class CouponBatch : BaseEntity<Guid>, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Source { get; private set; }
    public DateTime? ImportedAt { get; private set; }
    public int TotalCount { get; private set; }
    public int ActivatedCount { get; private set; }
    public int UsedCount { get; private set; }
    public int FailedCount { get; private set; }

    public ICollection<Coupon> Coupons { get; private set; } = [];
    public ICollection<CouponCode> Codes { get; private set; } = [];

    private CouponBatch() { }

    public static CouponBatch Create(string name, string? source = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Batch name cannot be empty.");

        return new CouponBatch
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Source = source,
        };
    }

    /// <summary>Structural counter update only - no import/activation logic lives here.</summary>
    public void RecordImport(DateTime importedAt, int totalCount)
    {
        ImportedAt = importedAt;
        TotalCount = totalCount;
    }

    public void UpdateCounts(int activatedCount, int usedCount, int failedCount)
    {
        ActivatedCount = activatedCount;
        UsedCount = usedCount;
        FailedCount = failedCount;
    }
}
