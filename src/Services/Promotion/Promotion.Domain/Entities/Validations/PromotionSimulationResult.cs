namespace NovaCore.Promotion.Domain.Entities.Validations;

/// <summary>The outcome of running a PromotionSimulationScenario - Output is an opaque string blob, no simulation execution logic lives here.</summary>
public sealed class PromotionSimulationResult : BaseEntity<Guid>, IAuditable
{
    public Guid ScenarioId { get; private set; }
    public string? Output { get; private set; }
    public SimulationResultStatus Status { get; private set; }

    private PromotionSimulationResult() { }

    public static PromotionSimulationResult Create(Guid scenarioId, SimulationResultStatus status, string? output = null)
    {
        return new PromotionSimulationResult
        {
            Id = Guid.CreateVersion7(),
            ScenarioId = scenarioId,
            Status = status,
            Output = output,
        };
    }
}
