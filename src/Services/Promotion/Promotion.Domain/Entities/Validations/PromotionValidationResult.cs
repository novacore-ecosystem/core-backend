namespace NovaCore.Promotion.Domain.Entities.Validations;

/// <summary>The outcome of running a PromotionValidationPolicy against something - related by PolicyId only, no evaluation logic lives here.</summary>
public sealed class PromotionValidationResult : BaseEntity<Guid>, IAuditable
{
    public Guid PolicyId { get; private set; }
    public ValidationResultStatus Status { get; private set; }
    public string? Message { get; private set; }

    public PromotionValidationPolicy Policy { get; private set; } = default!;

    private PromotionValidationResult() { }

    public static PromotionValidationResult Create(Guid policyId, ValidationResultStatus status, string? message = null)
    {
        return new PromotionValidationResult
        {
            Id = Guid.CreateVersion7(),
            PolicyId = policyId,
            Status = status,
            Message = message,
        };
    }
}
