# Reference: Caching

**Scope:** `ICacheService`/Redis usage, the role-caching decorator pattern, and the Gateway's separate minimal Redis path. Supersedes/merges the old `CACHING.md` + `ROLE_CACHING.md` (archived, see [08-migration-plan.md](../08-migration-plan.md)).

## Standard path: `ICacheService`

`BuildingBlock.Application.Abstractions.Services.ICacheService` — generic Get/GetMany/Set/SetMany/Remove/RemoveMany/RemoveByPattern/Exists, all `<T>`, all `async`. Implementation: `BuildingBlock.Infrastructure.Caching.RedisCacheService` (JSON-serializes via `BuildingBlock.SharedKernel.Serialization.JsonSerializerConfiguration.Default`). Registered via `services.AddRedisCache(configuration)` (binds `"Cache"` config section → `CacheOptions`: `ConnectionString`, `DefaultExpirationMinutes`, `EnableCompression`, `KeyPrefix`, `ConnectionTimeout`).

**Note:** `CacheOptions.KeyPrefix` is bound from config but not actually applied anywhere in `RedisCacheService` — keys are exactly what you pass in, no automatic prefixing happens despite the option existing. Don't rely on it; build fully-qualified keys yourself via `BuildingBlock.SharedKernel.Constants.CacheKeys`.

### Key convention

All cache keys are centralized in `BuildingBlock.SharedKernel/Constants/CacheKeys.cs` as static builder methods, one nested class per cached entity type (`CacheKeys.Roles.UserRoles(userId)`, `CacheKeys.RefreshTokens.ByTokenString(token)`, etc.). **Add new key builders here, don't inline key strings in a service.** This is what lets a cross-cutting consumer (like the Gateway, below) share the exact key format with the service that owns the write side.

## No decorators for cache invalidation/refresh (mandatory, project-wide)

**New work must not wrap `IXxxReadService`/`IXxxWriteService` in a cache decorator** (a class implementing the same Persistence-facing interface, swapped in via DI so the interception is invisible to Application). Reasons, in order of severity:

1. **Wrong invalidation timing.** A Persistence call is often just one step of a larger workflow (e.g. `UpdateUserHandler`: `UpdateProfileDetailsAsync` → enqueue outbox event → commit transaction). A decorator that invalidates immediately after the Persistence call it wraps has no way to know whether the *surrounding* transaction later commits or rolls back — it fires mid-transaction, before the write is durable. A concurrent reader can then repopulate the cache from pre-commit (or, on rollback, permanently stale) data.
2. **Hidden side effects.** Reading a command handler top to bottom no longer tells you the whole story — cache invalidation happens invisibly one layer down, in Infrastructure.

Instead: cache services expose plain, explicitly-named methods (`GetByIdAsync`, `InvalidateAsync`, ...) that Application calls directly and visibly, at the point in the workflow where it actually makes sense (after a transaction commits, after a delete completes). See "User Detail cache" below for the canonical example. `Auth.Infrastructure/Caching/CachedAuthServiceDecorator.cs` (roles) predates this policy and still uses the decorator shape — it's called out here as legacy, not as a pattern to copy for new caches.

## Cross-service read-only consumer

A service can read a cache another service owns and writes, as long as it uses the same `CacheKeys` builder. Example: `User.Infrastructure/Caching/Roles/RoleCacheReader.cs` (`IRoleCacheReader`) reads `CacheKeys.Roles.UserRoles(userId)` — the same key Auth's `RoleCacheService` writes — without User owning any write path to it. If you add a reader like this, **never** write to a key another service owns; only the owning service's write path should mutate it.

## User Detail cache (explicit orchestration, canonical example)

Added 2026-07-28 (`docs/tasks/2026-07-28/Task11_user-detail-cache-scaffold.md`); redesigned twice on 2026-08-04 - first to move caching out of Application (a raw `cache.Get/Set/Remove`-calling `CachedUserProfileReader` lived in `User.Application`), then again to drop the decorator that first fix introduced, once it became clear the write-side decorator invalidated mid-transaction (see "No decorators..." above). This is the canonical example of the current, mandatory pattern: an Infrastructure cache service with its own explicit interface, called directly and visibly by Application at the correct point in each workflow.

**Key group:** `CacheKeys.UserProfiles` (`user:users:detail:{userId}`, 10-minute default TTL) — a new, correctly-namespaced group, *not* the pre-existing `CacheKeys.Users` (`auth:users:*`), which was dead scaffolding seeded for Auth's own account concept and never wired to anything. User and Auth share one physical Redis instance, so the distinct prefix isn't just style — it avoids a real key collision risk.

`UserReadModel` (`User.Application/Features/Users/DTOs/UserReadModel.cs`) is already a flat, JSON-serializable read model, so the cache targets it directly — no separate `CachedUserProfile` DTO needed. Files live under `User.Infrastructure/Caching/Users/` (cache implementations are grouped by business capability/aggregate — `Caching/Roles/` for `RoleCacheReader`, `Caching/Users/` here — not one flat `Caching/` folder):

- `IUserProfileDetailCache` (`User.Application/Abstractions/Services/IUserProfileDetailCache.cs`) — the Application-facing contract: `GetByIdAsync`, `GetByIdsAsync`, `InvalidateAsync`. A distinct interface from `IUserReadService`/`IUserWriteService`, not an implementation of either - Application chooses to call it explicitly wherever it wants the cached path.
- `UserProfileDetailCache : IUserProfileDetailCache` (`User.Infrastructure/Caching/Users/UserProfileDetailCache.cs`) — owns the entire lifecycle itself: `GetByIdAsync`/`GetByIdsAsync` check Redis, on miss call `IUserReadService` (Persistence abstraction) and refresh Redis, then return (batch does exactly one inner DB round trip for whatever wasn't already cached, never a loop of single lookups); `InvalidateAsync` just removes the key. Config-driven TTL via `Caching:EntityTtl:UserProfiles:MinutesToExpire`, fallback `CacheKeys.UserProfiles.DefaultTtlMinutes`.
- Consumers call it directly: `GetUserByIdHandler`/`GetUserDetailHandler`/`GetUsersByIdsHandler` inject `IUserProfileDetailCache` and call `GetByIdAsync`/`GetByIdsAsync` in place of `IUserReadService`. `UpdateUserHandler` calls `userProfileCache.InvalidateAsync(...)` as the last line of `Handle`, *after* `unitOfWork.ExecuteTransactionAsync` returns - not inside the transaction delegate. `OnUserDeletionHandler` (the real deletion path — `DeleteUserCommand`/`DeleteUserHandler` are dead code, unreferenced anywhere in the repo) calls it right after `DeleteWithNoTrackingAsync` succeeds. Both invalidation calls are plainly visible in the handler body.

**Registration is a plain `services.AddScoped<IUserProfileDetailCache, UserProfileDetailCache>()`** in `User.Infrastructure/DependencyInjection.cs` - no Scrutor, no decoration, no ordering dependency on `User.Persistence`'s registrations. `UserProfileDetailCache` just takes `IUserReadService` as a constructor dependency like any other Infrastructure class collaborating with Persistence.

## Gateway minimal lookup vs `ICacheService`

The API Gateway does **not** use `ICacheService`/`CacheOptions` at all. `BuildingBlock.Web/RefreshTokens/RefreshTokenCacheExtensions.cs` provides a standalone `AddRefreshTokenCache(connectionString)` (raw `IConnectionMultiplexer`, no serialization/options layer) + a single `RefreshTokenExistsAsync` extension method doing one `EXISTS` check via `CacheKeys.RefreshTokens.ByTokenString(token)`. This is deliberate — see [services/gateway.md](../services/gateway.md#refresh-token-filtering) and [decisions/buildingblock-web-extraction.md](../decisions/buildingblock-web-extraction.md). **Do not "fix" this by switching the Gateway to `ICacheService`** — that would pull `BuildingBlock.Infrastructure`'s full package set into the Gateway for a single `EXISTS` check, which is the opposite of what this design intentionally avoids.

## When to add caching

Only after measuring a real read-heavy, infrequently-changing path — see [workflows/performance-optimization.md](../workflows/performance-optimization.md). Don't cache write-heavy or per-request-unique data.
