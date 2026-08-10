# Task 19: Get Cart returns no live stock/availability state per item

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`CartService.GetCartAsync` → `EnrichAndPruneAsync` (`src/Services/Order/Order.Infrastructure/Caching/CartService.cs:25-29, 103-125`) re-resolves each cart item's name/price/orderable-status from the local `OrderProductCatalog` table and **drops** (silently prunes) lines whose variation is deleted/inactive — but this is the same Product-status flag as Task 18, not a real Inventory stock check. `IInventoryClientService`/`GetAvailableStockBatchAsync` is never referenced in this file. `CartItemResponse` (`Order.Application/Abstractions/Services/ICartService.cs:23`) has fields `(ProductId, VariationId, ProductName, UnitPrice, Quantity)` — there is no `AvailableStock`/`IsInStock` field in the response shape at all, so there's currently nowhere to put an availability signal even if the check existed.

## Why this matters

Checklist requirement: Get Cart should check latest stock and return an availability state per item, reusing Add Cart/Create Order's logic rather than duplicating it. Today a user's cart can silently contain items that are no longer in sufficient stock, with no signal until they hit Create Order and get a checkout-time rejection — a worse UX than catching it while just viewing the cart.

## Suggested acceptance criteria

- Add an availability field to `CartItemResponse` (e.g. `AvailableStock: int` and/or `IsInsufficientStock: bool` given the item's current `Quantity`).
- `EnrichAndPruneAsync` calls the shared stock-availability primitive (Task 17) in the same batched-by-variation-IDs style already used for the catalog lookup, and populates the new field(s) — do not prune insufficient-stock items the way inactive ones are pruned; the cart should keep the line but mark it, per the checklist ("return availability state for every cart item," not remove them).
- Confirm this doesn't turn `GetCart` into a synchronous-gRPC-per-request bottleneck at scale — the existing catalog lookup is already a DB round-trip per `GetCart` call, so adding one more batched external call is consistent with current cost, but flag if this needs caching/backoff for a later pass.

## What was done

`CartItemResponse` gained `AvailableStock: int` and `IsInsufficientStock: bool`. `CartService.EnrichAndPruneAsync` now calls `IStockAvailabilityService.CheckAsync` (Task 17), batched across every surviving line (after the existing deleted/unorderable pruning, which is unchanged), and populates both new fields per line - one gRPC round trip regardless of cart size, same batching discipline as the existing catalog lookup right above it. Insufficient-stock lines are kept, not pruned, matching "surface it, don't silently drop it." `GetCart.cs` doc string updated. Scoped build of `Order.API` passes.

## What wasn't done

No caching/backoff was added for the new per-`GetCart` Inventory round trip - flagged as a possible follow-up if `GetCart` traffic volume makes the extra gRPC call a bottleneck, but not pursued speculatively without evidence it's needed. No live end-to-end verification against a running Inventory service this session (Docker wasn't up).

**Depends on:** Task 17 (shared stock service, done).
