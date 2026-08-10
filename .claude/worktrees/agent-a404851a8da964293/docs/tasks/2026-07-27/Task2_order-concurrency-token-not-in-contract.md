# Task 2: Order concurrency token exists in the DB but never round-trips through the API contract

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: "The system MUST correctly handle the scenario where two users edit the same order simultaneously" — verify optimistic concurrency / row version / xmin / conflict handling.

## Current state

A genuine Postgres `xmin`-based concurrency token is configured on Order: `OrderConfig.cs:27-31` (`.HasColumnType("xid").IsRowVersion()`). `EfUnitOfWork.cs:70-71` catches `DbUpdateConcurrencyException` and rethrows it as a `ConflictException` (→ 409), verified by `EfUnitOfWorkTests.cs:39-47`.

However:
- No command/DTO carries a client-supplied version token: `UpdateOrderCommand.cs:5-7`, `CancelOrder.cs`'s request, and `UpdateOrderOwnerInfoRequest` all lack any `RowVersion`/`xmin` field.
- `GetOrderResponse` never returns a version value for the client to echo back on a later write.
- `OrderRepo.UpdateAsync` (`OrderRepo.cs:42-51`) fetches the entity fresh from the DB immediately before mutating it, all within the same request/transaction — so the xmin check only guards against two requests landing within the same instant of each other. It cannot detect "user A loaded this order 10 minutes ago in their browser and is now saving over user B's edit," which is the actual scenario named in the requirement.

## Why this matters

This is the specific business scenario the requirement calls out, and it is not actually protected today. A real concurrent-edit race (the realistic case — two admins editing the same order minutes apart) will silently last-write-wins instead of surfacing a conflict.

## Open questions

- Should the version token be the raw `xmin` value (opaque, DB-specific) or a dedicated `RowVersion`/`ETag`-style field? `xmin` is simplest given it's already the mechanism in use.
- Does `UpdateOrderOwnerInfo` need the same protection as full `UpdateOrder` (item changes), or is owner-info edit considered lower-risk?

## Suggested acceptance criteria

- `GetOrder` response includes a version/rowversion value.
- `UpdateOrder`/`CancelOrder`/`UpdateOrderOwnerInfo` requests require that value; the update is rejected with 409 if the current DB row's version doesn't match what the client sent (not just concurrent-instant races).
- Manual test: open the same order in two sessions, save in session A, then save in session B using the version loaded before A's save → session B gets 409.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task4_order-concurrency-conflict-ui-missing.md` (blocked on this task).
