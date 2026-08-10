# Task 24 — UpdateOrderRaceConditionTests retargeted to UpdateOrderOwnerInfoCommand

**Status:** Done. Does **not** close Task 23 — `UpdateOrder` (item replacement) itself is
still unconditionally broken; this only unblocks the diagnostic suite by testing a
different command that was never affected by that bug.

## Why

`UpdateOrderRaceConditionTests` (`tests/integration/Order.IntegrationTests/Concurrency/`)
originally fired two concurrent `UpdateOrderCommand` (item-replacement) requests to
observe whether xmin-based optimistic concurrency produces the expected "one 200, one
409" outcome. Task 23 found that `UpdateOrder` fails unconditionally, even with zero
concurrency, so the suite could only ever observe "409, 409" — never exercising the
scenario it was built for.

Rather than wait on Task 23's fix (an unrelated, unfixed EF change-tracking bug — see
that doc), the suite was rewritten (2026-07-28) to fire two concurrent
`UpdateOrderOwnerInfoCommand` requests instead. Two independent reasons this is valid,
not just a workaround:

1. **Order Items cannot change after creation outside the Pending window** —
   `Order.UpdateItems` throws once `Status` has moved past `Pending`. A meaningful
   item-replacement race test would need to keep the order Pending for the whole test,
   a narrower and less representative window than owner-info edits (valid through
   `Confirmed`).
2. `UpdateOrderOwnerInfoCommand` → `OrderWriteService.UpdateOwnerInfoAsync` only mutates
   `Order.Owner` (`CustomerPhone`/`ShippingAddress`), never `Items` — it never touches
   the `Items.Clear()`/`Items.Add()` code path Task 23 identified as broken, so it's
   unaffected by that bug regardless of when it gets fixed.

It still exercises the same concurrency mechanism the original test targeted:
`Order.UpdateOwnerInfo` calls the same `Tourch()` every other mutator does, bumping the
`Orders` row's `UpdatedAt` — so two concurrent Owner mutations still compete for the same
per-Order `xmin` lock (`OrderConfig.cs`'s `.IsRowVersion()`), even though `Owner` lives in
a separate 1:1 `OrderOwner` table.

One new requirement fell out of this: `UpdateOrderOwnerInfoCommand` requires
`Status == Confirmed` (`OrderWriteService.UpdateOwnerInfoAsync`'s gate), unlike
`UpdateItems`' `Pending`-only window. Added `ConfirmOrderAsync` to
`OrderIntegrationTestBase` (calls `IOrderWriteService.ConfirmAsync` + `IUnitOfWork.SaveChangesAsync`
directly, the same saga-bypassing shortcut `CreateOrderAsync` already takes for creation)
so the test can move the order to `Confirmed` before firing the two concurrent requests.

## Result — verified live via Docker/Testcontainers

Ran `dotnet test --filter FullyQualifiedName~UpdateOrderRaceConditionTests` against a
real Postgres 17 container. All 3 iterations now show the **originally intended** steady
state — exactly one `200` and one `409 ConflictException` per iteration, zero corruption
findings:

```
Outcome distribution (Task A status, Task B status) -> count:
  (200, 409) -> 2
  (409, 200) -> 1
```

This empirically confirms the xmin-based optimistic concurrency protection
(`OrderConfig.cs`'s `.IsRowVersion()` + `EfUnitOfWork`'s `DbUpdateConcurrencyException` →
`ConflictException` translation) works correctly for this mutation path — the diagnostic
signal this suite was built to produce, previously blocked by Task 23's unrelated bug.

## What this does NOT resolve

Task 23 (`UpdateOrder`/item-replacement failing unconditionally) remains **open and
unfixed**. This suite no longer exercises that code path at all, so it provides zero
signal on whether Task 23's bug is fixed, present, or has regressed. If `UpdateOrder`
is ever fixed, it still has no dedicated concurrency test — that would need a separate
suite (or restoring the original item-replacement scenario here) once Task 23 lands.
