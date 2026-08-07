namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>A structural rule under a ProductBundle - Configuration is an opaque string blob, no rule evaluation logic lives here. Not navigated from ProductBundle, so construction is public.</summary>
public sealed class BundleRule : BaseEntity<Guid>, IAuditable
{
    public Guid BundleId { get; private set; }
    public string RuleType { get; private set; } = string.Empty;
    public string? Configuration { get; private set; }

    public ProductBundle Bundle { get; private set; } = default!;

    private BundleRule() { }

    public static BundleRule Create(Guid bundleId, string ruleType, string? configuration)
    {
        if (string.IsNullOrWhiteSpace(ruleType))
            throw ExceptionFactory.RequiredField("Rule type cannot be empty.");

        return new BundleRule
        {
            Id = Guid.CreateVersion7(),
            BundleId = bundleId,
            RuleType = ruleType,
            Configuration = configuration,
        };
    }
}
