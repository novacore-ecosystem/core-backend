namespace NovaCore.Promotion.Domain.Entities.Validations;

/// <summary>A named what-if simulation run - CreatedAt is inherited from BaseEntity, not redeclared. No Navigation section given, so its Scenarios (PromotionSimulationScenario) are related by SimulationId only, not owned here.</summary>
public sealed class PromotionSimulation : BaseEntity<Guid>, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public Guid? CreatedBy { get; private set; }

    public ICollection<PromotionSimulationScenario> Scenarios { get; private set; } = [];

    private PromotionSimulation() { }

    public static PromotionSimulation Create(string name, Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Simulation name cannot be empty.");

        return new PromotionSimulation
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreatedBy = createdBy,
        };
    }
}
