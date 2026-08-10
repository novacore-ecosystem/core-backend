namespace NovaCore.BuildingBlock.Saga.Abstractions;

/// <summary>
/// Provides context and data sharing across all steps in a saga workflow.
/// Similar to HttpContext but for saga execution.
/// </summary>
public interface ISagaContext
{
    /// <summary>
    /// Unique identifier for this saga execution.
    /// </summary>
    string SagaId { get; }

    /// <summary>
    /// Optional request/correlation ID for tracing across services.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Saga metadata and configuration.
    /// </summary>
    ISagaMetadata Metadata { get; }

    /// <summary>
    /// Get a value from the context by key.
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Set a value in the context.
    /// </summary>
    void Set<T>(string key, T value);

    /// <summary>
    /// Get all context data.
    /// </summary>
    IReadOnlyDictionary<string, object?> GetAllData();

    /// <summary>
    /// Current execution state of the saga.
    /// </summary>
    SagaExecutionState State { get; set; }

    /// <summary>
    /// Track which steps have completed (for compensation).
    /// </summary>
    void MarkStepCompleted(string stepName);

    /// <summary>
    /// Get list of completed steps for compensation.
    /// </summary>
    IReadOnlyList<string> GetCompletedSteps();
}

/// <summary>
/// Execution state of a saga.
/// </summary>
public enum SagaExecutionState
{
    Pending,
    Running,
    Succeeded,
    Failed,
    Compensating,
    Compensated
}

/// <summary>
/// Metadata about a saga execution.
/// </summary>
public interface ISagaMetadata
{
    /// <summary>
    /// User ID or service identity performing the saga.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Timestamp when saga started.
    /// </summary>
    DateTime StartedAt { get; }

    /// <summary>
    /// Timestamp when saga completed.
    /// </summary>
    DateTime? CompletedAt { get; }

    /// <summary>
    /// Additional context data (headers, user claims, etc).
    /// </summary>
    IReadOnlyDictionary<string, string> ContextData { get; }
}
