# Task 23: UpdateOrder fails unconditionally — not a race condition, a pre-existing EF Core change-tracking bug

**Status:** Open. Discovered while building the `UpdateOrderRaceConditionTests` diagnostic suite (see `tests/integration/Order.IntegrationTests/`); NOT fixed as part of that work, per that task's explicit "don't modify production code" instruction. See `Task24_race-test-retargeted-to-updateownerinfo.md` (2026-07-28): that suite has since been retargeted to `UpdateOrderOwnerInfoCommand` to get a useful concurrency signal without waiting on this fix — `UpdateOrder` itself remains untested for concurrency and this bug is still unfixed.

## Source

User asked for a dedicated integration test reproducing a race condition when two requests update the same Order simultaneously, to later verify optimistic concurrency (xmin/RowVersion)/distributed locking actually fixes it. While building that test, every iteration showed **both** concurrent requests failing with `ConflictException` (`DbUpdateConcurrencyException`) — never the expected "one 200, one 409." A follow-up single-request, zero-concurrency sanity check confirmed: a **lone** `UpdateOrder` call, with no second request at all, **also** fails the same way, every time.

## Current state

`UpdateOrderHandler` → `OrderWriteService.UpdateItemsAsync` → `OrderRepo.UpdateAsync` loads the `Order` (tracked, `Include(Items).Include(Owner)`), then calls `Order.UpdateItems(items)` (`Order.cs:47-64`):

```csharp
Items.Clear();
foreach (var model in models)
    Items.Add(OrderItem.Create(Guid.CreateVersion7(), Id, model.ProductId, model.ProductName, model.UnitPrice, model.Quantity, model.Discount));
Tourch();
```

The new `OrderItem` entities are added via **plain POCO collection mutation** (`Items.Add(...)`), not via `context.Add(...)`/graph-traversal from an explicit `Set<T>().Add()` call. EF Core only discovers them on the next `ChangeTracker.DetectChanges()` pass, and for entities discovered this way — as opposed to reached via explicit `Add()` on a `DbSet`/root entity, which unconditionally marks the whole graph `Added` — EF Core falls back to a **key-value heuristic**: if the primary key is already at a non-default value (which `Guid.CreateVersion7()` always produces — never `Guid.Empty`) and the key's `ValueGenerated` isn't configured as `Never`, EF Core cannot tell "brand-new entity with a client-generated key" apart from "an entity that already exists in the DB and should have been `Attach`ed," and picks **`Modified`**, not `Added`.

Confirmed directly via a temporary `ISaveChangesInterceptor` printing `ChangeTracker.Entries()` right before `SaveChanges`: the new `OrderItem`s are `[Modified]`, not `[Added]`. The generated SQL for them is therefore `UPDATE order_items SET ... WHERE id = @newGuid` — a row that has never existed — which affects 0 rows. EF Core's uniform "expected 1 row affected" check (applies to *every* Modified/Deleted entity, not just ones with an explicit concurrency token) throws `DbUpdateConcurrencyException` for this 0-row `UPDATE`, which `EfUnitOfWork.ExecuteTransactionAsync` catches and rethrows as `ConflictException` ("The record was modified concurrently. Please retry.") — a **misleading message**, since nothing was actually modified concurrently; the "new" row simply never existed.

By contrast, `CreateOrderAsync`'s initial items (created via `writeService.CreateAsync(order)` → `repo.AddAsync(order, ct)` → `dbContext.Orders.AddAsync(entity, ct)`) go through EF's **graph-traversal `Add()`** path, which unconditionally marks every reachable entity (including `Items`) `Added` regardless of key value — which is why order *creation* works fine and only *replacing* an existing order's items is broken.

## Why this matters

This is not an edge case — **every single call to `PUT /orders/{orderId}` (UpdateOrder) fails**, unconditionally, today. Any admin or customer flow that edits a Pending order's item list (e.g. NovaCoreUI's `EditOrderItemsForm`, built in an earlier session) is completely broken in practice, not just under concurrent load. This is a far more urgent bug than the race condition it was originally mistaken for — concurrency behavior is moot if the base single-request case never succeeds.

## Suggested acceptance criteria

- `OrderItem`'s new-vs-existing determination must not depend on its Guid key's value. Standard fixes, roughly in order of how idiomatic they are for this codebase:
  1. Configure `.Property(x => x.Id).ValueGeneratedNever()` in `OrderItemConfig.cs` (tells EF the app always supplies the key and a non-default value doesn't imply pre-existing) **and** confirm this actually resolves the navigation-discovery ambiguity (it should — with `ValueGeneratedNever`, EF stops using the key-value heuristic entirely for this property) — verify empirically with a quick test before considering this fixed, since this is the crux of the whole bug.
  2. Alternatively/additionally, have `OrderRepo.UpdateAsync` (or `Order.UpdateItems`'s caller) explicitly call `dbContext.OrderItems.Add(newItem)` (or `dbContext.Entry(newItem).State = EntityState.Added`) for each new item instead of relying purely on collection-navigation discovery — more explicit, doesn't depend on a global convention fix, but requires touching `OrderRepo`/`OrderWriteService` instead of just the EF config.
- After fixing, re-run `tests/integration/Order.IntegrationTests/Concurrency/UpdateOrderRaceConditionTests.cs` — it should then show the originally-expected "one 200, one 409 (Conflict)" steady state (or, if the fix removes even that, whatever the new correct behavior is), confirming both this bug and the original concurrency question are resolved.
- Worth auditing whether any *other* domain aggregate's "replace a child collection wholesale" method (search for other `.Clear()` + `.Add()` patterns on a tracked collection navigation) has the identical bug — this pattern likely isn't unique to Order.

## What wasn't done

Not fixed here, per the race-condition task's explicit "don't modify production code unless absolutely required" instruction — this deserves its own dedicated fix-and-verify pass, not a drive-by change bundled into an unrelated test-writing task. Also not audited: whether other aggregates (Product's variations, etc.) share this exact "Clear + Add on a tracked collection with client-generated Guid keys" pattern and therefore the same bug — flagged as worth checking, not confirmed either way.
