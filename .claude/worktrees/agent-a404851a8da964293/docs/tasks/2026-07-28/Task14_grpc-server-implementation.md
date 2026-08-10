# Task 14: Implement Server-Side GetUser/GetUsers (Cache-Backed)

**Status:** Done (2026-07-28)
**Category:** gRPC

## What was done

`GetUserByIdQuery`/`Handler` and `GetUsersByIdsQuery`/`Handler` added (`User.Application/Features/Users/Queries/GetUserById/`), both backed by Task 11's `CachedUserProfileReader` — the batch handler never falls back to a loop of single lookups (`CachedUserProfileReader.GetManyAsync` does exactly one `IUserProfileReadService.GetByIdsAsync` round trip for whatever wasn't already cached). `UserGrpcServiceImpl` gained `GetUser`/`GetUsers` overrides — the first read-oriented RPCs on this server, dispatching via `ISender` per `docs/reference/grpc.md`'s convention (unchanged from `CreateUserProfile`'s existing shape, just Query instead of an internal event). `GetUsers` iterates the *original* requested id list (not the result dictionary) to build the response, so a duplicate, unparseable, or nonexistent id in the request still gets exactly one `found=false` item back, never silently dropped.

## Objective

Implement the two new RPCs (Task 13) on `UserGrpcServiceImpl`, following the request's explicit flow: single lookup is Cache → gRPC-entry → DB → refresh cache → return; batch lookup reads all cache keys at once, determines misses, does exactly one DB round trip for the misses (never one query per missing id), refreshes cache, merges, and returns.

## Current state (grounded findings)

- `UserGrpcServiceImpl.cs:10-32` today implements exactly one method (`CreateUserProfile`), a thin adapter that builds an `OnUserInitiatedEvent` and dispatches it via `IInternalEventDispatcher` — **no query/`ISender`-dispatch pattern exists yet on this server**; this task's RPCs are the first read-oriented ones.
- No batch-by-ids method exists anywhere in the User read stack today — not on `IUserProfileReadService`, not on `IUserProfileRepository`/`UserProfileRepo`. Everything is single-id. Adding `GetByIdsAsync(IEnumerable<Guid>, ct)` to `IUserProfileReadService` (a straightforward `WHERE Id = ANY(...)` EF query) is new work, not an optimization of something existing.
- **The batch cache-read primitive already exists and is ready to use**: `ICacheService.GetManyAsync<T>`/`SetManyAsync<T>` (one Redis round trip for N keys). Its only current caller in the whole repo, `Auth.Infrastructure/Caching/RefreshTokenCacheService.cs:106-122` (`GetManyByTokenStringAsync`), is the closest template for "look up N keys in one round trip, then figure out which ones are missing" — even though its own source-of-truth isn't a gRPC call, the batch-cache-read shape is directly reusable.
- **The one existing "cache in front of a cross-service call" precedent is read-only/asymmetric**, and explicitly the wrong shape to copy fully: `User.Infrastructure/Caching/RoleCacheReader.cs` checks cache, and on miss calls Auth's gRPC `GetUserRoles` — but **does not write the result back to cache** (doc comment: "User service never writes to this key"). This task's flow is different and stronger: it must populate/refresh the cache on every miss, both single and batch — there is no existing fully-implemented example of that combined flow anywhere in this codebase; it's being built new here, using existing primitives (`ICacheService`, Task 11's `UserProfileCacheService`), not an existing end-to-end pattern.

## Scope

- `GetUser(GetUserRequest, ServerCallContext)`: dispatch a new `GetUserByIdQuery` (or reuse `GetUserQuery` if its shape already fits) via `ISender` — the query itself should go through Task 11's cache decorator (cache → `IUserProfileReadService.GetByIdAsync` on miss → cache refresh), so this RPC gets caching "for free" by depending on the already-cached read seam rather than re-implementing cache logic in the gRPC layer.
- `GetUsers(GetUsersRequest, ServerCallContext)`:
  1. Parse `user_ids` (strings → `Guid`s, skip/flag unparseable ones rather than throwing for the whole batch).
  2. `ICacheService.GetManyAsync<UserProfileCacheEntry>(keys)` — one round trip.
  3. Determine missing ids (cache miss or parse failure).
  4. **One** `IUserProfileReadService.GetByIdsAsync(missingIds, ct)` call (new repository/read-service method, per Task 11/above) — never a loop of single lookups.
  5. `ICacheService.SetManyAsync` to refresh the newly-fetched entries.
  6. Merge cached + freshly-fetched into the response, one `UserProfileItem` per originally-requested id, `found=false` for any that resolve to nothing in Postgres either (never throw for a partially-missing batch — return everything resolvable, per the request's explicit requirement).
- Both RPCs map `UserProfile`/cache entries to proto messages including `display_name` (Task 5's formatter, fixed-locale per Task 13's decision).

## Dependencies

- **Depends on:** Task 11 (cache decorator), Task 13 (proto contract), Task 2/5 (fields to return).
- **Blocks:** Task 15 (any consumer needs this server implementation to exist and work).

## Estimated complexity

Medium — the single-lookup path is simple (delegates entirely to Task 11's cache decorator); the batch path is the genuinely new piece (cache-many → determine-missing → one DB call → cache-many-refresh → merge), with no fully-worked precedent in this codebase to copy end-to-end, only the individual primitives (`GetManyAsync`/`SetManyAsync`, `RefreshTokenCacheService`'s partial precedent).

## Risks

- The most likely implementation bug is falling back to a loop-of-single-lookups for the batch path's cache misses "just to get it working" — this is exactly the N+1 anti-pattern the whole task is meant to prevent; guard against it explicitly in code review, not just in this planning doc.
- Cache-stampede on a hot miss (many concurrent `GetUsers` calls for the same missing id at once) isn't handled by anything existing in this codebase for a plain read-cache — the idempotency framework's `IDistributedLockProvider` is a general-purpose Redis lock that *could* be reused here, but doing so would be a novel application, not an existing convention; don't over-engineer this for v1 unless a real stampede risk is identified (likely low, given User lookups aren't as hot as, say, product stock checks).

## Completion checklist

- [ ] `IUserProfileReadService.GetByIdsAsync` added (single Postgres query, `WHERE Id = ANY(...)`)
- [ ] `GetUser` RPC implemented, delegates to the cached read seam (Task 11)
- [ ] `GetUsers` RPC implemented: batch cache read → determine misses → one batch DB call → batch cache refresh → merge, verified via a test that asserts exactly one DB call regardless of batch size/miss count
- [ ] Partial-not-found behavior verified: a batch with some invalid/nonexistent ids still returns all resolvable users, `found=false` for the rest, no exception
- [ ] Integration test: cold cache batch call populates cache; immediate repeat call hits cache only (no DB call)
