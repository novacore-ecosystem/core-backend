using NovaCore.BuildingBlock.Saga.Abstractions;

namespace NovaCore.BuildingBlock.Saga.Core;

/// <summary>
/// Default implementation of ISagaContext for data sharing across saga steps.
/// </summary>
public sealed class SagaContext : ISagaContext
{
    private readonly Dictionary<string, object?> _data = [];
    private readonly List<string> _completedSteps = [];
    private readonly SagaMetadata _metadata;

    public string SagaId { get; }
    public string? CorrelationId { get; }
    public ISagaMetadata Metadata => _metadata;
    public SagaExecutionState State { get; set; }

    public SagaContext(
        string sagaId,
        string? correlationId = null,
        Dictionary<string, object?>? initialData = null,
        IReadOnlyDictionary<string, string>? contextData = null)
    {
        SagaId = sagaId;
        CorrelationId = correlationId;
        State = SagaExecutionState.Pending;

        if (initialData != null)
        {
            foreach (var kvp in initialData)
                _data[kvp.Key] = kvp.Value;
        }

        _metadata = new SagaMetadata(contextData ?? new Dictionary<string, string>());
    }

    public T? Get<T>(string key)
    {
        if (_data.TryGetValue(key, out var value))
            return (T?)value;
        return default;
    }

    public void Set<T>(string key, T value)
    {
        _data[key] = value;
    }

    public IReadOnlyDictionary<string, object?> GetAllData() => _data.AsReadOnly();

    public void MarkStepCompleted(string stepName)
    {
        if (!_completedSteps.Contains(stepName))
            _completedSteps.Add(stepName);
    }

    public IReadOnlyList<string> GetCompletedSteps() => _completedSteps.AsReadOnly();

    private sealed class SagaMetadata : ISagaMetadata
    {
        public string? UserId { get; set; }
        public DateTime StartedAt { get; }
        public DateTime? CompletedAt { get; set; }
        public IReadOnlyDictionary<string, string> ContextData { get; }

        public SagaMetadata(IReadOnlyDictionary<string, string> contextData)
        {
            StartedAt = DateTime.UtcNow;
            ContextData = contextData;
        }
    }
}
