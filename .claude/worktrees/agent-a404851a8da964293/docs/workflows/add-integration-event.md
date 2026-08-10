# Workflow: Add Integration Event

**Read first:** [reference/events.md](../reference/events.md), [reference/inbox-outbox-runtime.md](../reference/inbox-outbox-runtime.md), [06-implementation-templates.md](../06-implementation-templates.md#integration-event-publish-side) (both publish and consume templates).

**Before you start:** confirm this genuinely needs to cross a service boundary. If the reaction happens within the same service, use an Internal event instead ([reference/events.md](../reference/events.md#the-two-tiers)) — integration events are for "another service needs to know," not general decoupling within one service.

## Publish side

1. Define the DTO in `BuildingBlock.Contract/Events/{Name}IntegrationEvent.cs` implementing `IIntegrationEvent` — plain class, no MediatR coupling, self-stamps `CorrelationId`/`PublishedAt` in its constructor.
2. From the command handler that made the change (the same handler, not a separate event handler), construct the DTO and call `await outboxStore.EnqueueAsync(new {Name}IntegrationEvent(...), ct)`, then `await unitOfWork.SaveChangesAsync(ct)` — the OutboxMessage row commits in the same transaction as the aggregate change. See `CreateProductHandler`/`UpdateProductHandler`/`DeleteProductHandler` (`Product.Application/Features/Products/Commands/`) for the reference shape.
3. Topic name is derived automatically as `"{serviceName}.{eventType}"` lowercased (`serviceName` comes from the `AddKafkaMessaging(configuration, serviceName)` call in that service's `DependencyInjection.cs`) — you don't configure it separately.
4. **Do not call `IEventPublisher.PublishAsync` directly.** That's a lower-level primitive the Outbox relay itself is built on; calling it from feature code bypasses the Outbox's atomicity guarantee (the publish would happen even if the surrounding transaction later rolled back). Always go through `IOutboxStore.EnqueueAsync`.
5. A background service (`OutboxRelayHostedService`) polls the Outbox table and actually publishes to Kafka, with retry/backoff — you don't drive this yourself. Full mechanics: [reference/inbox-outbox-runtime.md](../reference/inbox-outbox-runtime.md).

## Consume side

1. Implement `IIntegrationEventConsumer` in `{Service}.Infrastructure/Messaging/Consumers/{Name}Consumer.cs` — declare `Topics`, deserialize the message, and **dispatch to a command via `ISender`**. No business logic in the consumer itself — it's an adapter, same as every existing consumer.
2. Register it: `services.AddScoped<IIntegrationEventConsumer, {Name}Consumer>()` in `{Service}.Infrastructure/DependencyInjection.cs`. **Order matters** — this registration must happen before `AddKafkaMessaging(...)` is called, because `AddKafkaMessaging` eagerly builds a temporary service provider to discover all registered consumers' `Topics` before configuring the KafkaFlow consumer pipeline.
3. **Inbox dedup is automatic** — you don't write any dedup code yourself. Every consumer dispatched through `IntegrationEventConsumerRegistry` gets `(messageId, consumerName)`-keyed dedup, retry with exponential backoff, and dead-lettering after `MaxRetryCount` failures, for free — see [reference/inbox-outbox-runtime.md](../reference/inbox-outbox-runtime.md#inbox-lifecycle-retry--transaction-safety). Let exceptions propagate out of `HandleAsync` (don't swallow them) so the Inbox retry mechanism can see the failure.
4. Even with Inbox dedup, make the target command handler idempotent if a duplicate side effect would be wrong (e.g. creating a second row) — see `User.Application`'s `CreateUserCommandHandler` check-then-create-then-recheck pattern, or Inventory's `GetByVariationAndWarehouseAsync` pre-check in `OnVariantCreatedEvent`/`Handler`. Inbox dedup protects against *redelivery of the same message*; it doesn't protect against two *different* messages producing the same logical effect.

## Checklist

- [ ] DTO lives in `BuildingBlock.Contract`, not a per-service project
- [ ] Enqueued via `IOutboxStore.EnqueueAsync` in the same handler/transaction as the aggregate change — never `IEventPublisher.PublishAsync` directly
- [ ] Consumer registered before `AddKafkaMessaging(...)` in the same DI method chain
- [ ] Consumer contains zero business logic — only deserialize + dispatch
- [ ] Consumer lets exceptions propagate (no swallowed catch) so Inbox retry/dead-letter tracking works
- [ ] Target handler is idempotent beyond Inbox dedup if a duplicate logical effect (not just redelivery) is plausible
- [ ] Confirmed this isn't better served by an Internal event (same-service) instead
