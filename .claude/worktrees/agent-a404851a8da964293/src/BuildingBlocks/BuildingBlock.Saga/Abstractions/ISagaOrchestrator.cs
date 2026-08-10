namespace NovaCore.BuildingBlock.Saga.Abstractions;

/// <summary>
/// Orchestrates the execution of a saga workflow.
/// Handles step execution, error handling, and compensation.
/// </summary>
public interface ISagaOrchestrator
{
    /// <summary>
    /// Execute a saga workflow with the given definition and context.
    /// </summary>
    /// <param name="sagaDefinition">The saga workflow to execute.</param>
    /// <param name="context">The saga execution context with input data.</param>
    /// <param name="cancellationToken">Cancellation token for the workflow.</param>
    /// <returns>The completed context containing results and state.</returns>
    Task<ISagaContext> ExecuteAsync(
        ISagaDefinition sagaDefinition,
        ISagaContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when a saga fails execution.
/// </summary>
public class SagaExecutionException : Exception
{
    public string SagaName { get; }
    public string FailedStepName { get; }
    public string SagaId { get; }

    public SagaExecutionException(
        string sagaName,
        string failedStepName,
        string sagaId,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        SagaName = sagaName;
        FailedStepName = failedStepName;
        SagaId = sagaId;
    }
}
