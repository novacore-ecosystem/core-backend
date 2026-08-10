# Task 17: Testing (Unit + Integration, Threaded Through All Phases)

**Status:** Not started (planning only)
**Category:** Testing

## Objective

Cover the new behavior with tests as each phase lands (per `docs/testing/` conventions — NSubstitute + Shouldly, phased Domain-before-Application roadmap), not as a single end-of-epic pass. This file enumerates what needs coverage; each concrete task above should close its own tests as part of "done," not defer them here.

## Current state (grounded findings)

- Existing test conventions (per prior project memory, cross-checked against this session's findings): `docs/testing/*` defines the phased approach, NSubstitute for fakes (not hand-rolled), Shouldly for assertions, `Directory.Build.props`/`Packages.props` for shared test infra.
- No existing tests reference `MiddleName`, `DisplayName`, `SearchName`, `ICurrentLocaleService`, or any User-specific ES/cache/gRPC-batch behavior (all genuinely new surface area, confirmed by the absence of any of these terms anywhere in the User-service research).
- A recent, directly relevant precedent for *how* this repo writes integration tests for exactly this kind of race/consistency-sensitive feature: `tests/integration/Order.IntegrationTests/Concurrency/UpdateOrderRaceConditionTests.cs` (per `docs/tasks/2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md`) — a real, recent example of testing eventual-consistency/concurrency behavior in this codebase; the ES sync path (Task 8) and the cache/gRPC batch path (Tasks 11-14) both have similar "eventually consistent" or "must not race" properties worth testing the same way.

## Scope

**Unit tests** (fast, no external dependencies):
- `UserProfile` domain: `MiddleName` defaults to empty when omitted from `Create`/`UpdateProfile` (Task 1).
- `IUserDisplayNameFormatter`: en-US ordering, vi-VN ordering, empty `MiddleName` (no double space), unknown locale falls back sensibly (Task 5).
- `SearchName` generation: word-order variants, accent variants, case variants, whitespace variants all normalize to matching tokens (Task 7) — this is the highest-value test suite in the whole epic, since it's the one area with no existing precedent to lean on.
- `ICurrentLocaleService`: header present/absent/malformed → correct locale/default (Task 4).

**Integration tests** (real Postgres/Redis/Elasticsearch/Kafka, per this repo's existing integration-test infrastructure):
- Create/Update User with `MiddleName` end-to-end through the REST API (Task 2/3).
- Auth Register → gRPC `CreateUserProfile` → `UserProfileCreatedIntegrationEvent` → Notification consumer, `MiddleName` intact through the whole chain (Task 3).
- ES sync eventual consistency: Create → poll index → document appears; Update → poll → reflects change; Delete (via the real `OnUserDeletionHandler` path, not the dead `DeleteUserHandler`) → poll → document gone (Task 8).
- `SearchUsers` (ES-backed) parity check against every filter/sort the old Postgres endpoint supported, before cutover (Task 10).
- Cache: Update/Delete invalidate the User Detail cache; subsequent read repopulates correctly (Task 12).
- gRPC single `GetUser`: cache miss → DB → cache populated → repeat call hits cache only (Task 14).
- gRPC batch `GetUsers`: mixed valid/invalid/nonexistent ids in one batch → all resolvable users returned, `found=false` for the rest, **exactly one** DB query regardless of batch size (assert query count, not just correctness — this is the test that actually proves the N+1 anti-pattern was avoided) (Task 14).
- Locale header → `DisplayName` variation: same user, two requests with different `Accept-Language` values, two different formatted names back (Task 4/5).

## Dependencies

- **Depends on:** each respective task's implementation (tests land alongside, not after).
- **Blocks:** nothing — but no task above should be marked "done" in `PROGRESS.md` without its corresponding tests from this list.

## Estimated complexity

Medium overall (spread across every other task) — no single large lift, but real discipline required not to defer all of it to the end.

## Risks

- The biggest risk with a "testing" task listed separately from implementation tasks is that it becomes a checkbox nobody actually does — each implementation task's own completion checklist already includes its relevant tests for exactly this reason; treat this file as the index/summary, not a substitute for closing tests within each task.
- The gRPC batch "exactly one DB query" assertion is easy to skip if only checking output correctness — a loop-of-single-queries can produce the *same correct output* while completely failing the actual performance goal; make the query-count assertion a required, not optional, part of that test.

## Completion checklist

- [ ] Unit test suite for `SearchName` normalization (word order, accent, case, whitespace) — highest priority, no existing precedent to lean on
- [ ] Unit test suite for `DisplayName` formatter (both locales + edge cases)
- [ ] Integration test: full Register→gRPC→event→Notification chain with `MiddleName`
- [ ] Integration test: ES sync eventual consistency (create/update/delete)
- [ ] Integration test: cache invalidation on update/delete
- [ ] Integration test: gRPC batch with query-count assertion proving no N+1
- [ ] Integration test: locale header drives different `DisplayName` output for the same user
