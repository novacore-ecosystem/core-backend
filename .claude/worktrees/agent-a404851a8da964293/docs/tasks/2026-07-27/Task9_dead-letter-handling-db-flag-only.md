# Task 9: Dead-letter handling is a DB status flag, not a real Kafka DLQ

**Status:** Resolved 2026-07-27 — periodic count-and-log monitor added (see below). A real
replay-from-topic DLQ was judged out of scope for this pass; see "What wasn't done."

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: Inventory reliability — verify "Dead Letter."

## Current state

`InboxMessageStatus.DeadLetter` marks a message dead in-table (part of the generic Inbox in `BuildingBlock.Persistence.Ef`). Per `InboxAttemptExecutor.cs:39-42`, dead-lettered messages are explicitly "not retried automatically" — there is no separate Kafka dead-letter topic, no automatic reprocessing path, and no operator-visible alert when a message lands in this state.

## Why this matters

A message that repeatedly fails today sits inert with no signal to anyone that it needs attention — it must be discovered by someone manually querying the Inbox table.

## Suggested acceptance criteria

- Dead-lettered messages are either replayable via a topic/admin action, or surfaced to an ops-visible dashboard/alert (even a simple periodic count-and-log is better than silence).

## What was done

Added `IInboxStore.GetDeadLetterSummaryAsync` (both the `BuildingBlock.Persistence.Inbox` and
`BuildingBlock.Application.Abstractions.Outbox` contracts), returning counts grouped by
`(ConsumerName, Topic)` with the oldest `DeadLetteredAt` timestamp per group. Implemented in
`EfInboxStore`/`MongoInboxStore` and wired through all 7 services' `Reliability/Inbox/InboxStore.cs`
adapters.

A new recurring job, `InboxDeadLetterMonitorJob` (`BuildingBlock.Infrastructure/BackgroundJobs/Monitoring/`),
runs every 15 minutes (configurable via `Jobs:InboxDeadLetterMonitor`, same options shape as
`InboxCleanupJob`/`OutboxCleanupJob`) and logs a `Warning` per dead-lettered group - consumer, topic,
count, and how long it's been stuck. It piggybacks on the existing `AddInboxOutboxCleanupJobs`
opt-in call already present in every service's DI, so no per-service wiring was needed beyond the
one shared registration point (`CleanupJobsExtensions.cs`).

## What wasn't done (as of 2026-07-27)

A real Kafka DLQ topic (publish-to-topic + admin replay tooling) was not built. The suggested
acceptance criteria explicitly treats "even a simple periodic count-and-log" as sufficient, and the
existing `InboxRetryHostedService`/manual-requery path already allows a dead-lettered row to be
un-stuck by hand once someone is alerted to it - a full replay pipeline would be new scope, not a
gap-closing fix, and wasn't pursued without confirming it's wanted.

## 2026-07-29 update: full Dead Letter Queue management APIs built

The replay pipeline above was confirmed wanted and implemented across all 7 services (Audit,
Auth, Inventory, Notification, Order, Product, User).

**Note on the earlier "manual-requery path already allows..." claim**: that turned out to be
inaccurate — `InboxRetryHostedService` only polls `Status == Retrying`, structurally excluding
`DeadLetter` rows; there was no code path anywhere that retried a dead-lettered message before
this change.

**What was added:**
- `IInboxStore.RequeueDeadLetterAsync`/`GetRetryHistoryAsync`/`RevertFailedRequeueAsync` (both
  layers, Ef+Mongo impls, all 7 per-service adapters) — atomic `DeadLetter -> Retrying` conditional
  update (`WHERE Status = DeadLetter`), so concurrent retry calls for the same row can never both
  win, no locking required for correctness.
- `InboxRetryHistory` (Ef) / `InboxRetryHistoryDocument` (Mongo) — new append-only table/collection
  per service, one row per manual retry attempt (`StartedAt`/`FinishedAt`/`Duration`/`Operator`/
  `RetryNumber`/`Result`/`Exception`), closed out automatically by the existing
  `CompleteAttemptAsync`/`FailAttemptAsync` chokepoint when the redelivered message is next
  processed.
- `IDeadLetterQueryService` (Ef+Mongo) — paged search (via the existing `BuildingBlock.Criteria`
  pipeline on the Ef side) and full detail-with-history, always scoped to `Status == DeadLetter`.
- `IDeadLetterRetryService` (`BuildingBlock.Infrastructure/DeadLetters`) — the single retry
  implementation shared by single/bulk/retry-all commands: acquire an optional per-row distributed
  lock (skipped gracefully on the 3 services with no Redis - see below), requeue, republish via
  `IOutboxPublisher.PublishOutboxMessageAsync` onto the real topic (never re-invokes the consumer
  handler directly, unlike `InboxRetryHostedService`), revert-on-publish-failure.
- Generic Carter API, mounted identically on all 7 services: `POST /deadletters/search`,
  `GET /deadletters/{id}`, `POST /deadletters/{id}/retry`, `POST /deadletters/retry` (bulk),
  `POST /deadletters/retry-all` (capped 500/call).

**Gap found and fixed along the way**: only User/Order/Product call `AddIdempotency()`
(`IDistributedLockProvider`); Auth has Redis but not idempotency wired, and Audit/Inventory/
Notification have no Redis at all. `IDistributedLockProvider` is now resolved optionally
(`GetService`, not `GetRequiredService`) — the DB-level atomic conditional update is sufficient
for correctness on its own, the lock is defense-in-depth only.

**Deferred**: `Cancelled` retry-history status is modeled but no cancel-in-flight endpoint exists;
scheduled/background retry job (architecture supports it, nothing scheduled yet); full
Testcontainers-based E2E suite (script-based smoke test added instead, see
`docs/testing/deadletter-retry-e2e.md` — not run against a live stack this session).
