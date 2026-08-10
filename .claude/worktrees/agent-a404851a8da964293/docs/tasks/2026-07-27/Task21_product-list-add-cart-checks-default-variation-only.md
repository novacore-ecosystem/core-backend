# Task 21: Product Search's `isInStock` reflects only the default variation, not "any variation in stock"

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`SearchProductsItemResponse.isInStock` (`src/Services/Product/Product.Application/.../SearchProductsQuery.cs` — see `Product.Application`'s search DTOs; frontend consumer confirms it via `services/product/search-products.ts:24,34` with a doc comment "Null when there's no default variation...") is computed against the product's **default variation only**. There is currently no per-variation or aggregate-across-variations stock signal in the Product Search pipeline at all — `ProductSearchDocument` (see Task 15) doesn't carry stock data, and stock itself lives entirely in the Inventory service, reached only via gRPC from Order Service today (`IInventoryClientService`). Product Service has no existing dependency on Inventory's gRPC client.

## Why this matters

Checklist requirement: Product List's Add Cart button should disable only when ALL variations are out of stock; if any variation still has stock, it should stay enabled. Today a product whose *default* variation happens to be out of stock shows as fully unavailable even if three other variations are well-stocked — actively hiding purchasable inventory from customers.

## Suggested acceptance criteria

- Decide the computation point: either (a) Product Service calls Inventory's gRPC stock-batch endpoint when building search results (introduces a new cross-service dependency, consistent with how Order Service already does it), or (b) compute at query time on the frontend/BFF layer by fetching stock separately per result page. Recommend (a) for consistency with the "single source of truth" goal in Task 17, exposing an `IInventoryClientService`-equivalent client in Product Service (or, if Product should not depend on Inventory directly by design, route through Order's existing client via an internal API — confirm architectural boundary before implementing).
- Replace or supplement `isInStock` with an aggregate flag computed as OR across all active variations' stock > 0, not just the default variation's.
- This will add a per-search-results-page Inventory round-trip (batched by all variation IDs on the page) — confirm acceptable latency/caching approach given Product Search is a high-traffic, low-latency-sensitive path unlike Order/Cart flows.

## What was done

Resolved via direct gRPC (confirmed with the user over the alternative of an event-driven local read-model, given Inventory doesn't publish domain stock events yet - that gap is tracked separately as this folder's Task 7 from the earlier business-requirements audit). Turned out Product Service already had the client wired for exactly this purpose but only used it for the Default variation:

- `ProductSearchDocument` gained `VariationIds: IReadOnlyList<Guid>` (every Active variation's id - same Active-only filter as Task 15's `VariationNames`), mapped as `Keyword` in `ProductSearchIndexMapping` (same treatment as `CategoryIds`/`DefaultVariationId`). `ProductSearchProjectionBuilder` populates it.
- `SearchProductsHandler` now batches Inventory's stock lookup across the *union* of every result's `VariationIds` (was: only each product's `DefaultVariationId`) - still one gRPC call per results page, same fail-open try/catch as before (`GetStockByVariationIdAsync` unchanged). `IsInStock` is now `d.VariationIds.Any(id => stockByVariationId.GetValueOrDefault(id) > 0)` instead of checking only the default variation.
- `SearchProductsItemResponse.IsInStock`'s doc comment updated; null still means "no Active variations" or "Inventory unreachable," not false.
- `docs/reference/search.md` updated for the new field + `IsInStock` semantics.
- Scoped build of `Product.API` passes.

## What wasn't done

Requires a full reindex (`RebuildProductSearchIndex`) after deploy, same caveat as Task 15 - not run live this session. No caching layer was added for the Inventory round trip; flagged as a possible follow-up if Product Search's higher traffic volume (vs. Order/Cart) makes this the bottleneck the acceptance criteria worried about, but not built speculatively.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task15_product-list-add-cart-single-variation-only.md`.
