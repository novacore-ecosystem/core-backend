namespace NovaCore.Promotion.Domain.Entities.Recommendations;

/// <summary>An append-only audit trail row for a RecommendationProgram - CreatedAt is inherited from BaseEntity, not redeclared. Construction stays public even though it is now navigated, since RecommendationProgram does not own it via an Add-style method.</summary>
public sealed class RecommendationHistory : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    public RecommendationProgram Program { get; private set; } = default!;

    private RecommendationHistory() { }

    public static RecommendationHistory Create(Guid programId, string action, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("History action cannot be empty.");

        return new RecommendationHistory
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
