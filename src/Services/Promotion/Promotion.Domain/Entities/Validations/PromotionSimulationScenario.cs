namespace NovaCore.Promotion.Domain.Entities.Validations;

/// <summary>A single input scenario under a PromotionSimulation - Input is an opaque string blob, no scenario evaluation logic lives here.</summary>
public sealed class PromotionSimulationScenario : BaseEntity<Guid>, IAuditable
{
    public Guid SimulationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Input { get; private set; }

    public PromotionSimulation Simulation { get; private set; } = default!;
    public ICollection<PromotionSimulationResult> Results { get; private set; } = [];

    private PromotionSimulationScenario() { }

    public static PromotionSimulationScenario Create(Guid simulationId, string name, string? input = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Scenario name cannot be empty.");

        return new PromotionSimulationScenario
        {
            Id = Guid.CreateVersion7(),
            SimulationId = simulationId,
            Name = name,
            Input = input,
        };
    }
}
