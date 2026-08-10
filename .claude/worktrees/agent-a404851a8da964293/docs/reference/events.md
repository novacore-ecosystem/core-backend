# Reference: Event Architecture

**Scope:** the two-tier event system in full detail. The binding rule ("which tier to use") is in [02-architecture-rules.md](../02-architecture-rules.md#the-two-tier-event-system) — this doc is the mechanics behind it. Condensed from the former `architecture/EVENT_ARCHITECTURE.md` (archived, see [08-migration-plan.md](../08-migration-plan.md)) and corrected against the actual codebase during the 2026-07-17 documentation audit — the previous version of this doc described a third, "Domain event" tier (`IDomainEvent`/`AggregateRoot.RaiseDomainEvent()`/`DomainEventPublisher`/`IDomainEventHandler<T>`) that **does not exist anywhere in the codebase**: `AggregateRoot<TId>` is a literal empty marker class with no event-raising method, and none of those other types exist. See [decisions/event-messaging-refactor.md](../decisions/event-messaging-refactor.md) for how that tier was scaffolded and later abandoned.

## The two tiers

| Tier | Type | Where raised | Scope | Timing | Handler contract |
|---|---|---|---|---|---|
| **Internal** | `IInternalEvent` | Anywhere in Application layer, explicit `IInternalEventDispatcher.PublishAsync` call | Same service | Synchronous, in-process, MediatR | `IInternalEventHandler<T>` (Application) |
| **Integration** | `IIntegrationEvent` | Application/command handler, via `IOutboxStore.EnqueueAsync` | Cross-service | Async, Kafka, via the transactional Outbox | `IIntegrationEventConsumer` (receiving service, Infrastructure) |

**Naming note:** the DI extension method that wires the Internal-event dispatcher is still named `AddApplicationEventDispatcher()` and the concrete type's doc-comment still says "IApplicationEventDispatcher" (`BuildingBlock.Infrastructure/Events/ApplicationEventDispatcher.cs`) — this is legacy naming left over from before the interfaces were renamed to `IInternalEvent`/`IInternalEventHandler<T>`/`IInternalEventDispatcher`. The type it actually registers is `InternalEventDispatcher : IInternalEventDispatcher`. Use the current interface/type names (`IInternalEvent*`) in new code and when talking about this tier; the file/method name is a known, harmless naming leftover, not a second mechanism.

Use an **Internal event** when a same-service reaction should happen in-process but doesn't need to be tangled into the primary handler logic — e.g. Auth's `Register` flow raises `OnUserRegisteredEvent` (`Auth.Application/Features/Auth/Events/OnUserRegistered/`) to trigger the post-registration gRPC call to User Service, keeping `RegisterHandler` focused on the registration itself.

Use an **Integration event** when another *service* needs to know something happened. There is no "domain event → integration event" hop in this codebase: the command handler that made the change is the same handler that enqueues the integration event, directly.

## Outbound flow: direct Outbox enqueue → Kafka (via Relay)

**The Outbox pattern ensures atomic writes:** integration events are stored in the database as part of the same transaction as the aggregate change. A background relay then publishes them to Kafka with guaranteed delivery.

```
1. Command handler mutates/creates the aggregate (repository call, staged not yet saved)
2. The SAME handler builds the IntegrationEvent DTO directly and calls
   await outboxStore.EnqueueAsync(integrationEvent, ct)
   → adds an OutboxMessage row to the DbContext (not persisted yet)
3. Handler calls await unitOfWork.SaveChangesAsync(ct)
   → aggregate change + OutboxMessage row commit together, in one transaction
4. OutboxRelayHostedService (background service) polls the Outbox table every N seconds:
   a. Fetch unprocessed messages in batches
   b. Deserialize and publish via IOutboxPublisher → Kafka (with message-id header)
   c. Mark as processed on success, or retry with backoff on failure
   d. Log permanent failures after max retries
```

There is no domain-event indirection between steps 1 and 2 — see `CreateProductHandler`/`UpdateProductHandler`/`DeleteProductHandler` (`Product.Application/Features/Products/Commands/`) for the reference shape, or `Order`/`Inventory`/`Auth`/`User`'s equivalent handlers; every service that publishes integration events does it this way. `UserProfileUpdatedIntegrationEvent` (`BuildingBlock.Contract/Events/User/`, added 2026-07-28) is a recent example of a new event added purely to feed a self-consuming Search sync (`User.Infrastructure/Messaging/Consumers/UserProfileUpdatedSearchSyncConsumer.cs`, see [reference/search.md](search.md#user-search)) — before it existed, `UpdateUserHandler` published nothing at all on profile edits, the same "new event added purely for search sync" pattern Product's Category/Tag events already established (see [reference/search.md](search.md#synchronization-flow)). `IEventPublisher`/`KafkaFlowEventPublisher` (`BuildingBlock.Messaging`/`BuildingBlock.Messaging.Kafka`) is a lower-level direct-publish primitive that the Outbox relay itself is built on — **no service calls it directly**; always go through `IOutboxStore.EnqueueAsync`, never `IEventPublisher.PublishAsync`, from feature code.

**Key guarantees:**
- **Atomicity:** OutboxMessage rows are written in the same transaction as the aggregate change — no message loss even if the service crashes
- **Idempotent inbound:** Every published message includes a `message-id` header; consumers check Inbox before processing
- **Retry support:** Failed publishes are retried up to `MaxRetries` with configurable backoff

See [reference/inbox-outbox-runtime.md](inbox-outbox-runtime.md) for the complete runtime flow, retry/dead-letter behavior, and configuration — this doc only covers which tier to reach for and the high-level shape.

## Inbound flow: Kafka → Inbox Dedup → Application Command

**The Inbox pattern prevents duplicate processing:** consumers check an Inbox table (with retry/backoff/dead-letter tracking, not just a boolean) before processing a message.

```
Kafka message arrives with headers (including message-id from the Outbox relay)
  → KafkaFlow IntegrationEventDispatchHandler (BuildingBlock.Messaging.Kafka) decodes bytes→JSON, headers→dict
  → IntegrationEventConsumerRegistry.DispatchAsync(topic, message, headers, ct)
      FOR EACH consumer whose Topics contain this topic:
        a. IInboxStore.BeginAttemptAsync(...) decides AlreadyProcessed / DeadLettered / NotDueYet / Proceed
        b. If Proceed: call consumer.HandleAsync(message, headers, ct)
           → deserialize to event type, build a Command, dispatch via ISender.Send(command, ct)
        c. On success, CompleteAttemptAsync marks the row Processed
        d. On failure, FailAttemptAsync schedules a backed-off retry or moves the row to DeadLetter
           after MaxRetryCount attempts
  → Application Command Handler orchestrates the use case, may enqueue further integration events (repeats the outbound flow)
```

Full retry/backoff/dead-letter mechanics and the exactly-once transaction-safety technique: [reference/inbox-outbox-runtime.md](inbox-outbox-runtime.md#inbox-lifecycle-retry--transaction-safety). Registration and topic-naming rules: [workflows/add-integration-event.md](../workflows/add-integration-event.md).

## Layer responsibilities

| Layer | Responsibility |
|---|---|
| Domain | Execute business logic; enforce invariants inside aggregate methods. Does not raise or know about events of any kind. |
| Application | Command/query handlers orchestrate the use case, mutate the aggregate, and — in the same handler — enqueue any resulting integration event via `IOutboxStore.EnqueueAsync`; dispatch Internal events via `IInternalEventDispatcher` for same-service reactions; implement `IInternalEventHandler<T>` for internal-event reactions. |
| Infrastructure | Consume inbound Kafka messages (`IIntegrationEventConsumer`), translate to a Command, dispatch via `ISender` — no business logic in the consumer itself. Run the Outbox relay / Inbox retry background services. |

## DOs and DON'Ts

**DO** — keep consumers thin (deserialize → dispatch a Command, nothing else); make consumers idempotent even beyond Inbox dedup where cheap to do (see `User.Application`'s `CreateUserCommandHandler` check-then-create-then-recheck pattern, and Inventory's `GetByVariationAndWarehouseAsync` pre-check in `OnVariantCreatedEvent`/`Handler`); use `CorrelationId` for cross-service tracing; enqueue the integration event in the same handler and the same `SaveChangesAsync` call as the aggregate mutation it describes.

**DON'T** — call `IEventPublisher.PublishAsync` directly from feature code (bypasses the Outbox's atomicity guarantee) — always go through `IOutboxStore.EnqueueAsync`; put business logic in a Kafka consumer; use an Internal event as a substitute for a real integration event when another *service* actually needs to know; call `IIntegrationEventConsumer` implementations directly (they're invoked only by `IntegrationEventConsumerRegistry`); expect a "domain event" hop to exist — it doesn't.

## Implementation status

**✅ Completed and in production use across every service:**
- Outbox pattern — integration events are enqueued transactionally and relayed to Kafka via `OutboxRelayHostedService`
- Inbox pattern — per-consumer dedup, retry with exponential backoff, and dead-lettering via `IInboxStore`/`InboxRetryHostedService` (see [reference/inbox-outbox-runtime.md](inbox-outbox-runtime.md))
- Aggregate-graph audit events — a parallel, automatic integration-event path for `IAuditable` aggregates, publishing exactly one `AuditIntegrationEvent` per changed Aggregate Root via `AuditInterceptor`, independent of any handler-authored event (see [reference/audit-trail.md](audit-trail.md))

**Known gaps (accepted, not yet addressed):**
- No distributed tracing wired to `CorrelationId` yet
- Outbox relay and Inbox retry are per-service (run in every service's AppDomain); no central relay architecture
- Management APIs to inspect/requeue DeadLetter Inbox rows do not exist yet — a DeadLetter row just stops retrying
