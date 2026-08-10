namespace NovaCore.BuildingBlock.Saga.Abstractions;

/// <summary>
/// Defines the workflow for a saga, including all steps and their order.
/// </summary>
public interface ISagaDefinition
{
    /// <summary>
    /// Unique identifier for this saga type.
    /// Example: "OrderCreationSaga", "RefundSaga"
    /// </summary>
    string SagaName { get; }

    /// <summary>
    /// Human-readable description of the saga workflow.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Get all saga steps in execution order.
    /// </summary>
    IReadOnlyList<ISagaStep> GetSteps();

    /// <summary>
    /// Optional: Get a step by name for direct lookup.
    /// </summary>
    ISagaStep? GetStep(string stepName);
}
