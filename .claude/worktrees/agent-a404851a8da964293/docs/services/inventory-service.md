# Inventory Service

**Scope:** Inventory-specific facts and its documented divergences from the [User Service](user-service.md) reference implementation. General patterns live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md) — not repeated here.

## Projects

`Inventory.Domain`, `Inventory.Application`, `Inventory.Infrastructure`, `Inventory.Persistence`, `Inventory.API` — same 5-layer split as User.

## Entities

- **Warehouse** (`Inventory.Domain/Entities/Warehouse.cs`) — `Code` (unique), `Name`, `Type`, `Address`, `Status`, `Zones` (collection). Every warehouse is created with a default storage zone (`DEFAULT` / "Default Storage Zone") automatically; a warehouse without a zone is an invalid state and cannot be created. The platform default warehouse is seeded as `PLATFORM` / "Platform Store Warehouse" with type `Virtual`, representing the logical inventory location for products managed without physical warehouse requirements (consignments, marketplace products, etc.). Physical warehouses can be created via the API and will similarly receive an automatic default zone; future warehouse features (receiving, transfers, picking, put-away) build upon this guaranteed zone foundation.
- **Inventory** (`Inventory.Domain/Entities/Inventory.cs`) — **stock-keeping unit is now `(VariantId, WarehouseId)`, not `(ProductId, WarehouseId)`** — a Product can no longer exist without a variation, and a variation is the actual priced/stocked SKU. `ProductId` is still carried on the row (indexed, non-unique) purely so "total stock for a product" can filter without a cross-service join back to Product — it is never a second source of truth for `Quantity`. `Increase`/`Decrease`/`Adjust` unchanged. `IAuditable`, Aggregate Root for `InventoryTransaction`. Mapped with a Postgres `xmin` concurrency token (see "Concurrency" below) — guards `StockIn`/`StockOut`/`Adjust`/`DeductStock`/`RestockStock` all racing the same row.
- **InventoryTransaction** (`Inventory.Domain/Entities/InventoryTransaction.cs`) — gained `VariantId` alongside the existing `ProductId`, otherwise unchanged (append-only movement log, one row per stock mutation in the same transaction as the `Inventory` update).
- **StockDeduction** (`Inventory.Domain/Entities/StockDeduction.cs`) — new. Idempotency ledger for the `DeductStock`/`RestockStock` gRPC pair, `Id` is the caller-supplied `deduction_id` (Order Service's `OrderId`), not a surrogate key. `Status` (`Succeeded`/`Failed`/`Reversed`), `ItemsJson` (snapshot of what was actually deducted, so `RestockStock` can reverse it without the caller resending item details), `FailureCode`, `Reason`. Not `IAuditable` — it's technical plumbing, not a business record. See "gRPC" below and [reference/create-order-saga.md](../reference/create-order-saga.md).

## Ports & routing

Internal `8080` (REST) **and gRPC (`5002` internal, `ASPNETCORE_GRPC_PORT`)** — Inventory now has a dual HTTP/gRPC listener, following User's exact pattern (`Program.cs` `ConfigureKestrel` binds both `Http1` on the REST port and `Http2` on the gRPC port). Gateway path prefix `/api/inventory/` (`RequireAuth: true`) for REST; gRPC is called directly (no gateway hop), same as User's gRPC surface.

## Routes (Carter endpoints, `Inventory.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/warehouses` | `CreateWarehouse.cs` | Create a warehouse with complete address information; a default storage zone is created automatically (RequireAdmin) |
| GET | `/warehouses/{warehouseId}` | `GetWarehouse.cs` | Fetch a warehouse by id (RequireAdmin) |
| GET | `/inventories/{inventoryId}` | `GetInventory.cs` | Fetch current stock quantity, incl. `VariantId` (RequireAuthenticated) |
| GET | `/inventories/{inventoryId}/history` | `GetInventoryHistory.cs` | List stock movement transactions for an inventory record (RequireAdmin) |
| POST | `/inventories/{inventoryId}/stock-in` | `StockIn.cs` | Increase stock, logs a StockIn transaction (RequireAdmin) |
| POST | `/inventories/{inventoryId}/stock-out` | `StockOut.cs` | Decrease stock, logs a StockOut transaction (RequireAdmin) |
| POST | `/inventories/{inventoryId}/adjust` | `AdjustStock.cs` | Directly correct stock to a known value, logs an Adjustment transaction (RequireAdmin) |
| GET | `/products/{productId}/stock` | `GetProductStock.cs` | `productId` alone → total stock across every variation/warehouse; `?productVariationId=` narrows to one variation. Same query the gRPC service is backed by. (RequireAuthenticated) |
| POST | `/warehouses/search` | `SearchWarehouses.cs` | Admin only. Paginated, filterable, sortable search over warehouses (`BuildingBlock.Criteria`, `WarehouseCriteriaDefinition`) |
| POST | `/inventories/search` | `SearchInventories.cs` | Admin only. Paginated, filterable, sortable search over stock-keeping rows (`InventoryCriteriaDefinition`) — no keyword search, stock rows have no text field |
| POST | `/inventory-transactions/search` | `SearchInventoryTransactions.cs` | Admin only. Paginated, filterable, sortable search over stock movements across every inventory row (`InventoryTransactionCriteriaDefinition`) — unlike `GetInventoryHistory`, not scoped to a single inventory id |

## gRPC: `InventoryGrpcService`

`BuildingBlock.Contract/Protos/inventory.proto` defines four RPCs. `Inventory.API/GrpcServices/InventoryGrpcServiceImpl.cs` is a thin adapter for all four (parse request → dispatch a Query/Command via `ISender` → map result), following User's `UserGrpcServiceImpl` pattern exactly. Wired via `AddGrpcServer()` in `Inventory.API/DependencyInjection.cs` and `app.MapGrpcService<InventoryGrpcServiceImpl>()` in `ApplicationPipeline.cs`. Two callers today: Order Service (`Order.Infrastructure/GrpcClients/InventoryClientService.cs`) — see [Order Service](order-service.md#grpc-client-order--inventory) and [reference/create-order-saga.md](../reference/create-order-saga.md) — and Product Service (`Product.Infrastructure/GrpcClients/InventoryClientService.cs`), which calls `GetProductsStock` to merge stock availability into Search Products results (fail-open: an unreachable Inventory Service yields `isInStock: null` for that page rather than failing the search).

- **`GetProductStock(GetProductStockRequest) returns (GetProductStockResponse)`** — `variant_id` is `optional` (proto3 `optional`, generates `HasVariantId`); omitted/empty means the whole-product rollup. Read-only; dispatches the same `GetProductStockQuery` the REST endpoint uses. Called by `CreateOrderHandler`'s pre-check (never reserves).
- **`GetProductsStock(GetProductsStockRequest) returns (GetProductsStockResponse)`** — batch rollup across every warehouse for each requested variation id, one round trip instead of N; a variation id with no inventory rows comes back as `0`, not omitted. Called by Order's `GetAvailableStockBatchAsync` (order-item validation) and Product's `SearchProductsHandler` (stock-in-search).
- **`DeductStock(DeductStockRequest) returns (DeductStockResponse)`** — new. `deduction_id` is the idempotency key for the whole batch (Order passes its `OrderId`); all requested `(variant_id, quantity)` items deduct together or none do. Dispatches `DeductStockCommand` (`Inventory.Application/Features/Inventories/Commands/DeductStock/`): validates every item's current stock against the `MAIN` warehouse first, and only if *all* are sufficient does it call `Inventory.Decrease` per item + log an `InventoryTransaction` (`StockOut`) + persist a `StockDeduction` row, inside one `IUnitOfWork.ExecuteTransactionAsync`. A repeat call with the same `deduction_id` replays the stored `StockDeduction` outcome instead of decrementing twice. Retries up to 3 times internally on `ConflictException` (a concurrent writer touched the same `Inventory` row — see "Concurrency" below), re-validating quantities fresh each attempt rather than blindly retrying stale numbers.
- **`RestockStock(RestockStockRequest) returns (RestockStockResponse)`** — new. Compensating action, looked up by the same `deduction_id` — reverses a previously-`Succeeded` `StockDeduction` (`Inventory.Increase` per item from `ItemsJson` + an `InventoryTransaction` `StockIn` entry), marks the ledger row `Reversed`. Idempotent: reversing an already-`Reversed` (or never-`Succeeded`) `deduction_id` is a no-op `Success: true` — a compensating action must never itself become a blocking failure.

## Concurrency

`InventoryConfig` maps `Inventory`'s `xmin` (Postgres's native per-row system column) as an EF concurrency token via a shadow property + `IsRowVersion()` — guards `StockIn`/`StockOut`/`Adjust`/`DeductStock`/`RestockStock` racing the same row. `EfUnitOfWork.ExecuteTransactionAsync` (`BuildingBlock.Persistence.Ef`) translates the resulting `DbUpdateConcurrencyException` into an Application-layer `ConflictException`, so callers above Persistence never need to reference an EF-specific type. **Migration authors:** never let a scaffolded migration `AddColumn`/`DropColumn` an `xmin` column — Postgres rejects `ALTER TABLE ... ADD COLUMN xmin` outright ("column name xmin conflicts with a system column name"); hand-remove those lines, the model-level mapping alone is sufficient.

## Messaging: consumes three Product-originated integration events

`Inventory.Infrastructure/Messaging/Consumers/` — each deserializes → dispatches an internal event via `IInternalEventDispatcher`, no business logic in the consumer itself (per `docs/reference/events.md`'s DOs/DON'Ts):

- **`VariantCreatedIntegrationEvent`** (topic `productvariationcreatedintegrationevent`) → `OnVariantCreatedEvent`/`Handler` — looks up the `PLATFORM` warehouse and creates a zero-stock `Inventory` row for the new variation. Checks `GetByVariationAndWarehouseAsync` first as an idempotency safety net beyond the Inbox dedup (a redelivered/replayed message must never create a second row for the same variation+warehouse). If `PLATFORM` is missing, logs a warning and skips (best-effort, same as before).
- **`VariantDeletedIntegrationEvent`** → `OnVariantDeletedEvent`/`Handler` — deletes every Inventory row for that variation (across all warehouses); a deleted variation no longer exists to hold stock against.
- **`ProductDeletedIntegrationEvent`** → `OnProductDeletedEvent`/`Handler` — deletes every Inventory row for the whole product in one pass. Needed because whole-product deletion is an EF cascade over the owned `Variant` rows on the Product side, so no per-variation Deleted event fires for each one individually.

This replaces the pre-redesign single `OnProductCreatedEvent` consuming `ProductCreatedIntegrationEvent.VariantId` — stock now keys off the variation-scoped events instead, since `ProductCreatedIntegrationEvent` no longer carries Sku/Price/variant info at all (see [Product Service](product-service.md#messaging-product--inventory--order-six-integration-events)).

## Inventory-specific building blocks (not present in User)

- **Dual Outbox+Inbox, like Order/User/Auth** — unchanged from before.
- **Read/Write persistence services, per [conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md).** `Inventory.Application/Abstractions/Persistence/{Inventories,Warehouses,InventoryTransactions,StockDeductions}/` hold the four aggregates' Read/Write service ports. `InventoryRepo`/`WarehouseRepo`/`StockDeductionRepo` all implement the generic `IRepository<T>` in full and are Scrutor-scanned (`AddScopedByInterface(typeof(IRepository<>), typeof(InventoryDbContext))`); `InventoryRepo` additionally exposes `DeleteByProductIdAsync`/`DeleteByVariationIdAsync` on its own `IInventoryRepository` (bulk deletes keyed by a foreign key, not the primary key). `InventoryTransactionRepo` is the one exception — append-only, no tracked-update need, so it stays a thin hand-written repo (`AddAsync` only) registered manually, same reasoning as Audit's `AuditLogRepo`. Every Read Service queries `InventoryDbContext` directly and independently of its repo — the rollup methods (`GetTotalStockBy*`) live there, not on any repo.
- **`AdjustStock`/`DeductStock`/`RestockStock`/`StockIn`/`StockOut` are the reference example for a cross-aggregate-root transaction.** Every one of these commits `InventoryEntity` + `InventoryTransaction` (+ `StockDeduction` for Deduct/Restock) atomically, so their handlers inject `IUnitOfWork` directly and call non-committing `InventoryWriteService.StageUpdateAsync`/`InventoryTransactionWriteService.StageAddAsync`/`StockDeductionWriteService.StageAddAsync`/`StageUpdateAsync` inside one `ExecuteTransactionAsync` the handler owns. This isn't a special case anymore, just the general rule applied to more than one call: **no `WriteService` method calls `ExecuteTransactionAsync`/self-commits by default** — `CreateWarehouseHandler` is actually the one *exception* here, since `WarehouseWriteService.CreateAsync` self-commits via a bare `unitOfWork.SaveChangesAsync()` after creating the warehouse aggregate with its default zone, coordinating the complete warehouse initialization workflow in a single business transaction. The 3 `OnProduct*` event handlers (`OnVariantCreatedHandler`/`OnProductDeletedHandler`/`OnVariantDeletedHandler`) each own `IUnitOfWork.ExecuteTransactionAsync` around one non-committing `InventoryWriteService` call — they are not self-committing, despite mutating only one aggregate.
- **No Redis / `ICacheService` usage** — unchanged.
- **Search, per [User's `SearchUsers` pattern](user-service.md).** `IWarehouseReadService`/`IInventoryReadService`/`IInventoryTransactionReadService` each gained a `SearchAsync(CriteriaRequest, ct)` member, backed by a static `CriteriaDefinition` per aggregate (`Inventory.Application/Features/Warehouses/Search/`, `Inventory.Application/Features/Inventories/Search/` — the transaction definition lives alongside Inventory's, not in its own folder, since it has no dedicated Feature folder of its own, matching where `GetInventoryHistory` already lives). See `docs/tasks/2026-07-22/Task5_warehouse-inventory-list-search-endpoints.md`.

## Naming note: the `Inventory` entity vs. the `Inventory` root namespace

Unchanged — `Inventory.Application`/`Inventory.Persistence` alias it:

```csharp
global using InventoryEntity = Inventory.Domain.Entities.Inventory;
```

## Known issues

- None yet.
