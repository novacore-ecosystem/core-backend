# Reference: Audit Trail (Aggregate-Graph Audit Tracking)

**Scope:** the generic, provider-agnostic audit pipeline - `IAuditable`, `[AuditIgnore]`, the hierarchy registration API, the `AuditGraphBuilder` algorithm, and the EF implementation (`AuditInterceptor`) that turns tracked entity changes into one `AuditIntegrationEvent` per changed Aggregate Root. Not to be confused with the [Audit Service](../services/audit-service.md), which is the (sole) consumer of this event - this doc is about the *producer* side that lives in every EF-backed service.

## Why per-Aggregate-Root, not per-entity

An earlier version of this pipeline published one `AuditIntegrationEvent` per changed `EntityEntry` - an Order with two changed OrderItems produced three events. That's wrong: an Order and its OrderItems aren't independent facts, they're one business change to one aggregate. It also meant a *type* of audit event per business concept, which doesn't scale and can't be queried/consumed generically.

The pipeline now publishes **exactly one event per changed Aggregate Root instance per `SaveChanges` call**, carrying the full tree of everything that changed underneath it. Two Orders and an unrelated User changing in the same `SaveChanges` produce exactly three events - never fewer (roots are never merged), never more (an unchanged aggregate produces nothing, and a root's descendants never get their own separate event).

## Two opt-in gates: `IAuditable` and hierarchy registration

An entity is tracked only if **both** are true:

1. It implements `BuildingBlock.Domain.Abstractions.IAuditable` (a member-less marker - a cheap `is` check, tried first).
2. It's registered via `ConfigureAuditHierarchy` (a dictionary lookup).

Both are required by design - a type can never end up half-configured (marked but not placed in the graph, or placed without ever declaring intent). `IAuditable` also documents intent directly on the entity class, independent of wherever hierarchy registration happens to live.

```csharp
public sealed class Order : AggregateRoot<Guid>, IAuditable
public sealed class OrderItem : BaseEntity<Guid>, IAuditable
```

## `[AuditIgnore]` - per-property exclusion

`BuildingBlock.Domain.Attributes.AuditIgnoreAttribute` excludes one property from the change snapshot. `BaseEntity`/`BaseEntity<T>.UpdatedAt` carries it at the base-class level, so every entity gets that exclusion for free. `AuditInterceptor` resolves ignored properties via reflection once per CLR type (cached in a static `ConcurrentDictionary<Type, HashSet<string>>`).

## Hierarchy registration - `ConfigureAuditHierarchy`

Lives in `BuildingBlock.Persistence` (`Audit/` folder) - strongly-typed, expression-based, compile-time-safe, and never guesses a relationship from reflection, EF navigations, or naming conventions. Developers configure only the **direct** parent; the framework resolves the full ancestor chain to the root automatically, both at startup (validation) and at graph-build time (per-instance path resolution).

```csharp
services.ConfigureAuditHierarchy(builder =>
{
    builder.Entity<Order>().IsRoot(x => x.Id);
    builder.Entity<OrderItem>().BelongsTo<Order>(x => x.OrderId);
});
```

- `IsRoot(idSelector)` declares an Aggregate Root.
- `BelongsTo<TParent>(parentIdSelector)` declares a direct child, identified by the FK selector back to its parent.
- An entity's **own** identity is a provider concern, not a hierarchy concern: the EF provider reads it via EF's own primary-key metadata (`IEntityType.FindPrimaryKey()`), never reflection-guessing - robust even for composite or renamed keys. `IsRoot`'s selector exists for API symmetry and for future non-EF providers that don't have EF's metadata to fall back on.
- Built once into an `IAuditHierarchyRegistry` singleton (`AuditHierarchyRegistry`) - metadata is never rebuilt per `SaveChanges`. The registry **fails fast at startup** (`InvalidOperationException`) if a `BelongsTo<TParent>` chain doesn't resolve to a registered root, or if it cycles - a misconfiguration is caught immediately, not guessed at or silently ignored deep inside a request.
- Registered today, per service (`*.Persistence/DependencyInjection.cs`'s `AddAuditHierarchy()`):
  - **Order**: `Order` is root, `OrderItem` belongs to it (`OrderProductCatalog` is a separate local read-model, not part of this aggregate, so it's excluded).
  - **User**: `UserProfile` is root, with no children - a single-node aggregate is still a valid graph, just one with no children.
  - **Auth**: `Account` and `Role` are each their own root - independent aggregates, related many-to-many via `AccountRole`, which isn't itself audited (a join row has no single owning parent this model can express).
  - **Product**: `Product` and `ProductCategory` are each their own root - `Product.ProductCategoryId` is a reference, not ownership, so they're independent, not parent/child.
  - **Inventory**: `Inventory` and `Warehouse` are each their own root (`Warehouse` is a physical location, not owned by `Inventory`); `InventoryTransaction` belongs to `Inventory` via `InventoryId` - a stock mutation's audit graph is attached to the `Inventory` record it happened against. Inventory previously had no Outbox table at all (it only consumed events) - one was added (`Migrations/*_AddOutboxSupport.cs`) specifically so this pipeline has somewhere to enqueue its events; see [services/inventory-service.md](../services/inventory-service.md).

## The Audit Graph

`BuildingBlock.Contract.Events.Audit.AuditNode` is a real recursive tree - `NodeId`, `ParentNodeId`, `Depth`, `EntityType`, `EntityId`, `Action`, `Changes`, `Children` (a true hierarchy, not a flat list). A node with an empty `Changes` collection is a **structural pass-through**: it didn't change itself, but sits on the path between the root and a descendant that did (e.g. an untouched `OrderItem` carrying a changed `Discount`) - this is what lets a consumer walk from any changed descendant all the way back to its root without ever losing the hierarchy in between.

## `SaveChanges` flow

```
Collect ChangeTracker entries (IAuditable + registered only - filtered before any property work)
  -> Extract AuditTrackedEntity per entry (own id via EF PK metadata, parent id via the
     registered FK accessor, Changes via [AuditIgnore]-filtered property comparison)
  -> AuditGraphBuilder.Build(entities, registry)   [provider-agnostic, BuildingBlock.Persistence]
       - resolves each changed entity's ancestor path by walking ParentType/ParentEntityId
         through whatever else is tracked in the same DbContext instance
       - merges all paths sharing a root into one tree, one root group = one AuditGraphResult
       - a root group with zero real changes never happens by construction (it only exists
         because a changed leaf produced it)
  -> One AuditIntegrationEvent per AuditGraphResult, serialized exactly once
  -> One OutboxMessage per event, added directly onto the same DbContext (inside
     SavingChangesAsync, before SaveChanges runs) - commits in the exact same transaction as
     the business change, the same transactional-outbox guarantee every other integration
     event relies on (see [inbox-outbox-runtime.md](inbox-outbox-runtime.md))
```

`AuditInterceptor` (`BuildingBlock.Persistence.Ef/Interceptors/`) is the only EF-aware piece; everything above the "Extract AuditTrackedEntity" step is pure, provider-agnostic logic in `BuildingBlock.Persistence` (`AuditGraphBuilder`) reusable by a future Dapper/Mongo provider without change.

## A structural constraint worth knowing: full-aggregate tracking

Ancestor-path resolution only ever looks at what's *already tracked* in the current `DbContext` instance. If only a leaf entity (e.g. a `Discount`) is loaded and mutated, without its parent `OrderItem`/`Order` also being tracked, the framework has no way to know the true root - it falls back, best-effort, to treating that leaf as its own root rather than guessing. This isn't a bug: it falls naturally out of "never guess a relationship that isn't explicitly resolvable," and it holds automatically whenever a repository loads a complete aggregate before mutating it (the normal DDD/EF pattern, and the only way `Order.Cancel()` etc. work correctly today).

## Traversal example

```
Order                  <- Root, Depth 0
├── OrderItem A         <- Depth 1
│     ├── Discount       <- Depth 2
│     └── Tax            <- Depth 2
└── OrderItem B         <- Depth 1
```

Down: `Root.Children[0].Children[...]`. Up: any node's `ParentNodeId` points at its direct parent's `NodeId`, all the way to the root (`ParentNodeId == null`). See `AuditInterceptorTests.SaveChanges_PreservesFullAncestorPath_WhenOnlyADeepDescendantChanges` (`tests/BuildingBlock.Persistence.Ef.Tests/`) for a working demonstration of both directions, and `AuditGraphBuilderTests.Build_GroupsByAggregateRoot_NeverMergingDifferentRoots` for the exactly-one-event-per-root guarantee against a mixed multi-root batch.

## Extension points: `AuditMetadata`

`BuildingBlock.Contract.Events.Audit.AuditMetadata` is optional, additive context attached to every graph - `Actor`, `Service`, `ClientIp`, `UserAgent`, `BusinessAction`, `Reason`, `RequestPath`, `TraceId`. A new field is always a new nullable property here, never a breaking change to `AuditIntegrationEvent`.

Populated by `BuildingBlock.Application.Abstractions.Services.IAuditMetadataProvider.Capture()` - deliberately **not** coupled to EF. `BuildingBlock.Persistence.Ef.AddPersistenceDbContext` registers a default no-op `NullAuditMetadataProvider` so DI always resolves even for services (background workers) with no request context. `BuildingBlock.Infrastructure.Audit.HttpAuditMetadataProvider` is the HTTP-aware implementation (`ICurrentUserService` + `IHttpContextAccessor` + `Activity.Current`), registered per service via `services.AddHttpAuditMetadataProvider("Order")` from that service's own Infrastructure DI composition. Both the default and the real registration use plain `Add`/`TryAdd` so registration order relative to `AddPersistenceDbContext` never matters (Microsoft.DI resolves the last plain-`Add` registration, and `TryAdd` never overrides an existing one either way).

## The single event, and the events that stayed

`AuditIntegrationEvent` (`BuildingBlock.Contract.Events.Audit`) is the **only** audit event type in the project - `RootEntityType`, `RootEntityId`, `Root` (the graph), `Metadata`. `OrderCreatedIntegrationEvent`/`OrderCancelledIntegrationEvent` were removed entirely: they existed solely to feed Audit Service's old per-event consumers and had no other business subscriber (confirmed - `OrderCreatedAuditConsumer`/`OrderCancelledAuditConsumer` were their only consumers). `ProductCreatedIntegrationEvent`, `UserDeletionIntegrationEvent`, and `UserProfileCreatedIntegrationEvent` remain, unchanged - they have real business consumers (Inventory, Order, User, Auth) independent of auditing, so removing them was never in scope; Audit Service simply stopped consuming them for audit purposes, since that's now `AuditIntegrationEvent`'s job exclusively.

## MongoDB persistence strategy (Audit Service)

See [services/audit-service.md](../services/audit-service.md) for the full picture. In short: `AuditLogEntry` persists the graph as a real nested document (`AuditTrailNode`, Mongo's own mirror of `AuditNode` - kept as a separate type so `Audit.Domain` stays free of a `BuildingBlock.Contract` dependency, same reasoning as every other entity in that project), with `RootEntityType`/`RootEntityId`/`Service`/`Timestamp` kept as flat, top-level, indexed fields (see `scripts/mongodb/init-mongo.js`) so a query never has to descend into the nested document just to filter by aggregate identity or originating service. No secondary flat-index collection was added - the existing top-level fields already cover the query shapes this service needs (`ListAuditLogsQuery`'s service/date-range filter, `GetAuditLogQuery`'s by-id lookup); a per-node index collection would only be worth adding if a future query needed to search *inside* arbitrary graphs, which nothing does today.
