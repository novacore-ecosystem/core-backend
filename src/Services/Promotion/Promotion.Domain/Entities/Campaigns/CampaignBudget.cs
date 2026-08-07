namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>
/// A Campaign's allocated/spent budget. Related to Campaign via CampaignId + Campaign.BudgetId
/// (not a Campaign navigation collection - not listed in the requested Navigation set) - own
/// surrogate Id since a campaign could accumulate budget history across revisions (same
/// independent-lifecycle justification UserRoleAssignment already documents for its own Id).
/// No spend calculation lives here - RecordSpend is a structural setter, not an accumulator.
/// </summary>
public sealed class CampaignBudget : BaseEntity<Guid>, IAuditable
{
    public Guid CampaignId { get; private set; }
    public Money AllocatedAmount { get; private set; } = default!;
    public Money SpentAmount { get; private set; } = default!;
    public Currency CurrencyCode { get; private set; } = default!;

    private CampaignBudget() { }

    public static CampaignBudget Create(Guid campaignId, Money allocatedAmount, Currency currencyCode)
    {
        return new CampaignBudget
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            AllocatedAmount = allocatedAmount,
            SpentAmount = Money.Create(0),
            CurrencyCode = currencyCode,
        };
    }

    public void UpdateAllocation(Money allocatedAmount)
    {
        AllocatedAmount = allocatedAmount;
    }

    public void RecordSpend(Money spentAmount)
    {
        SpentAmount = spentAmount;
    }
}
