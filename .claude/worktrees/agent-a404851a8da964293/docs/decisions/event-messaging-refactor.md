# ADR: Event Messaging Refactor (DomainEventPublisher + IIntegrationEventConsumer + KafkaFlow)

**Scope:** why the event/messaging system looks the way it does today. See [reference/events.md](../reference/events.md) for the resulting architecture — this doc is the "why," not the "how."

> **Superseded, 2026-07-17:** the `DomainEventPublisher`/`IDomainEventHandler<T>`/`AggregateRoot.RaiseDomainEvent()` tier this ADR introduces below no longer exists in the codebase — `AggregateRoot<TId>` is now a plain marker class. It was later abandoned in favor of every service enqueueing integration events directly from the command handler via `IOutboxStore.EnqueueAsync` (the Outbox pattern this ADR's "known gap" section correctly anticipated but hadn't built yet), and the "Application event" tier below was renamed to "Internal event" (`IInternalEvent`/`IInternalEventHandler<T>`/`IInternalEventDispatcher`). This ADR is kept as-is below for historical record of the reasoning at the time; it is **not** current guidance — see [reference/events.md](../reference/events.md) for what's actually true today.

## Problems found

1. **Domain events were raised but never published.** `AggregateRoot.RaiseDomainEvent()` added events to an in-memory list, but no `UnitOfWork` implementation ever read that list or called `MediatR.Publish()`. Infrastructure event handlers (e.g. `UserCreatedDomainEventHandler`) existed but were dead code.
2. **No inbound message consumer pattern.** Kafka publishing worked, but there was no defined way for a service to consume another service's integration events — the previous `KafkaEventSubscriber` used reflection over `AppDomain.CurrentDomain.GetAssemblies()` to auto-discover `IIntegrationEventHandler<T>` implementations, and nothing actually started it.
3. **Integration events published with incomplete data.** `UserCreatedDomainEventHandler` built `UserCreatedIntegrationEvent` from the domain event alone, which didn't carry email/name — those fields were published as empty strings.
4. **Raw `Confluent.Kafka` wrapper duplicated what KafkaFlow provides** (worker pools, typed handlers, middleware pipeline) with more custom code to maintain.

## What changed

- Added `DomainEventPublisher` (`BuildingBlock.Infrastructure`) and wired it into `UnitOfWork.SaveChangesAsync` for both User and Auth — domain events are now actually published after a successful commit.
- Fixed `UserCreatedDomainEventHandler` to fetch the persisted entity from the repository instead of relying on the domain event's own fields.
- Replaced the reflection-based `KafkaEventSubscriber`/`IMessageConsumer`/`IMessageProducer`/raw `KafkaConsumer`/`KafkaProducer` with: `IIntegrationEventConsumer` (explicit, DI-registered, declares `Topics`), `IntegrationEventConsumerRegistry` (topic-filtered dispatch — the previous transient registry broadcast every message to every consumer regardless of topic, fixed here), KafkaFlow for broker plumbing, `IntegrationEventDispatchHandler` (bridges KafkaFlow to the registry), `KafkaFlowEventPublisher`, `KafkaFlowBusHostedService`.
- Collapsed four overlapping DI entry points (`AddKafkaMessageQueue`, `AddKafkaEvents`, `AddEventDispatcher`, `AddIntegrationEventConsumers`) into one: `AddKafkaMessaging(configuration, serviceName)`.
- Fixed a latent bug in Auth's DI: it called `AddEventDispatcher()` with no `IEventPublisher` ever registered (would throw if resolved). Unified onto the same `AddKafkaMessaging` path as User.

## Why the registration-order constraint exists

`AddKafkaMessaging` needs each consumer's declared `Topics` *before* calling KafkaFlow's `AddKafka(...)`, because KafkaFlow's topic subscriptions are configured statically at registration time, not discovered at runtime. To get those topics, `AddKafkaMessaging` builds a temporary `ServiceProvider` from the partially-built `IServiceCollection` and resolves the registered `IIntegrationEventConsumer` instances — this only works if everything those consumers depend on (typically `IMediator`/`ISender`) is already registered, which is why `Program.cs` for both services calls `AddApplication()` before `AddInfrastructure()`. See [02-architecture-rules.md](../02-architecture-rules.md#composition-root-convention-per-service).

## Known gap, not yet fixed

`IIntegrationEventHandler<T>` (`BuildingBlock.Messaging/Abstractions/IIntegrationEventHandler.cs`) is an orphaned interface — zero implementers, zero dispatchers, since the mechanism that used to route to it was removed in this refactor. If you're tempted to implement it expecting it to be wired up automatically, it is not — use `IIntegrationEventConsumer` instead (see [workflows/add-integration-event.md](../workflows/add-integration-event.md)).
