# Task 14: Order search has no Order ID field at all

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`OrderCriteriaDefinition` (`src/Services/Order/Order.Application/Features/Orders/Search/OrderCriteriaDefinition.cs:9-14`) whitelists exactly `Owner.CustomerName`, `phone`, `Status`, `CreatedAt` — there is no `Id`/`orderId` field. The API's own doc string confirms this (`src/Services/Order/Order.API/Endpoints/SearchOrders.cs:19`: `"allowed fields: customerName, phone, status, createdAt"`). Exact-ID lookup exists only via the separate `GetOrder` endpoint (`OrderReadService.GetByIdAsync`), which requires already knowing the order exists and its precise ID — it's a fetch, not a search, and returns 404 rather than an empty result set for a not-found ID.

## Why this matters

Checklist requirement: Order ID should be a searchable keyword field with exact match. Support staff searching a partial order list by ID today have no way to do it through `SearchOrders` — they'd have to guess/know the full ID upfront.

## Suggested acceptance criteria

- Add `Id`/`OrderId` to `OrderCriteriaDefinition` as an exact-match-only field (`Eq`/`In`, no `Contains`/`StartsWith`/`EndsWith` — IDs are GUIDs, partial matching isn't meaningful).
- Confirm whether the checklist's "Order ID: Exact match only" implies it should also be OR'd into the free-text keyword search, or only usable as an explicit field filter — recommend explicit-filter-only, since mixing a GUID into a `Contains`-based keyword OR against name/phone fields is not useful and could regress the keyword box's performance (GUID columns aren't typically full-text indexed the same way).
- Update `SearchOrders.cs`'s doc string once implemented.

## What was done

Added `.Field(x => x.Id).Guid()` to `OrderCriteriaDefinition` (logical name `id`, defaulted from the member name). Left it at `Guid`'s default operator set (`Eq`/`Ne`/`In`/`NotIn`) rather than narrowing further — that set is already exact-match-only, so there was nothing to restrict. Not added to the free-text keyword OR (keyword search stays customerName-only, consistent with a GUID not being a useful `Contains` target). Updated `SearchOrders.cs`'s doc string to list `id` among allowed filter fields. Scoped build of `Order.API` passes.

## What wasn't done

Nothing deferred — this was a single-field addition using existing infrastructure.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task13_order-id-search-filter-missing.md`.
