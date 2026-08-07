namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>A priced offer for a ProductBundle - reuses the shared Money Value Object, same split (Currency scalar + currency-less Money) already used by Voucher/CampaignBudget. Not navigated from ProductBundle, so construction is public.</summary>
public sealed class BundlePrice : BaseEntity<Guid>, IAuditable
{
    public Guid BundleId { get; private set; }
    public Currency Currency { get; private set; } = default!;
    public Money Price { get; private set; } = default!;

    private BundlePrice() { }

    public static BundlePrice Create(Guid bundleId, Currency currency, Money price)
    {
        return new BundlePrice
        {
            Id = Guid.CreateVersion7(),
            BundleId = bundleId,
            Currency = currency,
            Price = price,
        };
    }

    public void UpdatePrice(Money price)
    {
        Price = price;
    }
}
