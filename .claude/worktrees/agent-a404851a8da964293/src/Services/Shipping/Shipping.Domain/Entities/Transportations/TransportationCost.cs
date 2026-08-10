namespace NovaCore.Shipping.Domain.Entities.Transportations;

/// <summary>One cost line of a Transportation, bucketed by CostCategory. Own table/PK, FK back to Transportation; only Transportation may construct one (it also keeps TotalCost in sync).</summary>
public sealed class TransportationCost : BaseEntity<long>, IAuditable
{
    public Guid TransportationId { get; private set; }
    public CostCategory Category { get; private set; }
    public Money Amount { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime IncurredAt { get; private set; }

    private TransportationCost() { }

    internal static TransportationCost Create(
        Guid transportationId,
        CostCategory category,
        Money amount,
        string? description = null)
    {
        return new TransportationCost
        {
            TransportationId = transportationId,
            Category = category,
            Amount = amount,
            Description = description?.Trim(),
            IncurredAt = DateTime.UtcNow,
        };
    }
}
