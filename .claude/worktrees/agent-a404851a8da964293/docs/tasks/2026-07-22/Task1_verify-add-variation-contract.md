# Task 1: Verify AddVariation contract against the frontend's "cannot create variation" report

**Scope:** Backend-side double-check of Frontend Task 2 (NovaCoreUI `docs/tasks/2026-07-22/Task2_add-variation-cannot-create.md`) — confirm whether the Product service itself rejects the reported payload, and why.

## Reported payload (from the frontend session)

```json
{
  "sku": "E2EB2SKU-1784707349",
  "price": 70,
  "barcode": "1234512412",
  "cost": 20,
  "images": []
}
```

No HTTP status code or response body was captured alongside this — see "Open questions" below.

## Investigation

Files: `Product.API/Endpoints/AddVariation.cs`, `Product.Application/Features/Products/Commands/AddVariation/{AddVariationCommand,AddVariationValidator,AddVariationHandler}.cs`, `Product.Domain/ValueObjects/{Sku,Barcode}.cs`, `Product.Domain/Entities/ProductVariation.cs`, `Product.Persistence/Repository/ProductRepo.cs`.

**Field-level validation passes cleanly for this payload:**

- `AddVariationRequest` (`Product.API/Endpoints/AddVariation.cs:9-19`) declares `Weight`, `DimensionsLength/Width/Height` as `decimal? = null` — all genuinely optional, no default-value trap.
- `AddVariationValidator.cs:25-27` only checks `Weight` via `ProductVariation.IsValidWeight` (`weight is null || weight > 0`) — passes when omitted. **No rule exists at all for the three Dimensions fields** — they're unvalidated, so their absence can't fail validation either.
- `Sku = "E2EB2SKU-1784707349"` passes `Sku.IsValid` (regex `^[A-Z0-9-]+$`, ≤50 chars).
- `Barcode = "1234512412"` (10 digits) passes `Barcode.IsValid` (regex `^[0-9]{8,14}$`).
- `Price = 70`, `Cost = 20` both pass (`>= 0`).
- `Images = []` — no rule on Images at all.

So a 400 from `ValidationBehavior` is **not** the expected outcome for this exact payload.

**Most likely actual failure: 409 Conflict from a *global*, not product-scoped, SKU-uniqueness check.**

`AddVariationHandler.cs:19-23`:

```csharp
if (await productRepo.SkuExistsAsync(request.Sku, ct: ct))
    throw new ConflictException($"Variation with SKU ({request.Sku}) already exists");

var product = await productRepo.GetByIdAsync(request.ProductId, ct)
    ?? throw new NotFoundException(nameof(ProductEntity), request.ProductId);
```

The uniqueness check runs **before** the target product is even loaded, and `SkuExistsAsync` (`ProductRepo.cs:60-67`) scans every product's variations with no `ProductId` filter:

```csharp
public async Task<bool> SkuExistsAsync(string sku, Guid? excludeVariationId = null, CancellationToken ct = default)
{
    var normalized = Sku.Create(sku);
    return await dbContext.Products.AsNoTracking()
        .SelectMany(p => p.Variations)
        .AnyAsync(v => v.Sku == normalized && (excludeVariationId == null || v.Id != excludeVariationId), ct);
}
```

`ConflictException` → HTTP **409** (`BuildingBlock.Application.Exceptions.ConflictException`, `statusCode: 409`).

**Read:** if `"E2EB2SKU-1784707349"` was already inserted once — by an earlier attempt, a different product, or a retried E2E test (the timestamp-looking suffix strongly suggests an automated test SKU) — every subsequent `AddVariation` call with that same SKU 409s, *regardless of which product it targets*. This matches "cannot create new variation" exactly if the repro was attempted more than once with the same literal payload.

## Open questions / not yet confirmed — resolved 2026-07-22

- **Is global (cross-product) SKU uniqueness intentional?** **Confirmed yes by the user.** SKU uniqueness stays global across all products, not scoped per-product — not a bug, matches the documented endpoint contract. No behavior change made.
- **`AddVariation`'s auth requirement is undocumented.** **Was already documented** — `docs/services/product-service.md:38` states `(RequireAdmin)` for this route. The earlier claim it was undocumented was itself stale; no doc change needed.
- **The actual HTTP status + response body from the failed attempt were never captured.** Still true — this was never reproduced against a live repro payload with a captured response. Left open, but no longer blocking: the two other open questions are resolved, and a real, related bug was found and fixed independently (below), which is the more likely actual explanation for an intermittent "cannot create" report.

## Real bug found and fixed (separate from the two open questions above)

Investigating this surfaced a genuine TOCTOU race: `AddVariationHandler.cs`'s `SkuExistsAsync` pre-check and the actual DB insert aren't atomic, so two concurrent requests submitting the same new SKU could both pass the pre-check and then collide on the DB's unique index (`ix_product_variations_sku`) — this surfaced as an unhandled 500, not a clean 409. This is a more likely explanation for an intermittent/flaky "cannot create variation" report than a simple pre-existing duplicate SKU (which would 409 consistently, not intermittently).

**Fixed:** `BuildingBlock.Persistence.Ef/UnitOfWork/EfUnitOfWork.cs`'s `ExecuteTransactionAsync` now also translates a Postgres unique-violation (`SqlState 23505`) into `ConflictException` (409), alongside the existing `DbUpdateConcurrencyException` translation. This is shared infra, so it fixes the same class of race for every service's unique-index checks, not just Product's SKU. Covered by `tests/unit/BuildingBlock.Persistence.Ef.Tests/EfUnitOfWorkTests.cs` (2 tests).

**Also improved:** the 409 message now names which product currently owns the conflicting SKU (`AddVariationHandler.cs`, backed by a new `IProductRepository.GetProductNameBySkuAsync`), e.g. `"Variation with SKU (X) already exists (used by product \"Y\")"` — removes the confusion the second open question flagged.

## Status

**Resolved.** Both open questions answered; the TOCTOU race (the more likely actual cause of an intermittent repro) fixed and tested; error message improved. If "cannot create variation" still reproduces after this fix, it's a new/different issue — re-open with an actual captured HTTP status + response body next time.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-22/Task2_add-variation-cannot-create.md`.
