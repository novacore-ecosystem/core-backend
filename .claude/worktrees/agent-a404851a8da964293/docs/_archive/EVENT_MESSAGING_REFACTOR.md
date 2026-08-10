# Event Messaging Refactor: DomainEventPublisher + IIntegrationEventConsumer + KafkaFlow

## Problems Found

1. **Domain events were raised but never published.** `AggregateRoot.RaiseDomainEvent()` added events to an in-memory list, but no `UnitOfWork` implementation ever read that list or called `MediatR.Publish()`. Infrastructure event handlers (e.g. `UserCreatedDomainEventHandler`) existed but were dead code.
2. **No inbound message consumer pattern.** Kafka publishing worked, but there was no defined way for a service to consume another service's integration events — `KafkaEventSubscriber` used reflection over `AppDomain.CurrentDomain.GetAssemblies()` to auto-discover `IIntegrationEventHandler<T>` implementations, and nothing actually started it.
3. **Integration events published with incomplete data.** `UserCreatedDomainEventHandler` built `UserCreatedIntegrationEvent` from the domain event alone, which didn't carry email/name — those fields were published as empty strings.
4. **Raw `Confluent.Kafka` wrapper duplicated what KafkaFlow provides** (worker pools, typed handlers, middleware pipeline) with more custom code to maintain.

## What Changed

- Added `DomainEventPublisher` (`BuildingBlock.Infrastructure`) and wired it into `UnitOfWork.SaveChangesAsync` for both User and Auth services — domain events are now actually published after a successful commit.
- Fixed `UserCreatedDomainEventHandler` to fetch the persisted entity from the repository instead of relying on the domain event's fields.
- Replaced the reflection-based `KafkaEventSubscriber`/`IMessageConsumer`/`IMessageProducer`/raw `KafkaConsumer`/`KafkaProducer` with:
  - `IIntegrationEventConsumer` — explicit, DI-registered consumer interface declaring `Topics` + `HandleAsync`
  - `IntegrationEventConsumerRegistry` — filters registered consumers by topic and dispatches (previously the registry, when it existed transiently, broadcast every message to every consumer regardless of topic — fixed as part of this change)
  - KafkaFlow for the actual broker plumbing (producer/consumer worker pools, middleware pipeline)
  - `IntegrationEventDispatchHandler` — the one KafkaFlow-aware class, bridging `IMessageHandler<byte[]>` to the broker-agnostic registry
  - `KafkaFlowEventPublisher` — replaces the old `KafkaEventPublisher`, same JSON body + header format
  - `KafkaFlowBusHostedService` — starts/stops `IKafkaBus` with the app lifetime
- Collapsed four overlapping DI entry points (`AddKafkaMessageQueue`, `AddKafkaEvents`, `AddEventDispatcher`, `AddIntegrationEventConsumers`) into one: `AddKafkaMessaging(configuration, serviceName)`.
- Fixed a latent bug in Auth's DI: it called `AddEventDispatcher()` with no `IEventPublisher` ever registered (would throw if resolved). Now unified onto the same `AddKafkaMessaging` path as User.

## Why This Ordering Constraint Exists

`AddKafkaMessaging` needs each consumer's declared `Topics` *before* calling KafkaFlow's `AddKafka(...)`, because KafkaFlow's topic subscriptions are configured statically at registration time, not discovered at runtime. To get those topics, `AddKafkaMessaging` builds a temporary `ServiceProvider` from the partially-built `IServiceCollection` and resolves the registered `IIntegrationEventConsumer` instances. This only works if everything those consumers depend on (typically `IMediator`) is already registered — which is why `Program.cs` for both `User.API` and `Auth.API` now calls `AddApplication()` before `AddInfrastructure()`.

## Known Gap Not Yet Fixed

`IIntegrationEventHandler<T>` (`BuildingBlock.Messaging/Abstractions/IIntegrationEventHandler.cs`) is now an orphaned interface — zero implementers, zero dispatchers, since the mechanism that used to route to it (`KafkaEventSubscriber`) was removed. Auth's `UserAccountDeletionIntegrationEvent` resilience leg (queue consumer re-publishing `OnUserDeletionEvent`) that some older docs described was built against this interface and was likely already non-functional before this refactor — it has no `IIntegrationEventConsumer` implementation today. See [EVENT_ARCHITECTURE.md](../architecture/EVENT_ARCHITECTURE.md) for the current pattern.

See also: [EVENT_ARCHITECTURE.md](../architecture/EVENT_ARCHITECTURE.md) for the resulting architecture.
