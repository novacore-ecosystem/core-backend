namespace NovaCore.BuildingBlock.Saga.Abstractions;

/// <summary>
/// Represents a single step in a saga workflow.
/// A saga step defines the action to execute and compensation to rollback on failure.
/// </summary>
public interface ISagaStep
{
    /// <summary>
    /// Unique identifier for this step in the saga.
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Executes the forward action of this step.
    /// Should throw an exception if execution fails.
    /// </summary>
    Task ExecuteAsync(ISagaContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes compensation logic to rollback the step.
    /// Called when a later step fails, allowing cleanup of this step's work.
    /// Should handle failures gracefully and not throw exceptions that fail the entire compensation.
    /// </summary>
    Task CompensateAsync(ISagaContext context, CancellationToken cancellationToken = default);
}
