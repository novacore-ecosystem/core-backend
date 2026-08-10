using NovaCore.BuildingBlock.Saga.Abstractions;

namespace NovaCore.BuildingBlock.Saga.Core;

/// <summary>
/// In-memory implementation of ISagaStore for development and testing.
/// Data is lost when the application restarts.
/// For production, implement a persistent store (database-backed).
/// </summary>
public sealed class InMemorySagaStore : ISagaStore
{
    private readonly Dictionary<string, SagaExecutionRecord> _records = [];

    public Task SaveAsync(SagaExecutionRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        _records[record.SagaId] = record;
        return Task.CompletedTask;
    }

    public Task<SagaExecutionRecord?> LoadAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        _records.TryGetValue(sagaId, out var record);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<SagaExecutionRecord>> GetHistoryAsync(string sagaName, CancellationToken cancellationToken = default)
    {
        var history = _records.Values
            .Where(r => r.SagaName == sagaName)
            .OrderByDescending(r => r.StartedAt)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<SagaExecutionRecord>>(history);
    }

    public Task<IReadOnlyList<SagaExecutionRecord>> GetFailedSagasAsync(CancellationToken cancellationToken = default)
    {
        var failed = _records.Values
            .Where(r => r.State == SagaExecutionState.Failed)
            .OrderByDescending(r => r.StartedAt)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<SagaExecutionRecord>>(failed);
    }

    public void Clear() => _records.Clear();
}
