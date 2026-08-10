# Task 5: Add list/search endpoints for Warehouse, Inventory, and stock transactions

**Scope:** NovaCoreUI's `docs/tasks/2026-07-22/Task7_inventory-warehouse-notification-integration-gaps.md` (item 2) reports Warehouse/Inventory/Stock-Transaction admin list pages sourced from browser `localStorage` id-tracking instead of the backend, because "the backend has no list/search endpoint for Warehouse or Inventory at all." Confirmed true by re-reading `Inventory.API/Endpoints/` directly — genuinely no `Search`/`List` file exists there, unlike User (see Task 4 — User's equivalent gap turned out to already be fixed; Inventory's has not been touched at all).

## Confirmed gap

`Inventory.API/Endpoints/` today: `CreateWarehouse.cs`, `GetWarehouse.cs`, `GetInventory.cs`, `GetInventoryHistory.cs` (single-inventory-scoped), `StockIn.cs`, `StockOut.cs`, `AdjustStock.cs`, `GetProductStock.cs`. No endpoint returns more than one record by anything other than a single id/product lookup.

The Read-service abstractions already have the same shape User's had *before* its search endpoint was added — `Get*Async` only, no `SearchAsync`:

- `IWarehouseReadService` (`Inventory.Application/Abstractions/Persistence/Warehouses/IWarehouseReadService.cs`): `GetByIdAsync`, `GetByCodeAsync`.
- `IInventoryReadService` (`Inventory.Application/Abstractions/Persistence/Inventories/IInventoryReadService.cs`): id/product-scoped lookups only (backs `GetInventory`/`GetProductStock`).
- `IInventoryTransactionReadService` (`Inventory.Application/Abstractions/Persistence/InventoryTransactions/IInventoryTransactionReadService.cs`): backs `GetInventoryHistory`, scoped to one `inventoryId` — there is no cross-inventory transaction list. NovaCoreUI's `inventory.queries.ts` (`useTransactionsForInventoryIds`) already works around this by calling `GetInventoryHistory` once per known inventory id and merging client-side — its own comment says outright: *"there's no single 'list all transactions' endpoint."*

So this is really **three** missing list/search surfaces, not one: Warehouses, Inventory records, and Inventory transactions (stock movements) each need their own.

## The pattern to replicate: User's `SearchUsers` (Task 4 confirmed this is real and working)

No new design needed — copy the shape that already ships for User, using the same shared `BuildingBlock.Criteria` library:

1. **Criteria definition** (static, one per aggregate) — e.g. `User.Application/Features/Users/Search/UserCriteriaDefinition.cs`:
   ```csharp
   public static readonly CriteriaDefinition<Warehouse> Instance = CriteriaDefinition<Warehouse>.Create()
       .Field(x => x.Code).String().Sortable().KeywordSearchable()
       .Field(x => x.Name).String().Sortable().KeywordSearchable()
       .Field(x => x.Address).String().KeywordSearchable()
       .Field(x => x.Status).Enum().Sortable()
       .Build();
   ```
   Analogous fields available on `Inventory` (`ProductId`, `ProductVariationId`, `WarehouseId`, `Quantity` — all filterable/sortable, no free-text field on this one) and `InventoryTransaction` (`InventoryId`, `ProductId`, `ProductVariationId`, `WarehouseId`, `Type`, `Quantity`, `QuantityAfter`, `Reason`, plus `CreatedAt` via `IAuditable`/`BaseEntity` for date-range filtering — matches how `GetInventoryHistory` is already consumed, "movements over time").

2. **Read service method** — add `SearchAsync(CriteriaRequest, ct)` to each of the three interfaces above, implemented exactly like `UserProfileReadService.SearchAsync` (`User.Persistence/UserProfiles/Read/UserProfileReadService.cs:19-25`):
   ```csharp
   public async Task<PaginatedResult<Warehouse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
       => await dbContext.Warehouses.AsNoTracking().ApplyCriteria(WarehouseCriteriaDefinition.Instance, request).ToCriteriaPagedResultAsync(request, ct);
   ```

3. **Query + handler + Carter endpoint**, one triad per aggregate, mirroring `SearchUsersQuery`/`SearchUsersHandler`/`SearchUsersEndpoint` file-for-file. Suggested routes: `POST /warehouses/search`, `POST /inventories/search`, `POST /inventory-transactions/search` (all under `Inventory.API`, same service — no new microservice needed).

## Open decision: authorization policy

Every existing Inventory mutation endpoint (`CreateWarehouse`, `StockIn`/`StockOut`/`AdjustStock`, `GetWarehouse`) is `RequireAdmin`; `GetInventory`/`GetProductStock` are `RequireAuthenticated` (broader — any logged-in caller can check stock). User's `SearchUsers` is `RequireAdmin`. Since NovaCoreUI's Warehouse/Inventory/Stock-Transaction *list* pages are admin-dashboard-only screens (not customer-facing, unlike Shop/Cart), `RequireAdmin` on all three new search endpoints is the consistent default — flag if a broader read audience turns out to be needed.

## Status

Done. Built exactly as scoped: `SearchAsync` added to `IWarehouseReadService`/`IInventoryReadService`/`IInventoryTransactionReadService`, one `CriteriaDefinition` per aggregate, three Query/Handler/Validator triads, three Carter endpoints (`POST /warehouses/search`, `POST /inventories/search`, `POST /inventory-transactions/search`), all `RequireAdmin` as recommended. Added supporting indexes (migration `AddSearchIndexes`): `WarehouseId` on `Inventory`; `ProductId`/`ProductVariationId`/`WarehouseId`/`Type` on `InventoryTransaction`. `Inventory.Application` needed a new `BuildingBlock.Criteria` project reference (User already had it). Also added `InventoryDbContextFactory` (`Inventory.API`) - Inventory was the one service missing a design-time `IDesignTimeDbContextFactory`, so `dotnet ef migrations add` was booting the full app host (Kafka, Hangfire, ...) and failing; now matches Order/User's existing pattern. `docs/services/inventory-service.md` updated (routes table + new bullet under "Inventory-specific building blocks").

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-22/Task7_inventory-warehouse-notification-integration-gaps.md`.
