using NovaCore.BuildingBlock.Saga.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Reliability.Saga;

/// <summary>
/// Persistent ISagaStore for CreateOrderSaga, registered as a singleton (matching
/// SagaOrchestrator's own lifetime - see NovaCore.BuildingBlock.Saga.Extensions.SagaExtensions) but
/// OrderDbContext is scoped, so every method opens its own DI scope rather than taking
/// OrderDbContext as a constructor dependency. Purely observability/audit - see
/// SagaExecutionRecordEntity's remarks for why correctness never depends on this table.
/// </summary>
public sealed class EfSagaStore(IServiceScopeFactory scopeFactory) : ISagaStore
{
    public async Task SaveAsync(SagaExecutionRecord record, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var existing = await db.SagaExecutionRecords.FirstOrDefaultAsync(r => r.SagaId == record.SagaId, ct);
        if (existing is null)
            db.SagaExecutionRecords.Add(SagaExecutionRecordEntity.FromRecord(record));
        else
            existing.Apply(record);

        await db.SaveChangesAsync(ct);
    }

    public async Task<SagaExecutionRecord?> LoadAsync(string sagaId, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var entity = await db.SagaExecutionRecords.AsNoTracking().FirstOrDefaultAsync(r => r.SagaId == sagaId, ct);
        return entity?.ToRecord();
    }

    public async Task<IReadOnlyList<SagaExecutionRecord>> GetHistoryAsync(string sagaName, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var entities = await db.SagaExecutionRecords
            .AsNoTracking()
            .Where(r => r.SagaName == sagaName)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(ct);

        return [.. entities.Select(e => e.ToRecord())];
    }

    public async Task<IReadOnlyList<SagaExecutionRecord>> GetFailedSagasAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var entities = await db.SagaExecutionRecords
            .AsNoTracking()
            .Where(r => r.State == (int)SagaExecutionState.Failed)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(ct);

        return [.. entities.Select(e => e.ToRecord())];
    }
}
