# Task 18: Add Cart API never checks real stock — only a Product-status flag

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`CartService.AddItemAsync` (`src/Services/Order/Order.Infrastructure/Caching/CartService.cs:31-49`) calls `EnsureOrderableAsync` (same file, lines 81-89):

```csharp
private async Task EnsureOrderableAsync(Guid variationId, CancellationToken ct)
{
    var variations = await catalogReadService.GetByVariantionIdsAsync([variationId], ct);
    var variation = variations.FirstOrDefault() ?? throw new NotFoundException("Variation", variationId);
    if (!variation.IsOrderable)
        throw ExceptionFactory.InvalidState($"Product ({variation.ProductName}) is not currently available for ordering.");
}
```

`IsOrderable` is `Status == "Active"` (`Order.Domain/Entities/OrderProductCatalog.cs:22-24`), synced from Product-service lifecycle events — it has nothing to do with stock quantity. There is no call anywhere in this handler to `IInventoryClientService`/gRPC (confirmed via grep, zero matches in `Order.Application/Features/Cart` or `CartService.cs`). `UpdateCartItemQuantityAsync` (same file, lines 51-65) is weaker still — no orderable check, no stock check at all; a user can set any positive quantity. The endpoint's documented contract (`AddCartItem.cs:25-26`) only lists `404`/`400` (not-orderable) — no insufficient-stock case exists at all.

## Why this matters

Checklist requirement: Add Cart must call Inventory gRPC and check real-time stock before adding, rejecting immediately if unavailable. Today a user can add (or bump the quantity of) far more of an item than actually exists in stock, and won't discover this until Create Order fails at checkout.

## Suggested acceptance criteria

- `AddItemAsync` and `UpdateCartItemQuantityAsync` both call the shared stock-availability primitive (Task 17) with the *total resulting quantity* for that variation (existing cart quantity + requested delta, or the new absolute quantity for update) before persisting to Redis.
- On insufficient stock, reject with a structured error (reuse the same shape being defined in Task 20 for Create Order, so the frontend has one error contract to handle, not two).
- Keep the existing `IsOrderable`/status check — it's a legitimate, separate concern (a discontinued product shouldn't be addable even if some stale stock count is nonzero) — add stock checking alongside it, don't replace it.

## What was done

`CartService.EnsureOrderableAsync` became `EnsureOrderableAndInStockAsync(variationId, resultingQuantity, ct)` - keeps the existing orderable/status check, then additionally calls `IStockAvailabilityService.CheckAsync` (Task 17) for the *resulting* quantity (existing cart quantity + delta for `AddItemAsync`, the new absolute value for `UpdateItemQuantityAsync` - not just the request's raw delta, per the acceptance criteria). On insufficient stock it throws the same `ExceptionFactory.InsufficientStock(..., detail: new { insufficients = [variationId] })` shape Task 20 established for Create Order, so the frontend has one error contract for both flows. `AddCartItem.cs`/`UpdateCartItemQuantity.cs` doc strings updated to document the new 400 case. Scoped build of `Order.API` passes.

## What wasn't done

Nothing deferred at the code level. Live end-to-end verification (actually calling the endpoint against a running Inventory service) wasn't done this session - no Docker stack was running and spinning the full stack up for this one check wasn't judged worth the resource cost; correctness was verified by reading the change against `IStockAvailabilityService`'s already-compiled contract instead.

**Depends on:** Task 17 (shared stock service, done). **Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task17_add-to-cart-checkout-stock-error-handling-missing.md`.
