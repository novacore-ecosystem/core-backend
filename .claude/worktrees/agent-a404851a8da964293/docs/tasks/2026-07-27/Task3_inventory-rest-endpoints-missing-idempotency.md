# Task 3: Inventory's REST stock-mutation endpoints have no idempotency protection

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: inventory design "must guarantee that inventory events are NOT lost" — verify Outbox / Inbox / Idempotency / Retry / Dead Letter / exactly-once (or equivalent).

## Current state

The Redis-backed Idempotency + Distributed Lock framework (opt-in via `AddIdempotency`/`UseIdempotency`/`.RequireIdempotency()`) is wired to `User/CreateUser.cs:62`, `Product/CreateProduct.cs:65`, `Order/CreateOrder.cs:58`, and `Order/UpdateOrderOwnerInfo.cs:43`.

`Inventory.API/Endpoints/AdjustStock.cs`, `StockIn.cs`, `StockOut.cs` have **zero** idempotency middleware — confirmed by grep returning nothing for `AddIdempotency`/`UseIdempotency` in the Inventory tree. Structurally, `inventory-api` has no Redis dependency in `docker-compose.yml` at all (unlike auth/user/order), so the framework can't be applied there without also wiring Redis into the container first.

Separately, the gRPC `DeductStock`/`RestockStock` pair (used by the Order create-order saga) **does** have its own bespoke ledger-based idempotency via a `StockDeduction` table keyed by the caller's `DeductionId` (`StockDeduction.cs:8-16`) — but this only covers the saga path, not the manual/admin REST endpoints.

## Why this matters

A retried `AdjustStock`/`StockIn`/`StockOut` call today (e.g. an admin's client retrying after a network timeout) can double-apply a stock change. This directly contradicts the "inventory events must not be lost" requirement's implicit twin: they also must not be silently duplicated.

## Suggested acceptance criteria

- `inventory-api` gets a Redis connection in `docker-compose.yml`/`docker-compose.override.yml`.
- `AdjustStock`, `StockIn`, `StockOut` endpoints require an idempotency key and use `.RequireIdempotency()`, matching the existing pattern on `CreateUser`/`CreateOrder`/`CreateProduct`.
- Replaying the same idempotency-key request twice against any of the three endpoints results in exactly one stock mutation.
