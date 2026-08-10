using System.Text.Json;

using NovaCore.BuildingBlock.Saga.Abstractions;

namespace NovaCore.Order.Persistence.Reliability.Saga;

/// <summary>
/// EF row backing EfSagaStore - a plain persistence-layer record, not a domain aggregate (SagaId
/// is CreateOrderSagaConsumer's own string id, not a Guid; nothing here is loaded through
/// IRepository/IUnitOfWork's business-transaction path). ContextData round-trips through JSON for
/// history/audit purposes only - see docs/reference/create-order-saga.md's idempotency notes for
/// why saga *safety* never actually depends on this table (Outbox/Inbox already guarantee that).
/// </summary>
public sealed class SagaExecutionRecordEntity
{
    public string SagaId { get; set; } = string.Empty;
    public string SagaName { get; set; } = string.Empty;
    public int State { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
    public string CompletedStepsJson { get; set; } = "[]";
    public string ContextDataJson { get; set; } = "{}";
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public static SagaExecutionRecordEntity FromRecord(SagaExecutionRecord record)
    {
        var entity = new SagaExecutionRecordEntity { SagaId = record.SagaId };
        entity.Apply(record);
        return entity;
    }

    public void Apply(SagaExecutionRecord record)
    {
        SagaName = record.SagaName;
        State = (int)record.State;
        StartedAt = record.StartedAt;
        CompletedAt = record.CompletedAt;
        FailureReason = record.FailureReason;
        CompletedStepsJson = JsonSerializer.Serialize(record.CompletedSteps, JsonOptions);
        ContextDataJson = JsonSerializer.Serialize(record.ContextData, JsonOptions);
        CorrelationId = record.CorrelationId;
        UserId = record.UserId;
    }

    public SagaExecutionRecord ToRecord() => new()
    {
        SagaId = SagaId,
        SagaName = SagaName,
        State = (SagaExecutionState)State,
        StartedAt = StartedAt,
        CompletedAt = CompletedAt,
        FailureReason = FailureReason,
        CompletedSteps = JsonSerializer.Deserialize<List<string>>(CompletedStepsJson) ?? [],
        ContextData = JsonSerializer.Deserialize<Dictionary<string, object?>>(ContextDataJson, JsonOptions) ?? [],
        CorrelationId = CorrelationId,
        UserId = UserId,
    };
}
