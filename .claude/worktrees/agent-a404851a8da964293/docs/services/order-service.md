# Order Service

**Scope:** Order-specific facts and its documented divergences from the [User Service](user-service.md) reference implementation. General patterns live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md) — not repeated here.

## Projects

`Order.Domain`, `Order.Application`, `Order.Infrastructure`, `Order.Persistence`, `Order.API` — same 5-layer split as User.

## Entities

- **Order** (`Order.Domain/Entities/Order.cs`) — `CustomerId`, `Status` (`OrderStatus`: Pending/Confirmed/Cancelled/Completed), `TotalAmount` (computed sum over `Items`, not stored), `Items`, `IdempotencyKey` (nullable, client-supplied dedup key scoped per `CustomerId` — unique partial index in `OrderConfig`), `CancellationReason` (nullable, set only when `Status == Cancelled`, e.g. `"OutOfStock"`/`"CancelledByCustomer"`). Factory `Create(id, customerId, items, idempotencyKey)` requires at least one item (throws `BuildingBlock.Domain.Exceptions.EmptyCollectionException`). `Confirm()`/`Cancel(reason)`/`Complete()` are status-guarded and throw `InvalidStatusException` on an invalid transition — the first entity in this codebase to actually exercise these two domain exception types (Product/Inventory/User never trigger them). Implements `IAuditable` and is registered as the Aggregate Root in `Order.Persistence/DependencyInjection.cs`'s `ConfigureAuditHierarchy` call — see [reference/audit-trail.md](../reference/audit-trail.md). Also mapped with a Postgres `xmin` concurrency token (see "Concurrency" below).
- **OrderItem** (`Order.Domain/Entities/OrderItem.cs`) — child entity owned by `Order`: `OrderId`, `ProductId`, `ProductName`/`UnitPrice` (snapshotted at order-creation time, not a live reference to the product catalog), `Quantity`, computed `LineTotal`. This is the first true parent/child aggregate in the codebase (Product/Inventory/User entities are all flat, single-entity aggregates) — see the EF mapping note below. Also `IAuditable`, registered as `BelongsTo<Order>(x => x.OrderId)` in the same audit hierarchy — an Order's audit graph includes its changed items as child nodes, never a separate event per item.
- **OrderProductCatalog** (`Order.Domain/Entities/OrderProductCatalog.cs`) — local read-model **keyed by `VariantId`** (`Id` is the variation id itself, no surrogate key), carrying `ProductId`, `ProductName`, `Sku`, `Price`, `Status` (mirrors Product's `VariantStatus` as of the last synced event; `IsOrderable` is `Status == "Active"`). Re-keyed from `ProductId` down to variation level because Product was redesigned so a single Product can have many priced `Variant`s — a catalog row per product no longer has a single meaningful price. Kept in sync by five Product-originated integration event consumers (see "Messaging" below). Exists so `CreateOrderHandler` can validate/price/enable-check requested variations without a synchronous call to Product Service.
- **OrderPayment** (`Order.Domain/Entities/Orders/OrderPayment.cs`) — 1:1 with `Order` (shared PK, `OrderId`), a **lightweight reference + snapshot only** — `PaymentId` (nullable, Payment Service's own `Payment.Id`), `PaymentStatus` (Pending/Paid/Failed/Refunded/PartiallyRefunded — a local snapshot enum, not Payment Service's own `PaymentStatus`), `PaidAmount`, `CurrencyCode`, `PaidAt`. Order never knows a payment gateway, payment method, payment account, redirect URL, webhook, or gateway response — those are exclusively Payment Service's concern (`src/Services/Payment`). Updated wholesale via `internal RecordPayment(...)`, mirroring `OrderShipping.UpdateSnapshot` — not yet called from anywhere (no consumer wired to a Payment Service integration event yet). See [payment-service.md](payment-service.md) and [reference/payment-ownership-boundaries.md](../reference/payment-ownership-boundaries.md) for the full responsibility split.

## Ports & routing

Internal `8080` (REST) only. Gateway path prefix `/api/order/` (`RequireAuth: true`, already configured in the Gateway's `appsettings.json` ahead of this service's implementation).

## Routes (Carter endpoints, `Order.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/orders` | `CreateOrder.cs` | Create an order from a list of `(ProductId, Quantity)` items plus an optional `IdempotencyKey` — validates against the local catalog and a read-only Inventory stock check, persists a `Pending` order, returns **202 Accepted** immediately (inventory deduction/confirmation continue asynchronously via the CreateOrder saga — see below). (RequireAuthenticated) |
| GET | `/orders/{orderId}` | `GetOrder.cs` | Fetch an order with its items, status, and `CancellationReason` by id (RequireAuthenticated) |
| POST | `/orders/{orderId}/cancel` | `CancelOrder.cs` | Cancel a Pending or Confirmed order, with a reason (defaults to `"CancelledByCustomer"`) (RequireAuthenticated) |
| POST | `/orders/{orderId}/complete` | `CompleteOrder.cs` | Mark a Confirmed order Completed — admin/fulfillment action, no automated saga step reaches this status (RequireAdmin) |

## gRPC client: Order → Inventory

Order now has a gRPC **client** (still no gRPC server/listener of its own — the divergence from User noted below is unchanged on the server side). `Order.Infrastructure/GrpcClients/InventoryClientService.cs` implements `IInventoryClientService` (`Order.Application/Abstractions/Services/`) over `InventoryGrpcService`:

- `GetAvailableStockAsync` — wraps the existing `GetProductStock` RPC, called synchronously from `CreateOrderHandler`'s Phase 3 stock pre-check (read-only, never reserves).
- `DeductStockAsync`/`RestockAsync` — wrap the new `DeductStock`/`RestockStock` RPCs (see [Inventory Service](inventory-service.md#grpc-inventorygrpcservice)), called from the CreateOrder saga's `DeductInventoryStep`.

Full flow, event contracts, retry/idempotency/compensation: [reference/create-order-saga.md](../reference/create-order-saga.md).

## Documented divergence from User: no gRPC server

Same as [Product Service](product-service.md#documented-divergence-from-user-no-grpc-surface) — no gRPC listener or `GrpcServices/` folder; Order only calls *out* via gRPC (see above), nothing calls *into* Order via gRPC.

## Messaging: Order consumes five Product-originated events, publishes three of its own, and implicitly publishes via the audit pipeline

`Order.Infrastructure/Messaging/Consumers/` — Order's Inbox spans five Product integration events, each deserialized and dispatched as an internal event, no business logic in the consumer itself:

| Consumer | Internal event | Effect on `OrderProductCatalog` |
|---|---|---|
| `VariantCreatedIntegrationEventConsumer` | `OnVariantCreatedEvent` | Upserts the catalog row keyed by `VariantId` (create, or update Sku/Price/Status/ProductName if a redelivery races an update) |
| `VariantUpdatedIntegrationEventConsumer` | `OnVariantUpdatedEvent` | Updates Sku/Price/Status on the existing row (logs a warning and skips if the row is somehow missing) |
| `VariantDeletedIntegrationEventConsumer` | `OnVariantDeletedEvent` | Deletes the catalog row for that variation |
| `ProductUpdatedIntegrationEventConsumer` | `OnProductUpdatedEvent` | Refreshes `ProductName` on **every** catalog row for that `ProductId` (a Product's Name is shared across all its variations) |
| `ProductDeletedIntegrationEventConsumer` | `OnProductDeletedEvent` | Deletes every catalog row for that `ProductId` in one pass |

`ProductCreatedIntegrationEvent` (bare product creation, before any variation exists) has no consumer on the Order side — there's nothing useful to build a priced catalog row from until the first `VariantCreatedIntegrationEvent` arrives, which happens in the same Product-side transaction anyway. `VariantCreatedIntegrationEvent`/`VariantUpdatedIntegrationEvent` gained a `Status` field (Product's `VariantStatus`, e.g. `"Active"`/`"Inactive"`/`"Discontinued"`) specifically so Order's catalog can reject ordering a disabled variation — see `OrderProductCatalog.IsOrderable` above.

- **Publishes again, for the CreateOrder saga — not the old audit-era events.** `CreateOrderHandler`/`CancelOrderHandler`/`CompleteOrderHandler` (and the saga's `ConfirmOrderStep`/failure path) construct and enqueue `OrderCreatedIntegrationEvent`/`OrderConfirmedIntegrationEvent`/`OrderCancelledIntegrationEvent`/`OrderCompletedIntegrationEvent` via `IOutboxStore`, in the same `SaveChangesAsync` as the aggregate change. This is a distinct mechanism from the aggregate-graph audit pipeline (`AuditInterceptor` still separately produces `AuditIntegrationEvent` for every `IAuditable` change, same as before) — these events exist purely to drive the saga and Notification Service's realtime/persisted reactions. **`OrderCompletedIntegrationEvent` has no consumer yet** — Notification Service doesn't react to it (unlike the other three), since `CompleteOrder` was added without a corresponding realtime/persisted-notification requirement; revisit if Completed-order notifications are ever needed. Full detail: [reference/create-order-saga.md](../reference/create-order-saga.md).
- **`OrderCreatedSagaConsumer`** (`Order.Infrastructure/Messaging/Consumers/`) is Order's sixth consumer — a deliberately thin adapter (only `ISender`+`IAppLogger`) that dispatches `RunCreateOrderSagaCommand`, whose handler (`Order.Application/Features/Orders/Commands/RunCreateOrderSaga/`) actually drives `BuildingBlock.Saga`'s `ISagaOrchestrator`. See [reference/saga.md](../reference/saga.md) and [reference/create-order-saga.md](../reference/create-order-saga.md).

**Known naming mismatch:** `OrderItemCreateModel`/`CreateOrderCommand`'s `ProductId` field is looked up against `OrderProductCatalog` (keyed by `VariantId`) unchanged — `CreateOrderHandler` still compiles and behaves correctly, but the field is semantically a VariantId now, not a ProductId. Renaming `OrderItem`/`Order`'s own fields remains out of scope — flagging as a follow-up rename if Order's own commands are revisited.

Because Order needs both an Outbox and an Inbox, it is the first service (besides Auth/User) to use `BuildingBlock.Infrastructure.BackgroundJobs.Cleanup.CleanupJobsExtensions.AddInboxOutboxCleanupJobs(configuration)` as-is, rather than hand-registering a single cleanup job the way Product (Outbox-only) and Inventory (Inbox-only) do — `Order.Persistence`/`Order.Infrastructure` are modeled directly on `User.Persistence`/`User.Infrastructure`.

## Price/Sku moved off `ProductCreatedIntegrationEvent` onto the variation-level events

Pre-redesign, `ProductCreatedIntegrationEvent` carried a single `Sku`/`Price` (via a `VariantId` field) because Product itself was flat. Now that Price/Sku live on `Variant`, `ProductCreatedIntegrationEvent` is product-level-only (`Code`/`Name`/`Slug`) and `OrderProductCatalog` gets its price from `VariantCreatedIntegrationEvent`/`VariantUpdatedIntegrationEvent` instead (see the table above) — not an additive field on the original event.

## Order-specific building blocks (not present in User)

- **`OrderItem` is mapped as an EF owned collection (`OwnsMany`), not a related entity with its own `DbSet`/repository.** `OrderConfig.Configure` calls `builder.OwnsMany(x => x.Items, ...)`. `Items` is a plain `ICollection<OrderItem> { get; private set; }` auto-property (migrated 2026-07-17 from a backing-field + `IReadOnlyCollection` wrapper to match [conventions/domain-coding-conventions.md#3](../conventions/domain-coding-conventions.md#3-aggregate-collections-are-normal-navigation-properties-not-backing-field-wrappers)), so no explicit `UsePropertyAccessMode(PropertyAccessMode.Field)` is needed — EF invokes the private setter directly. Owned collections are always loaded with their owner — this is why `OrderReadService`/`GetOrderHandler` never call `.Include()` (no Application-layer handler in this codebase does; adding one would require referencing EF Core from the Application project, breaking the layering rule). If `OrderItem` ever needs independent identity/querying, it would need to become a real related entity with its own repository instead.
- **Dual Outbox+Inbox, like User/Auth, unlike Product/Inventory** — `OrderDbContext` implements both `IOutboxDbContext` and `IInboxDbContext`; `Order.Persistence/DependencyInjection.cs`'s `AddOutboxAndInbox()` registers both EF stores plus both Application-level adapters (`Order.Persistence/Outbox/OutboxStore.cs`, `Order.Persistence/Inbox/InboxStore.cs` — copied verbatim from `User.Persistence`'s equivalents, since these adapters only touch the provider-agnostic primitive interfaces).
- **`ISagaStore` backed by Postgres, not the in-memory default.** `Order.Persistence/Saga/EfSagaStore.cs` persists `SagaExecutionRecord` history to a `saga_execution_records` table — registered as a singleton (matching `SagaOrchestrator`'s own lifetime) that opens its own DI scope per call rather than taking `OrderDbContext` as a constructor dependency, since the DbContext itself is Scoped. Purely observability/audit; saga *correctness* (retry-safety, idempotency) never depends on this table — see [reference/create-order-saga.md](../reference/create-order-saga.md).
- **Concurrency tokens on `orders` and (Inventory's) `inventories`** — Postgres's native `xmin` system column, mapped via a shadow property + `IsRowVersion()` (not the Npgsql `UseXminAsConcurrencyToken()` sugar method, unavailable in the referenced provider version). **Migration authors: never let the scaffolded migration `AddColumn`/`DropColumn` an `xmin` column** — Postgres rejects `ALTER TABLE ... ADD COLUMN xmin` (system column name conflict); hand-remove those two lines from any future migration touching an entity with this mapping.
- **No Redis / `ICacheService` usage** — no cache keys reserved for Order yet.
- **No database seeding** — unlike Product's `ProductSeeder` (reference data), Order has nothing to seed; `Order.API/ApplicationPipeline.cs` skips the `SeedDatabase()` step Product/User have.

## Persistence: Read/Write services

Per [conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md) — Order was the last service migrated (Phase 7), both aggregates built to the settled shape from the start. `Order.Application/Abstractions/Persistence/{Orders,OrderProductCatalogs}/` hold the two aggregates' Read/Write service ports; handlers, the Saga step (`ConfirmOrderStep`), and `Order.Infrastructure/Caching/CartService.cs` (a non-MediatR consumer, easy to miss with a `*Handler.cs`-only search) all inject these instead of a repository.

- **`IOrderRepository` is an empty marker** — `OrderRepo` implements the generic `IRepository<OrderEntity>` in full, and `Order`+`OrderItem` (an owned collection, see below) has no bulk-by-foreign-key need beyond that. `IOrderProductCatalogRepository` isn't empty: it keeps `UpdateProductNameByProductIdAsync`/`DeleteByProductIdAsync`, since a Product's name change or deletion fans out to every catalog row for that `ProductId`, not a single tracked entity — the same shape as Inventory's `DeleteByProductIdAsync`/`DeleteByVariationIdAsync`.
- **`IOrderWriteService.CancelAsync`/`CompleteAsync` absorb the status guard their callers used to run separately** (`BadRequestException` if the order isn't in a cancellable/completable status) — the guard now lives inside the `repo.UpdateAsync` mutation lambda, same precedent as Product's `ReorderVariationsAsync`. Both methods are shared verbatim by their obvious caller (`CancelOrderHandler`/`CompleteOrderHandler`) and by `RunCreateOrderSagaHandler`'s saga-driven cancel path — the guard is a strict match for `Order.Cancel`'s own domain invariant, so reusing it doesn't change that path's behavior.
- **`OrderProductCatalogReadService.GetByVariantionIdsAsync`/`ExistsAsync`** are read-only and live entirely off the repo, querying `OrderDbContext` directly — including the pre-existing `Variantion` typo in the method name, kept as-is (out of scope for this migration, not silently renamed).

## Naming note: the `Order` entity vs. the `Order` root namespace

Same C# namespace-vs-type collision as [Product Service](product-service.md#naming-note-the-product-entity-vs-the-product-root-namespace)/[Inventory Service](inventory-service.md#naming-note-the-inventory-entity-vs-the-inventory-root-namespace). `Order.Application` and `Order.Persistence` both alias it in `GlobalUsings.cs`:

```csharp
global using OrderEntity = Order.Domain.Entities.Order;
```

Use `OrderEntity` (not bare `Order`) wherever the entity type itself is referenced outside `Order.Domain`. `OrderItem` and `OrderProductCatalog` have no such collision and are used by their plain names.

## Known issues

- See the `ProductId`/`VariantId` naming mismatch noted under "Messaging" above.
- An order that stays `Pending` because the CreateOrder saga exhausted Inbox retries (dead-lettered) has no automatic resolution or admin requeue UI yet — see [reference/create-order-saga.md](../reference/create-order-saga.md#failure-scenarios).
