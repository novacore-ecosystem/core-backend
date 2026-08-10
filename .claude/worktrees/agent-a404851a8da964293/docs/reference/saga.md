# Reference: Saga Orchestration

**Scope:** `BuildingBlock.Saga` — an in-process saga/orchestration framework for multi-step, compensable workflows.

> **In use since the CreateOrder saga.** Order Service's CreateOrder workflow (`DeductInventory` → `ConfirmOrder`, with `EfSagaStore` as the persistent `ISagaStore`) is the first and, as of this writing, only consumer — see [reference/create-order-saga.md](create-order-saga.md) for the full worked example (real event flow, compensation, idempotency, failure scenarios). Before reaching for this for a *new* workflow, still confirm you actually have a multi-step, cross-repository (or cross-service) workflow that needs compensation on partial failure — a single transaction via `IUnitOfWork.ExecuteTransactionAsync` (see [04-coding-rules.md#transaction-management](../04-coding-rules.md#transaction-management)) covers the common case and is simpler.

## When it's the right tool

A workflow with multiple steps, potentially across services/aggregates, where a failure partway through needs explicit rollback of the steps that already succeeded (compensating transactions) — e.g. a future Order → reserve Inventory → charge Payment flow where any step can fail and the prior ones must be undone.

## Shape

```csharp
public sealed class ReserveInventoryStep : ISagaStep
{
    public string Name => nameof(ReserveInventoryStep);
    public async Task ExecuteAsync(ISagaContext context, CancellationToken ct) { /* do the work, store results via context.Set<T> */ }
    public async Task CompensateAsync(ISagaContext context, CancellationToken ct) { /* undo it */ }
}

var definition = new SagaDefinitionBuilder()
    .AddStep(new ReserveInventoryStep())
    .AddStep(new ChargePaymentStep())
    .Build();

await sagaOrchestrator.ExecuteAsync(definition, new SagaContext(), ct);
```

`SagaOrchestrator` runs steps sequentially; on any exception it sets state `Failed`, compensates completed steps in **reverse order** (logging, not throwing, on compensation failures), then throws `SagaExecutionException`. An audit record is persisted via `ISagaStore` if one is registered.

## DI

`AddSagaOrchestration()` (in-memory store — **dev-only, data lost on restart**), `AddSagaOrchestration<TStore>()`, or `AddSagaOrchestration(factory)` for a persistent store implementation. If you adopt this for a real workflow, implement a persistent `ISagaStore` first — don't ship the in-memory default.

## Before adopting

- Confirm your workflow genuinely needs orchestration (this is orchestration-based, not choreography — one place controls the sequence). If services should react independently without central coordination, integration events ([reference/events.md](events.md)) are the better fit.
- No service currently depends on this, so there's no existing example to mirror inside this codebase — read the abstractions directly (`BuildingBlock.Saga/Abstractions/`) rather than assuming an established local convention exists beyond what's documented here.
