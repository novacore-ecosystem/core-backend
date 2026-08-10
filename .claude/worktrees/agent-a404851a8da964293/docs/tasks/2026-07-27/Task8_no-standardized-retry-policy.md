# Task 8: No standardized retry policy (Polly) for transient Kafka/gRPC failures

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: Inventory reliability — verify "Retry."

## Current state

Zero Polly references anywhere in the solution. Resilience today is bespoke and inconsistent:
- The generic Inbox has DB-row-based retry/backoff (`InboxRetryHostedService`/`InboxRetryOptions`).
- `DeductStockHandler.cs:47-61`/`RestockStockHandler.cs:49-100` have a hand-rolled `MaxConcurrencyRetries = 3` loop specifically for xmin optimistic-concurrency conflicts.
- No retry exists for transient Kafka producer/consumer failures or gRPC call failures outside of these two mechanisms.

## Why this matters

Not a correctness bug today (the Inbox provides durability for consumer-side failures), but a maintainability/consistency gap — every service that wants retry behavior has to hand-roll it, and there is no consistent backoff/circuit-breaker strategy for gRPC calls (e.g. Order→Inventory) that could transiently fail.

## Suggested acceptance criteria

- A standard retry policy (Polly or equivalent) applied consistently to gRPC clients and any Kafka producer/consumer paths that don't already have Inbox-based durability.
