namespace NovaCore.BuildingBlock.Saga.Abstractions;

/// <summary>
/// Persists saga execution state for reliability and auditability.
/// Enables saga recovery and provides saga history.
/// </summary>
public interface ISagaStore
{
    /// <summary>
    /// Save saga execution state.
    /// </summary>
    Task SaveAsync(SagaExecutionRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load saga execution state by ID.
    /// </summary>
    Task<SagaExecutionRecord?> LoadAsync(string sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get execution history of a saga.
    /// </summary>
    Task<IReadOnlyList<SagaExecutionRecord>> GetHistoryAsync(string sagaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all failed sagas for recovery.
    /// </summary>
    Task<IReadOnlyList<SagaExecutionRecord>> GetFailedSagasAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Record of a saga execution for persistence and auditing.
/// </summary>
public class SagaExecutionRecord
{
    public required string SagaId { get; set; }
    public required string SagaName { get; set; }
    public required SagaExecutionState State { get; set; }
    public required DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
    public required List<string> CompletedSteps { get; set; } = [];
    public required Dictionary<string, object?> ContextData { get; set; } = [];
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
}
