# Reference: Inbox/Outbox Runtime Flow

**Scope:** The complete runtime implementation of the Outbox Relay and Inbox Processing, from the command handler that enqueues an integration event to successful consumer handling.

## The Complete Runtime Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│ SERVICE A: Auth/User                                                │
└─────────────────────────────────────────────────────────────────────┘

1. DOMAIN LOGIC
   Command Handler calls: aggregate.Create() / a behavior method on an existing aggregate
   Handler now holds the data needed to describe what happened (no event raised by the entity itself —
   BuildingBlock.Domain.Abstractions.AggregateRoot<TId> is a plain marker class, not an event source)

2. SAME HANDLER BUILDS THE INTEGRATION EVENT DIRECTLY
   Handler constructs the IntegrationEvent DTO from data it already has (its own request + the
   entity it just created/mutated) and calls: await outboxStore.EnqueueAsync(integrationEvent, ct)
   → Adds an OutboxMessage row to the DbContext (NOT yet persisted)
   There is no domain-event hop and no separate Infrastructure-layer handler in between —
   see CreateProductHandler/UpdateProductHandler/DeleteProductHandler
   (Product.Application/Features/Products/Commands/) for the reference shape.

3. PERSISTENCE (Same transaction)
   Handler calls: await unitOfWork.SaveChangesAsync(ct)
   └─ DbContext.SaveChangesAsync() → aggregate change + OutboxMessage row commit together, in one transaction

4. BACKGROUND: OUTBOX RELAY (OutboxRelayHostedService)
   Runs on a PeriodicTimer (every N seconds, configurable)
   ├─ Query: OutboxMessages where ProcessedAt IS NULL
   ├─ For each message in batch:
   │  ├─ Deserialize payload to IntegrationEvent
   │  ├─ Call IOutboxPublisher.PublishOutboxMessageAsync(...)
   │  │  └─ IProducerAccessor.ProduceAsync() → Kafka with headers:
   │     ├─ "event-type": EventType name
   │     ├─ "correlation-id": CorrelationId (from event)
   │     └─ "message-id": OutboxMessage.Id (for Inbox dedup)
   │  └─ Update: OutboxMessage.MarkProcessed() → ProcessedAt = Now
   │  └─ Catch exceptions: OutboxMessage.MarkFailed(error), retry logic
   └─ Repeat

   Configuration (appsettings.json):
   {
     "Outbox": {
       "Relay": {
         "PollingInterval": "00:00:05",  // Poll every 5 seconds
         "BatchSize": 10,                // Fetch 10 at a time
         "MaxRetries": 3,                // Give up after 3 attempts
         "RetryDelay": "00:00:05"        // Wait 5s between retries
       }
     }
   }

┌─────────────────────────────────────────────────────────────────────┐
│ KAFKA                                                               │
└─────────────────────────────────────────────────────────────────────┘

6. KAFKA TOPIC: usercreated (lowercased event type name only, no service prefix - see
   KafkaFlowEventPublisher.GenerateTopicName)
   Message persisted with partition key = CorrelationId
   Headers preserved: event-type, correlation-id, message-id

┌─────────────────────────────────────────────────────────────────────┐
│ SERVICE B: Any consumer                                             │
└─────────────────────────────────────────────────────────────────────┘

7. KAFKA CONSUMER (KafkaFlow)
   KafkaFlowBusHostedService started at application boot
   └─ Polls exactly the topics its own registered IIntegrationEventConsumers declare (topic
      names are global, not service-scoped - a service consuming a topic it also publishes to,
      e.g. Product's Search self-consumption, works via a separate consumer group, see
      reference/search.md)

8. MESSAGE ARRIVAL
   IntegrationEventDispatchHandler (KafkaFlow.IMessageHandler<byte[]>)
   ├─ Decode: bytes → JSON string
   ├─ Extract headers → dict
   └─ Call IntegrationEventConsumerRegistry.DispatchAsync(topic, json, headers, ct)

9. INBOX DEDUP + RETRY (IntegrationEventConsumerRegistry → InboxAttemptExecutor)
   For each IIntegrationEventConsumer whose Topics.Contains(topic):
   ├─ Extract message-id from headers (optional, only from Outbox relay)
   ├─ If message-id absent: call consumer.HandleAsync directly, catch+log, no tracking
   ├─ If message-id present: build an InboxDispatchContext and call the
   │  executeWithInboxAsync delegate (built in BuildingBlock.Infrastructure, backed by
   │  InboxAttemptExecutor - see "Inbox Lifecycle, Retry & Transaction Safety" below)
   │  ├─ IInboxStore.BeginAttemptAsync(...) looks up the row and returns a decision:
   │  │  AlreadyProcessed / DeadLettered / NotDueYet → skip, log, done
   │  │  Proceed → the row is optimistically staged Processed on the DbContext's
   │  │            change tracker (NOT saved yet)
   │  ├─ Call await consumer.HandleAsync(message, headers, ct)
   │  │  └─ Deserialize JSON → IntegrationEvent (throws on failure - no swallowed catch)
   │  │  └─ Build Command
   │  │  └─ Dispatch: await ISender.Send(command, ct)
   │  ├─ On success: IInboxStore.CompleteAttemptAsync flushes the staged marker (a
   │  │  no-op if the handler's own SaveChanges already committed it - see below)
   │  └─ On exception: IInboxStore.FailAttemptAsync(...) - see retry/backoff below

10. CONSUMER COMMAND HANDLER
    YourCommandHandler.Handle(command, ct)
    ├─ Execute business logic
    ├─ May itself enqueue further integration events (repeats from step 2)
    └─ Return result

11. NEW OUTBOX CYCLE (if the consumer's handler enqueued events)
    Repeats steps 2–4, but now Service B is the publisher
    → Cross-service communication established
```

## Inbox Lifecycle, Retry & Transaction Safety

### Lifecycle

```
Pending ──(handler succeeds)──────────────────► Processed
   │
   └─(handler throws, RetryCount < MaxRetryCount)──► Retrying ──┐
                                                       ▲          │ (NextRetryAt arrives,
                                                       │          │  handler retried)
                                                       └──────────┘
                                                       │
                                                       └─(handler throws again,
                                                          RetryCount >= MaxRetryCount)──► DeadLetter
```

`DeadLetter` is a terminal `Status` value on the same `inbox_messages` row — there is no
separate DeadLetter table. A DeadLetter row is never picked up by `InboxRetryHostedService`
and a live Kafka redelivery hitting `BeginAttemptAsync` for that row is skipped too
(`InboxAttemptDecision.DeadLettered`). Management APIs to inspect/requeue DeadLetter rows are a
separate, later piece of work — this implementation only guarantees the row stops retrying and
is clearly identifiable (`Status = 'DeadLetter'`, `LastError` populated).

### Retry metadata

`InboxMessage` (`BuildingBlock.Persistence.Ef/Inbox/InboxMessage.cs`) carries, in addition to the
original `(MessageId, ConsumerName)` dedup key:

| Column | Purpose |
|---|---|
| `Status` | `Pending` / `Retrying` / `Processed` / `DeadLetter` |
| `RetryCount` | Attempts recorded so far (incremented on every failure) |
| `NextRetryAt` | Earliest time `InboxRetryHostedService` (or a live redelivery) may retry; `null` once Processed/DeadLetter |
| `LastRetryAt` | Timestamp of the most recent failed attempt |
| `LastError` | Exception message from the most recent failed attempt |
| `Topic`, `Payload`, `HeadersJson` | Captured on first sight so a background retry can replay the consumer without the original Kafka message still being available |

### Retry policy & scheduling (`InboxRetryOptions`, `Inbox:Retry` config section)

```csharp
public sealed class InboxRetryOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);
    public int BatchSize { get; set; } = 10;
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(10);
    public double RetryBackoffMultiplier { get; set; } = 2.0;
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromMinutes(30);
}
```

`NextRetryAt` is computed as `InitialRetryDelay * RetryBackoffMultiplier ^ (RetryCount - 1)`,
capped at `MaximumRetryDelay` (exponential backoff, no busy-loop retries). The computation lives
on `InboxMessage.MarkFailed`/`ComputeRetryDelay`, using the row's own `RetryCount` - the policy
numbers themselves are supplied by the caller (`InboxRetryOptions.ToPolicy()`) so
`BuildingBlock.Persistence`/`BuildingBlock.Persistence.Ef` stay config-framework-agnostic.

### Background job (`InboxRetryHostedService`)

A `BackgroundService` registered alongside `OutboxRelayHostedService` by the same
`AddInboxOutboxInfrastructure()` call, following the identical `PeriodicTimer` + per-iteration
DI-scope pattern:

```
every PollingInterval:
  IInboxStore.GetDueForRetryAsync(BatchSize)   -- WHERE Status = Retrying AND NextRetryAt <= now
  for each due row (fresh DI scope per row, same as the live Kafka dispatch path):
    resolve the IIntegrationEventConsumer whose GetType().Name == row.ConsumerName
    replay it via the same InboxAttemptExecutor used by the live path,
    using the row's captured Topic/Payload/HeadersJson instead of a fresh Kafka message
```

Processed and DeadLetter rows are never selected (the query filters on `Status = Retrying`);
`BeginAttemptAsync` re-checks status/`NextRetryAt` regardless, so a row that races between a live
Kafka redelivery and a scheduled retry is still handled safely (whichever gets there first wins;
the other sees `AlreadyProcessed`/`NotDueYet` and backs off).

### Transaction boundary & exactly-once completion (the "duplicate side effects" problem)

The previous implementation wrote the Inbox row **after** `consumer.HandleAsync` returned, in a
separate `SaveChangesAsync` call. If the process crashed between the business handler's own
commit and that second write, the Inbox row was never marked processed — the message would be
redelivered and the business logic (already committed) would run again.

The fix reuses the same technique the Outbox side already relies on for its own atomicity
(`IOutboxStore.EnqueueAsync` tracks a row without saving, so the caller's own `SaveChanges`
commits it together with the aggregate change) — applied symmetrically to the inbound side:

1. `EfInboxStore.BeginAttemptAsync` fetches (or creates) the row and immediately calls
   `InboxMessage.MarkProcessed()` on it **before the handler runs**, without saving. This is an
   *optimistic* marker: it assumes success.
2. `IntegrationEventConsumerRegistry`/`InboxAttemptExecutor` then calls the consumer handler,
   which resolves the **same** scoped `DbContext` (KafkaFlow's typed handler runs with
   `InstanceLifetime.Scoped`, one DI scope per message, and `InboxRetryHostedService` opens one
   scope per retried row) as `EfInboxStore`. Whenever that handler's own `IUnitOfWork.SaveChangesAsync`
   / `ExecuteTransactionAsync` flushes, it flushes the *entire* change tracker — including the
   optimistic Inbox marker — in the same database transaction as the business change.
3. On success, `CompleteAttemptAsync` calls `SaveChangesAsync` again — a no-op if the handler
   already flushed it (the entity is already `Unchanged`), otherwise it performs the (only)
   actual save.
4. On failure, `FailAttemptAsync` inspects the tracked entity's `EntityState`:
   - **`Unchanged`** → the handler's own transaction already committed (including the optimistic
     Processed marker) before it threw further downstream. The row is left `Processed`:
     flipping it back to `Retrying` here would cause the next attempt to re-run
     already-committed business logic. This is reported as `InboxFailureOutcome.AlreadyCommitted`
     and logged as a warning — nothing is written.
   - **`Added` / `Modified` / `Detached`** → nothing committed. `MarkFailed` overwrites the
     optimistic marker with the real Retrying/DeadLetter state and a fresh `SaveChangesAsync`
     persists it.

A manual `transaction.RollbackAsync()` does not, by itself, revert EF's in-memory change tracker
back to `Unchanged`/`Detached` — entities added or modified inside the rolled-back transaction
stay marked `Added`/`Modified` even though nothing is actually in the database anymore. This was
a latent gap in `EfUnitOfWork.ExecuteTransactionAsync`'s catch block (rollback without a tracker
reset) that would have made the `EntityState` check above unreliable, so it was fixed alongside
this feature: the catch block now calls `Context.ChangeTracker.Clear()` after
`RollbackAsync`, and rethrows instead of silently returning `false` — every existing caller
already awaits `ExecuteTransactionAsync` without checking the returned bool, so a swallowed
failure previously meant callers silently proceeded as if the transaction had succeeded. Both
`EfUnitOfWork.ExecuteTransactionAsync` and Audit's Mongo `UnitOfWork.ExecuteTransactionAsync`
(which never had a change tracker to clear, but had the same swallow-and-return-false bug) now
rethrow.

The Mongo provider (`MongoInboxStore`, used only by Audit, which currently registers no
`IIntegrationEventConsumer`s) has no shared change tracker to stage an optimistic marker into —
Mongo writes commit immediately per call, as `Audit.Persistence.UnitOfWork` already documents.
`CompleteAttemptAsync` there writes `Status = Processed` directly once the handler returns, the
same small window that existed before this change; `InboxFailureOutcome.AlreadyCommitted` never
occurs on that provider.

## Entity Ownership

Outbox/Inbox are infrastructure plumbing, not domain concepts — they are not shared entities.
`BuildingBlock.Persistence` (contracts-only, no ORM reference) never sees them as entity
types; it only knows the primitive `IOutboxStore`/`IInboxStore` interfaces plus a plain
`OutboxMessageSnapshot` read-model record. **`BuildingBlock.Persistence.Ef` is the only
place the `OutboxMessage`/`InboxMessage` EF entities exist** — `BuildingBlock.Persistence.Mongo`
(used by [Audit Service](../services/audit-service.md)) defines its own `OutboxDocument`/
`InboxDocument` independently, without touching `BuildingBlock.Persistence` or
`BuildingBlock.Persistence.Ef` at all. See
[03-building-blocks-reference.md](../03-building-blocks-reference.md#persistence-persistenceef-persistencemongo)
for the full layering rationale.

Because Domain must not own infrastructure concerns either, there is no
`BuildingBlock.Domain.Outbox` — the relay and the per-service adapters pass a plain
`OutboxMessageSnapshot` record between layers instead of a Domain entity.

## Dependency Injection Wiring

**In BuildingBlock.Persistence.Ef:**
- `AddEfOutboxStore<TContext>()` → generic EF implementation, owns `OutboxMessage`/`IOutboxDbContext`
- `AddEfInboxStore<TContext>()` → generic EF implementation, owns `InboxMessage`/`IInboxDbContext`
- Both map to `BuildingBlock.Persistence.Outbox.OutboxMessageSnapshot` / primitive params when
  crossing into the `BuildingBlock.Persistence` contract — the EF entity itself never leaks out.

**In Service.Persistence:**
- `OutboxStore` adapter wraps the primitive `BuildingBlock.Persistence.Outbox.IOutboxStore`,
  translates typed events → primitives on write, and maps
  `BuildingBlock.Persistence.Outbox.OutboxMessageSnapshot` →
  `BuildingBlock.Application.Abstractions.Outbox.OutboxMessageSnapshot` on read (two separate
  DTOs, because the dependency direction is `BB.Persistence → BB.Application`, never the reverse)
- `InboxStore` adapter wraps the generic store
- Registered via `AddOutboxAndInbox()`

**In Service.Infrastructure:**
```csharp
services
    .AddKafkaMessaging(configuration, "auth-service")
    .AddInboxOutboxInfrastructure(configuration);
```

**In BuildingBlock.Messaging.Kafka.Extensions:**
- `KafkaOutboxPublisher` (IOutboxPublisher) — publishes with message-id header
- `IntegrationEventConsumerRegistry` constructed with a single Inbox delegate from DI:
  `Func<InboxDispatchContext, Func<Task>, CancellationToken, Task>` (`InboxDispatchContext` is
  defined in `BuildingBlock.Messaging.Abstractions`, so the Messaging project itself never needs
  to reference Application or Persistence)

**In BuildingBlock.Infrastructure.Messaging.Extensions:**
- `AddInboxOutboxInfrastructure()` registers:
  - `OutboxRelayHostedService` (IHostedService)
  - `InboxRetryHostedService` (IHostedService) — same `PeriodicTimer` pattern as the Outbox relay
  - `InboxAttemptExecutor` — shared begin/invoke/complete-or-fail orchestration + logging, used
    by both the live-dispatch delegate and `InboxRetryHostedService`
  - The `executeWithInboxAsync` delegate consumed by `IntegrationEventConsumerRegistry`
  - `InboxRetryOptions` bound from the `Inbox:Retry` configuration section

## Key Guarantees

| Guarantee | Mechanism | Notes |
|-----------|-----------|-------|
| **Atomic writes** | Outbox row + aggregate in same DB transaction | Message never lost, even on crash |
| **Guaranteed delivery** | OutboxRelayHostedService retries failed publishes | See `MaxRetries`, `RetryDelay` config |
| **Idempotent inbound** | Inbox row tracks (messageId, consumerName) + Status | Same message won't trigger same consumer twice |
| **Bounded inbound retries** | RetryCount / MaxRetryCount, DeadLetter is terminal | Never retries forever |
| **Delayed, backed-off retries** | NextRetryAt (exponential backoff), InboxRetryHostedService only picks up due rows | No busy-loop retries |
| **Exactly-once business effect** | Optimistic Processed marker staged on the same DbContext before the handler runs, flushed atomically with the handler's own SaveChanges | See "Transaction boundary & exactly-once completion" above; Mongo provider is best-effort, not atomic |
| **Consumer isolation** | Per-consumer error handling in registry | One bad consumer doesn't block others |
| **Ordered per partition** | Kafka partition key = CorrelationId | Related events stay ordered |

## Troubleshooting

**Outbox messages stuck as unprocessed:**
- Check `OutboxRelayHostedService` is running: look for "Outbox relay hosted service starting" in logs
- Verify Kafka is accessible and brokers are configured in `appsettings.json` under `Kafka:BootstrapServers`
- Check for errors in Outbox relay logs: "Error publishing outbox message {MessageId}"

**Message processed twice (consumer called twice):**
- Kafka consumer group rebalanced or redelivered the same message
- Inbox check failed: verify `IInboxStore.BeginAttemptAsync` is returning `AlreadyProcessed` for
  already-completed rows (check the `inbox_messages`/`InboxMessages` collection's `Status` column)
- message-id header missing: only Outbox relay messages have it; direct publishes don't get dedup/retry

**Outbox rows marked as failed:**
- Check the `Error` column in OutboxMessages table
- Verify event type can be deserialized (Type.GetType needs full assembly-qualified name)
- Check max retry limit in `OutboxRelayOptions.MaxRetries`

**Inbox message stuck Retrying / never reaches DeadLetter:**
- Check `InboxRetryHostedService` is running: look for "Inbox retry hosted service starting" in logs
- Confirm `NextRetryAt` has actually passed (`SELECT * FROM inbox_messages WHERE status = 'Retrying'`)
- Confirm the consumer named in `ConsumerName` is still registered (a renamed/removed consumer
  class means the row can never be replayed - `RetryOneAsync` logs "No registered consumer named ...")

**Inbox message DeadLettered unexpectedly on the very first failure:**
- Check `Inbox:Retry:MaxRetryCount` - a value of `1` dead-letters after a single failed attempt

**Business logic appears to run twice for the same message:**
- Check for "Consumer {ConsumerName} threw ... after its own transaction had already committed"
  warnings - if absent, the handler is genuinely re-running; check whether `HandleAsync` in that
  consumer still swallows exceptions internally (it must let them propagate for retry tracking to
  see the failure at all)
- On the Mongo provider (Audit), this guarantee is best-effort rather than atomic - see the
  Mongo caveat under "Transaction boundary & exactly-once completion"

## Configuration Reference

**appsettings.json:**
```json
{
  "Outbox": {
    "Relay": {
      "PollingInterval": "00:00:05",
      "BatchSize": 10,
      "MaxRetries": 3,
      "RetryDelay": "00:00:05"
    }
  }
}
```

**OutboxRelayOptions (from BuildingBlock.Infrastructure):**
```csharp
public sealed class OutboxRelayOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int BatchSize { get; set; } = 10;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}
```

Lower `PollingInterval` = faster delivery but more database queries.
Higher `BatchSize` = fewer queries but larger transaction windows.

**appsettings.json (Inbox retry):**
```json
{
  "Inbox": {
    "Retry": {
      "PollingInterval": "00:00:10",
      "BatchSize": 10,
      "MaxRetryCount": 5,
      "InitialRetryDelay": "00:00:10",
      "RetryBackoffMultiplier": 2.0,
      "MaximumRetryDelay": "00:30:00"
    }
  }
}
```

**InboxRetryOptions (from BuildingBlock.Infrastructure):**
```csharp
public sealed class InboxRetryOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);
    public int BatchSize { get; set; } = 10;
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(10);
    public double RetryBackoffMultiplier { get; set; } = 2.0;
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromMinutes(30);
}
```

`MaxRetryCount` counts attempts, including the first: with the default of `5`, a message that
fails five times in a row is moved to DeadLetter on the fifth failure. `NextRetryAt` after the
Nth failure is `min(InitialRetryDelay * RetryBackoffMultiplier^(N-1), MaximumRetryDelay)` - with
the defaults above: 10s, 20s, 40s, 80s, then DeadLetter (never reaching 30m).

Configured per-service in `Auth.API`/`User.API`/`Order.API`/`Audit.API`/`Product.API`'s
`appsettings.json`, next to that service's existing `Outbox:Relay` section (Inventory follows
Auth/User/Order/Audit/Product's own existing convention of relying on the code defaults instead).
**Product now has an Inbox** (previously publish-only) — it self-consumes its own integration
events to keep its Elasticsearch Search index in sync, see [reference/search.md](search.md).

## Cleanup

Outbox and Inbox rows accumulate forever unless something deletes them. Two Hangfire recurring
jobs — `OutboxCleanupJob` and `InboxCleanupJob` — periodically delete obsolete rows in batches.

### Layer responsibilities

Each layer only does what it's already responsible for; cleanup adds no new dependency direction:

```
Application (BuildingBlock.Application.Abstractions.Outbox.IOutboxStore/IInboxStore)
  └─ DeleteProcessedBeforeAsync(olderThanUtc, batchSize, ct) added to both contracts
       ↓
BuildingBlock.Persistence (primitive IOutboxStore/IInboxStore)
  └─ same method added to the primitive contract
       ↓
Persistence Provider (BuildingBlock.Persistence.Ef or BuildingBlock.Persistence.Mongo)
  └─ EfOutboxStore<TContext>/EfInboxStore<TContext> implement the DELETE - select up to
     batchSize ids older than the cutoff, then ExecuteDeleteAsync by id.
     MongoOutboxStore<TContext>/MongoInboxStore<TContext> implement the same contract
     independently (select matching ids, then DeleteManyAsync by id-list) - no changes
     to anything above this layer, in either direction.
       ↓
Service *.Persistence adapters (Auth.Persistence, User.Persistence, Audit.Persistence)
  └─ OutboxStore/InboxStore just delegate to the primitive store, same as every other method
       ↓
Infrastructure Scheduler (BuildingBlock.Infrastructure/BackgroundJobs/Cleanup)
  └─ OutboxCleanupJob/InboxCleanupJob (IRecurringJob) resolve the Application-level
     IOutboxStore/IInboxStore via DI - the same interface OutboxRelayHostedService already
     uses - and call only DeleteProcessedBeforeAsync in a loop. No DB logic, no SQL, no
     ORM reference here or anywhere in BuildingBlock.Infrastructure.
       ↓
Service (Auth.Infrastructure / User.Infrastructure)
  └─ Registers the jobs with one call: .AddInboxOutboxCleanupJobs(configuration)
```

`BuildingBlock.Persistence` and `BuildingBlock.Persistence.Ef` never reference Hangfire —
only `BuildingBlock.Infrastructure` does (`Hangfire.Core`/`Hangfire.AspNetCore`/`Hangfire.PostgreSql`,
referenced once there and flowing transitively to every service).

### Execution flow

```
1. Hangfire triggers OutboxCleanupJob.ExecuteAsync (cron: Jobs:OutboxCleanup:CronExpression)
2. If Jobs:OutboxCleanup:Enabled is false: log "disabled, skipping" and return (cron stays
   registered in the dashboard - a no-op run, not a removed job)
3. cutoffUtc = UtcNow - RetentionPeriod
4. Loop up to MaxBatchesPerRun times:
   ├─ deleted = await outboxStore.DeleteProcessedBeforeAsync(cutoffUtc, BatchSize, ct)
   │  └─ EfOutboxStore: selects ids WHERE ProcessedAt IS NOT NULL AND ProcessedAt < cutoffUtc,
   │     ORDER BY ProcessedAt, LIMIT BatchSize, then ExecuteDeleteAsync by id list
   ├─ totalDeleted += deleted
   └─ if deleted < BatchSize: break (nothing left older than the cutoff)
5. Log totalDeleted. Any remainder past MaxBatchesPerRun is picked up on the next scheduled
   run - one execution never holds a Hangfire worker slot indefinitely.
6. On exception: log and swallow (don't rethrow) - the next scheduled run retries; a failed
   batch never surfaces as a Hangfire "failed job" spam source
```

`InboxCleanupJob` is identical, operating on `InboxMessage` rows instead. Since the Inbox retry
upgrade, rows are no longer always already-processed - `Pending`/`Retrying` rows are actively
managed by `InboxRetryHostedService`, and `DeadLetter` rows are a deliberately-kept terminal
record. `DeleteProcessedBeforeAsync` filters on `Status == Processed && ProcessedAt < cutoff`
explicitly, so cleanup only ever removes completed dedup markers - `Pending`/`Retrying`/
`DeadLetter` rows are never touched by cleanup, regardless of age (DeadLetter rows are left for a
future management API to inspect/requeue, per the constraints of this feature).

### Retention policy

- **Outbox**: only rows with `ProcessedAt != null` are eligible - unprocessed/failed-pending
  rows are never touched by cleanup, regardless of age. The Outbox relay (`OutboxRelayHostedService`)
  owns retry/failure handling; cleanup only removes what's already been successfully published.
- **Inbox**: only `Status == Processed` rows are eligible past the retention window, since those
  are the only rows that represent a completed dedup marker no longer needed for anything.
  Retention should be set comfortably longer than the maximum plausible Kafka redelivery window
  for your consumer group, or a very late redelivery could bypass dedup and reprocess a message.
- Both use the `idx_outbox_processed_at` / `idx_inbox_processed_at` indexes (see
  `OutboxConfiguration`/`InboxConfiguration` in `BuildingBlock.Persistence.Ef`) so the cleanup
  query doesn't scan the full table.

### Configuration

**appsettings / docker-compose (env var pattern, same as `RefreshTokenSync`):**

```json
{
  "Jobs": {
    "OutboxCleanup": {
      "JobId": "auth-outbox-cleanup",
      "CronExpression": "0 3 * * *",
      "Queue": "default",
      "Enabled": true,
      "RetentionPeriod": "7.00:00:00",
      "BatchSize": 500,
      "MaxBatchesPerRun": 20
    },
    "InboxCleanup": {
      "JobId": "auth-inbox-cleanup",
      "CronExpression": "0 3 * * *",
      "Queue": "default",
      "Enabled": true,
      "RetentionPeriod": "7.00:00:00",
      "BatchSize": 500,
      "MaxBatchesPerRun": 20
    }
  }
}
```

Each service sets these via docker-compose env vars (`Jobs__OutboxCleanup__*` /
`Jobs__InboxCleanup__*`, see `docker-compose.override.yml` + `.env`), not appsettings.json —
consistent with the rest of `Jobs:*` config. `RetentionPeriod` uses .NET `TimeSpan` format
(`d.hh:mm:ss`).

### Registration

```csharp
// {Service}.Infrastructure/DependencyInjection.cs
services
    .AddBackgroundJobs(configuration)           // Hangfire storage/server + this service's own jobs
    .AddInboxOutboxCleanupJobs(configuration);   // opts into the shared cleanup jobs
```

`AddInboxOutboxCleanupJobs` registers `OutboxCleanupJob`/`InboxCleanupJob` in DI
(`AddScopedByInterfaceAndConcrete<IRecurringJob>`, scoped to the `BuildingBlock.Infrastructure`
assembly) and binds both options sections. `RecurringJobRegistry` (shared, see
`HangfireSchedulingExtensions`) then discovers and schedules them exactly like any other
`IRecurringJob` — no separate wiring path.

### How the Mongo provider implements cleanup

`BuildingBlock.Persistence.Mongo`'s `MongoOutboxStore`/`MongoInboxStore` add
`DeleteProcessedBeforeAsync` to their own store implementing `BuildingBlock.Persistence
.Outbox.IOutboxStore`/`Inbox.IInboxStore` exactly as anticipated: a `Find` filtered by
`ProcessedAt < cutoff` (and `ProcessedAt != null` for Outbox), sorted, `Limit(batchSize)`,
projected down to just the matching ids, then a follow-up `DeleteManyAsync` by that id list
(Mongo's `deleteMany` has no native `LIMIT`, so the batch size is enforced by the preceding
`Find`/`Limit` instead). Nothing above the provider layer changed to make this work —
`OutboxCleanupJob`/`InboxCleanupJob` and the Application-level contracts are provider-agnostic,
and [Audit Service](../services/audit-service.md) uses the shared `AddInboxOutboxCleanupJobs()`
helper unmodified.
