# Event Architecture

NovaCore uses a three-tier event architecture to decouple infrastructure concerns from business logic and enable async communication between services, plus an explicit consumer pattern for inbound Kafka messages.

## Event Type Hierarchy

```
┌─────────────────────────────────────────────────────────────────┐
│                    DOMAIN EVENT (IDomainEvent)                  │
│  • Where:   Domain layer, raised by aggregates                  │
│  • Scope:   Within a single service                             │
│  • Timing:  Collected during aggregate mutation, published      │
│             by UnitOfWork after a successful SaveChanges        │
│  • Handler: INotificationHandler<T> in Infrastructure           │
│  Example: UserCreatedDomainEvent                                │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│              APPLICATION EVENT (IApplicationEvent)              │
│  • Where:   Application layer                                   │
│  • Scope:   Within a single service                             │
│  • Timing:  Synchronous via MediatR                             │
│  • Use:     Orchestrate business logic in the Application layer │
│  • Handler: IApplicationEventHandler<T>                         │
│  Example: UserEmailVerifiedApplicationEvent                     │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│          INTEGRATION EVENT (IIntegrationEvent/Kafka)            │
│  • Where:   Infrastructure layer, published to Kafka            │
│  • Scope:   Across services                                     │
│  • Timing:  Asynchronous                                        │
│  • Use:     Cross-service communication                         │
│  • Handler: IIntegrationEventConsumer in the receiving service  │
│  Example: UserCreatedIntegrationEvent                           │
└─────────────────────────────────────────────────────────────────┘
```

| Event Type | When | Why |
|---|---|---|
| **Domain Event** | Aggregate state changes | Captures domain invariants, keeps the domain layer pure (no MediatR/Kafka knowledge required at the interface level) |
| **Application Event** | Orchestrate logic synchronously within a service | Moves logic out of Infrastructure, easier to test |
| **Integration Event** | Async communication across services | Decouples services, survives service restarts (Kafka), scales independently |

## Outbound Flow: Domain Event → Kafka

```
1. Aggregate raises event
   user.RaiseDomainEvent(new UserCreatedDomainEvent(...))
   → added to the aggregate's in-memory DomainEvents list, NOT published yet

2. Command handler persists
   await unitOfWork.SaveChangesAsync(ct)

3. Inside UnitOfWork.SaveChangesAsync (Persistence layer)
   a. dbContext.SaveChangesAsync() — data committed
   b. Collect DomainEvents from all tracked AggregateRoot entities
   c. Clear each aggregate's DomainEvents (before publishing, to avoid re-publish on retry)
   d. DomainEventPublisher.PublishAsync(events) → MediatR.Publish() per event

4. Infrastructure handler (INotificationHandler<TDomainEvent>) reacts
   - Fetches whatever additional data it needs (the domain event carries only
     what the aggregate had; don't assume it has every field an integration
     event needs — fetch from the repository if not)
   - Builds the IntegrationEvent and calls IEventDispatcher.PublishAsync(...)

5. IEventDispatcher → IEventPublisher → KafkaFlowEventPublisher
   → JSON-serializes the event, publishes to Kafka topic {service}.{eventType-lowercase}
```

Concrete example (`User.Infrastructure/Events/Handlers/UserCreatedDomainEventHandler.cs`):

```csharp
public sealed class UserCreatedDomainEventHandler(
    IRepository<UserProfile> userRepository,
    IEventDispatcher integrationEventDispatcher,
    ILogger<UserCreatedDomainEventHandler> logger)
    : INotificationHandler<UserCreatedDomainEvent>
{
    public async Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(domainEvent.AggregateId, ct);
        if (user is null) return;

        var integrationEvent = new UserCreatedIntegrationEvent(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName,
            domainEvent.AggregateId.ToString());

        await integrationEventDispatcher.PublishAsync(integrationEvent, ct);
    }
}
```

### Wiring (Persistence layer)

`UnitOfWork.SaveChangesAsync` is where domain events actually get published — this is the one part of the pattern that's easy to silently skip and have domain events go nowhere:

```csharp
public sealed class UnitOfWork(
    UserDbContext context,
    DomainEventPublisher domainEventPublisher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await context.SaveChangesAsync(ct);

        var aggregates = context.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        if (domainEvents.Count > 0)
            await domainEventPublisher.PublishAsync(domainEvents, ct);

        return result;
    }
}
```

`DomainEventPublisher` (in `BuildingBlock.Infrastructure`) is the one place that couples domain events to MediatR — the coupling lives in Infrastructure/Persistence, not in the Domain project itself.

## Inbound Flow: Kafka → Application Command

Consumers are explicit adapters — no reflection-based auto-discovery. Each consumer declares which topics it listens to and translates the raw message into an Application Command.

```
Kafka topic message arrives
    ↓
KafkaFlow IntegrationEventDispatchHandler (BuildingBlock.Messaging.Kafka)
    - decodes bytes → JSON string, headers → dictionary
    - looks up context.ConsumerContext.Topic
    ↓
IntegrationEventConsumerRegistry.DispatchAsync(topic, message, headers, ct)
    - filters registered IIntegrationEventConsumer instances to the ones
      whose Topics contain this topic
    - invokes each matching consumer, catching/logging errors per-consumer
      so one bad message doesn't take down the others
    ↓
YourService's IIntegrationEventConsumer.HandleAsync(message, headers, ct)
    - deserializes to the concrete IntegrationEvent type
    - creates an Application Command
    - dispatches via IMediator.Send(command, ct)
    ↓
Application Command Handler
    - orchestrates the use case, persists changes
    - may itself raise new domain events → repeats the outbound flow
```

Example consumer (`User.Infrastructure/Messaging/Consumers/UserAccountDeletionIntegrationEventConsumer.cs`):

```csharp
public sealed class UserAccountDeletionIntegrationEventConsumer(
    IMediator mediator,
    ILogger<UserAccountDeletionIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => ["user-service.useraccountdeletionintegrationevent"];

    public async Task HandleAsync(string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<UserAccountDeletionIntegrationEvent>(message);
        if (integrationEvent is null) return;

        // var command = new DeleteUserAccountCommand(Guid.Parse(integrationEvent.UserId));
        // await mediator.Send(command, ct);
    }
}
```

### Registering a Consumer

```csharp
// {Service}.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDomainEventPublisher();
    services.AddApplicationEventDispatcher();

    // Consumers must be registered BEFORE AddKafkaMessaging - their Topics
    // are discovered eagerly (via a temporary ServiceProvider) to configure
    // the KafkaFlow consumer pipeline at startup.
    services.AddScoped<IIntegrationEventConsumer, UserCreatedIntegrationEventConsumer>();

    services.AddKafkaMessaging(configuration, "user-service");

    return services;
}
```

Because topic discovery happens via a temporary `ServiceProvider` built mid-registration, anything your consumers depend on (e.g. `IMediator`) must already be registered in the collection by the time `AddKafkaMessaging` runs — in practice this means `AddApplication()` (which registers MediatR) must run **before** `AddInfrastructure()` in `Program.cs`.

Topic naming convention: `{source-service}.{event-type-lowercase}`, e.g. `user-service.usercreatedintegrationevent`.

## Layer Responsibilities

| Layer | Responsibility |
|---|---|
| Domain | Raise domain events via `AggregateRoot.RaiseDomainEvent()` |
| Persistence | Publish domain events (`UnitOfWork.SaveChangesAsync` → `DomainEventPublisher`) after a successful commit |
| Infrastructure | React to domain events, translate to integration events, publish to Kafka (`IEventDispatcher`/`IEventPublisher`) |
| Infrastructure | Listen to Kafka via `IIntegrationEventConsumer`, translate inbound messages to Application Commands |
| Application | Orchestrate use cases via Command/Query handlers and `IApplicationEventHandler<T>` |
| Domain | Execute business logic inside aggregates |

## DO's and DON'Ts

**DO**
- Raise domain events from aggregates during business logic
- Let `UnitOfWork.SaveChangesAsync` be the only place that publishes domain events
- Fetch complete data from the repository in the domain event handler rather than assuming the domain event carries everything an integration event needs
- Keep consumers as thin adapters — deserialize, then dispatch a Command
- Make consumers idempotent (messages may be delivered more than once)
- Use correlation IDs for tracing events across services

**DON'T**
- Expect a domain event handler to run without going through `SaveChangesAsync` — nothing calls it directly
- Publish integration events directly from a command handler, bypassing the domain event → infrastructure handler hop, unless there genuinely is no domain event for the change
- Put business logic in a Kafka consumer — translate to a Command and let the Application layer handle it
- Use MediatR domain/application events for cross-service communication (use Kafka integration events instead)
- Call `IIntegrationEventConsumer` implementations directly — they're wired up by `IntegrationEventConsumerRegistry`, not invoked manually

## Known Gaps

- No dead-letter queue: a consumer that keeps throwing just keeps getting retried by Kafka's redelivery, logged each time
- No idempotency check built into the framework — each consumer is responsible for its own (e.g. check-before-create)
- No distributed tracing wired to `CorrelationId` yet
