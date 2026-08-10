# Task 17: No shared stock-availability service — Cart and Order each roll their own (different) check

**Status:** Resolved 2026-07-27 (Order-side). Cart consumption is Tasks 18/19.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

Two structurally different checks exist today, and they are not shared:

| Flow | Class/method | What it checks | Calls Inventory gRPC? |
|---|---|---|---|
| Create Order | `OrderItemPreparationService.EnsureStockAvailableAsync` (`src/Services/Order/Order.Application/Features/Orders/Common/OrderItemPreparationService.cs:53`), via `IInventoryClientService.GetAvailableStockBatchAsync` (`Order.Infrastructure/GrpcClients/InventoryClientService.cs:10`) | Real stock quantity per variation | Yes |
| Add/Get Cart | `CartService.EnsureOrderableAsync`/`EnrichAndPruneAsync` (`Order.Infrastructure/Caching/CartService.cs:81`, `:103`), via `IOrderProductCatalogReadService.GetByVariantionIdsAsync` | Product `Status`/`IsOrderable` flag only (synced from Product-service events) | No |

`OrderItemPreparationService` is the only place real Inventory stock checking happens today, used by both `CreateOrderHandler` and `AdminCreateOrderHandler` via `OrderCreationService`. `CartService`'s checks never call Inventory at all — they're a different, narrower check (active/inactive product status) that happens to look similar but doesn't address the checklist's actual goal (real-time stock).

## Why this matters

Checklist section 4 explicitly asks for "a single source of truth for stock validation" across Get Cart, Add Cart, and Create Order. Building Add Cart's and Get Cart's real-time stock checks (Tasks 18/19) by copy-pasting `EnsureStockAvailableAsync`'s logic into `CartService` would recreate the exact duplication this section is meant to eliminate — Order Search/Detail (Tasks 21/22) will also need the same batched-availability primitive, so extracting it now avoids a fourth or fifth reimplementation.

## Suggested acceptance criteria

- Extract a shared `IStockAvailabilityService` (or similar) in `Order.Application` (or a location reusable by Product if a Task 21/22 result needs it cross-service — confirm whether Product should call Order/Inventory directly or vice versa; recommend Product calls `IInventoryClientService` directly rather than routing through Order, keeping the shared piece scoped to "given variation IDs + quantities, return which are insufficient" logic reusable within Order Service, and a separate simpler "given variation IDs, return available stock map" reusable by Product).
- Method shape: given a batch of `(variationId, requestedQuantity)`, return which are insufficient and by how much (generalizing `EnsureStockAvailableAsync`'s current insufficiency-detection logic without the throw, so callers can decide how to react — Create Order throws, Add/Get Cart mark items).
- `OrderItemPreparationService`, `CartService.EnsureOrderableAsync` (extended for real stock, not just status), and `CartService.EnrichAndPruneAsync` (extended for live availability) all depend on the shared service rather than each having their own Inventory-calling logic.

## What was done

Added `IStockAvailabilityService` (`Order.Application/Abstractions/Services/IStockAvailabilityService.cs`) — one method, `CheckAsync(IReadOnlyCollection<StockRequest>, ct) -> IReadOnlyDictionary<Guid, StockAvailability>`, where `StockRequest(VariationId, RequestedQuantity)` and `StockAvailability(VariationId, RequestedQuantity, AvailableQuantity)` with an `IsSufficient` computed property. Never throws - purely a batched lookup, matching the "callers decide how to react" design. Implemented by `StockAvailabilityService` (`Order.Application/Features/Stock/`), which wraps the existing `IInventoryClientService.GetAvailableStockBatchAsync` (one gRPC round trip regardless of how many distinct variation IDs are requested). Registered in `Order.Application/DependencyInjection.cs`.

`OrderItemPreparationService.EnsureStockAvailableAsync` now depends on `IStockAvailabilityService` instead of `IInventoryClientService` directly - same behavior (batched check, fail-fast before catalog validation), now sourced from the shared service. This also directly produced Task 20's fix (see that task file) since it's the same method.

Placed in `Order.Application` rather than a cross-service location: it only depends on the existing `IInventoryClientService` Application-abstraction (not on `Order.Infrastructure` concretions), matching where `OrderItemPreparationService`/`OrderCreationService` already live. Scoped build of `Order.API` passes.

## What wasn't done

Cart (`CartService.EnsureOrderableAsync`/`EnrichAndPruneAsync`) does not consume `IStockAvailabilityService` yet - that's Tasks 18 and 19, done as their own vertical slices next. Product Search/Detail's stock needs (Tasks 21/22) are a separate open architectural question (whether Product should depend on Inventory directly) and are not part of this shared service, which is scoped to Order Service.

**Enables:** Task 18 (Add Cart), Task 19 (Get Cart). Task 20 (Create Order structured error) — done as part of this same change.
