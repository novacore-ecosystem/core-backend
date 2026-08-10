# Task 22: GetProduct response has no per-variation stock data

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`ProductVariationResponse` (`src/Services/Product/Product.Application/.../ProductVariationResponse.cs:4-33`) has no stock/quantity/availability field — only `sku, barcode, price, cost, weight, dimensions, images, status, isDefault, displayOrder` (confirmed against the frontend-consumed shape in `get-product.ts:4-19`, identical field set). `GetProductHandler.cs:15-25` never queries Inventory. There is no code path today that returns per-variation stock to a Product Detail caller.

## Why this matters

Checklist requirement: Product Detail must show every variation's own stock state and disable out-of-stock variations in the selector, preventing selection of unavailable variations. This is structurally impossible today — there's no stock field on the DTO to check, not just a missing UI check (the frontend side of this, NovaCoreUI Task 16, is blocked on this).

## Suggested acceptance criteria

- Add a stock/availability field to `ProductVariationResponse` (e.g. `AvailableStock: int`), populated via a batched Inventory lookup (same primitive as Task 21, ideally the same client/service so this isn't a third reimplementation) keyed by all of the product's variation IDs in one call.
- `GetProductHandler` calls it once per request (batched across all the product's variations, not N+1 per variation).
- Confirm whether Product Detail is latency-sensitive enough that a synchronous Inventory round-trip per page load is acceptable, or whether a cached/eventually-consistent stock snapshot is preferable — same architectural question as Task 21, should be answered once for both.

## What was done

Resolved via direct gRPC, same decision as Task 21. `ProductVariationResponse` gained `AvailableStock: int?` (nullable trailing param, default `null`) and `From(ProductVariation, int? availableStock = null)`. `GetProductHandler` now injects the already-existing `IInventoryClientService`/`IAppLogger<GetProductHandler>`, batches a single stock lookup across all of the product's variation IDs, and passes each variation's resolved stock into `From`. Same fail-open try/catch pattern as `SearchProductsHandler` (`GetStockByVariationIdAsync`) - an Inventory outage yields `AvailableStock: null` (unknown) for every variation rather than failing the whole request or reporting false zeros. `GetProduct.cs` doc string updated. Scoped build of `Product.API` passes.

`AddVariationHandler`'s existing `ProductVariationResponse.From(variation)` call site (Task 16) is untouched - it still omits `AvailableStock` (stays `null`), since a synchronous Inventory call immediately after creating a variation (whose zero-stock Inventory row is created asynchronously via `OnProductVariationCreatedHandler`) wasn't part of this task's scope and would likely just report 0 or race the async row creation.

## What wasn't done

Same caching/latency caveat as Task 21 - not pursued speculatively. No live verification against a running Inventory service this session.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task16_product-detail-no-per-variation-stock-ui.md`. Related: Task 21 (same underlying data need, different endpoint).
