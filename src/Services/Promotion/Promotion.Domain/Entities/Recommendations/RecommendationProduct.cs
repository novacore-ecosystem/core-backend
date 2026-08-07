namespace NovaCore.Promotion.Domain.Entities.Recommendations;

/// <summary>A product entry under a RecommendationProgram - Score is a plain decimal weight, no ranking/scoring calculation lives here (see RecommendationScore for the standalone signal record).</summary>
public sealed class RecommendationProduct : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Score { get; private set; }
    public int DisplayOrder { get; private set; }

    public RecommendationProgram Program { get; private set; } = default!;

    private RecommendationProduct() { }

    /// <summary>Only RecommendationProgram may construct a RecommendationProduct - see RecommendationProgram.AddProduct.</summary>
    internal static RecommendationProduct Create(Guid programId, Guid productId, decimal score, int displayOrder)
    {
        return new RecommendationProduct
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            ProductId = productId,
            Score = score,
            DisplayOrder = displayOrder,
        };
    }

    public void UpdateScore(decimal score)
    {
        Score = score;
    }
}
