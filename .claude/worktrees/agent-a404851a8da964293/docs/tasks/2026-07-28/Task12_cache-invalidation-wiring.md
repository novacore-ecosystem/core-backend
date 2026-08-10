# Task 12: Wire Cache Invalidation into Create/Update/Delete

**Status:** Done (2026-07-28)
**Category:** Cache

## What was done

`UpdateUserHandler` invalidates (`IUserProfileCacheService.RemoveAsync`) after its transaction commits, not inside it — the cache isn't transactional storage, so there's nothing to roll back there if the transaction fails (it simply never runs). `OnUserDeletionHandler` — confirmed, again, to be the real deletion path (`UserAccountDeletionIntegrationEventConsumer` → `OnUserDeletionEvent` → this handler; `DeleteUserCommand`/`DeleteUserHandler` still have zero callers anywhere in the repo, re-verified via grep before wiring) — invalidates after `DeleteWithNoTrackingAsync`. `CreateUserHandler` was deliberately left without proactive cache population (the "optional, not required" item from the plan) — the first `GetUserDetail`/gRPC lookup after creation is a normal, expected cache miss.

## Objective

Ensure the User Detail cache (Task 11) never serves stale data after a Create/Update/Delete — invalidate immediately on every write path, including the real (non-dead) deletion flow.

## Current state (grounded findings)

- **None of User's three write handlers touch any cache today** — confirmed by direct reads of `CreateUserHandler.cs`, `UpdateUserHandler.cs`, `DeleteUserHandler.cs`: zero `ICacheService` references anywhere. This is greenfield, not a refactor of existing (possibly-wrong) invalidation logic.
- **`DeleteUserHandler`/`DeleteUserCommand` are dead code** — confirmed by repo-wide grep, nothing anywhere constructs/sends a `DeleteUserCommand`. **The real deletion path** is `UserAccountDeletionIntegrationEventConsumer` → `OnUserDeletionEvent` → `OnUserDeletionHandler.cs:8-14` → `IUserProfileWriteService.DeleteWithNoTrackingAsync` (a bulk `ExecuteDeleteAsync`, bypassing the change tracker entirely). **Cache invalidation on delete must be wired into `OnUserDeletionHandler`, not the unused `DeleteUserHandler`** — wiring the wrong one would leave deleted users' details cached indefinitely with no code path ever expiring them early (they'd still expire via TTL, but that's a much weaker guarantee than the request's "invalidate immediately" requirement).
- Auth's `CachedAuthServiceDecorator` (`Auth.Infrastructure/Caching/CachedAuthServiceDecorator.cs:78-93`) is the precedent for *where* invalidation calls live: inside the decorator's write-path methods (`DeleteUserAsync`, `AssignRoleAsync`), not inside the MediatR command handler itself. This repo has never actually combined "Redis cache + invalidation call inside a `Commands/*Handler.cs` file" before (confirmed by the cache-infra agent's grep) — the closest real analog is Notification's in-memory `NotificationChannelCache`, invalidated explicitly from `UpdateNotificationChannelConfigurationHandler`/`DisableNotificationChannelHandler`/`EnableNotificationChannelHandler`. **Decide which shape to follow**: invalidate inside the decorator's own write methods (if the decorator wraps a write-capable interface) or invalidate explicitly inside the three handlers (Create/Update/OnUserDeletion) — recommend the decorator-owns-invalidation shape for consistency with Auth's precedent, provided the decorator wraps something with visibility into writes; if the write service interface the decorator wraps is read-only (`IUserProfileReadService`), invalidation instead belongs in the handlers themselves, closer to Notification's shape.

## Scope

- `CreateUserHandler`: on success, no cache entry exists yet for a brand-new user, so nothing to invalidate — optionally, populate the cache proactively (write-through) rather than waiting for the first read to miss, since a Create is immediately followed by a likely `GetUserDetail`/gRPC lookup in practice — a reasonable optimization, not required.
- `UpdateUserHandler`: after `UpdateProfileDetailsAsync` succeeds, call `UserProfileCacheService.RemoveAsync(userId, ct)` (or the decorator's equivalent).
- `OnUserDeletionHandler` (the real delete path — **not** `DeleteUserHandler`): after `DeleteWithNoTrackingAsync` succeeds, call the same invalidation.
- If `DeleteUserHandler`/`DeleteUserCommand` truly have no callers anywhere (confirm once more at implementation time, in case something changed since this research), flag to the team whether to delete the dead code outright (per this repo's stated "remove unused files during refactoring" convention) or leave it — **not** this task's call to make unilaterally, but worth surfacing since this task is touching deletion semantics anyway.

## Dependencies

- **Depends on:** Task 11 (cache service/decorator must exist).
- **Blocks:** nothing downstream directly, but should land before/alongside Task 14 (gRPC reads through the same cache — stale-data risk is worse once more consumers depend on it).

## Estimated complexity

Small — a handful of one-line invalidation calls, once Task 11's decision (decorator-owned vs. handler-owned invalidation) is made.

## Risks

- The single biggest risk is wiring invalidation into the dead `DeleteUserHandler` instead of the real `OnUserDeletionHandler` path — this would look correct in a code review (right method name, right idea) while doing nothing in production. Verify against the actual Kafka-consumer-driven deletion flow, not the unused command.
- If Create doesn't populate the cache proactively, the very first read after a Create is guaranteed to be a cache miss anyway (correct, just not optimal) — not a correctness risk, just a minor efficiency note.

## Completion checklist

- [ ] `UpdateUserHandler` invalidates the User Detail cache on success
- [ ] `OnUserDeletionHandler` (the real delete path, verified against production Kafka flow, not `DeleteUserHandler`) invalidates the cache on success
- [ ] Decision recorded: decorator-owned vs. handler-owned invalidation, and why
- [ ] Integration test: Update → cache entry gone → next read repopulates with new data; Delete (via the real event-driven path) → cache entry gone
- [ ] Explicit note recorded on `DeleteUserCommand`/`DeleteUserHandler`'s dead-code status for the team to action separately
