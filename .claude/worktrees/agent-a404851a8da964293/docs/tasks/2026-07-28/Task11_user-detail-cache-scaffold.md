# Task 11: User Detail Cache — CacheKeys + Decorator Scaffold

**Status:** Done (2026-07-28)
**Category:** Cache

## What was done

Added a new, correctly-namespaced `CacheKeys.UserProfiles` group (`user:users:detail:{userId}`, 10-minute default TTL) — the dead, wrongly-namespaced `CacheKeys.Users` (`auth:users:*`) was kept in place (not deleted, to avoid unrelated churn) but annotated as dead scaffold, pointing readers to `UserProfiles` instead.

**Deviation from the literal "decorator wrapping `IUserProfileReadService`" plan, for a concrete technical reason**: `UserProfile`'s properties all have `private set` (domain encapsulation) and no public constructor, so a JSON-deserialized cache entry cannot be reconstructed as a real `UserProfile` instance from outside `User.Domain` — the same reasoning that gives Elasticsearch its own read-model document instead of indexing the aggregate directly. Built a parallel, purpose-built `CachedUserProfile` DTO (`User.Application.Abstractions.Services`) instead, with `IUserProfileCacheService` (Redis-backed, `User.Infrastructure/Caching/UserProfileCacheService.cs`, mirrors `RoleCacheService`'s shape exactly) and `CachedUserProfileReader` (`User.Application/Features/Users/Caching/`) providing the actual read-through orchestration (cache → `IUserProfileReadService.GetByIdAsync`/`GetByIdsAsync` on miss → refresh cache → return), both single and batch. `IUserProfileReadService.GetByIdsAsync` was added as a new batch method (needed by both this task and Task 14). Wired into `GetUserDetailHandler` in place of the direct `IUserProfileReadService.GetByIdAsync` call; `IRoleCacheReader` (Auth's role cache) is unchanged, a deliberately separate cache with a separate owner.

## Objective

Introduce a read-through cache in front of User Detail retrieval (cache → DB, short TTL), reusing the existing `ICacheService`/Redis infrastructure and the exact decorator pattern already proven by Auth's role cache — not a new caching mechanism.

## Current state (grounded findings)

- **Standard path already exists and should be reused as-is**: `ICacheService` (`BuildingBlock.Application/Abstractions/Services/ICacheService.cs`, 59 lines) — generic `GetAsync<T>`/`GetManyAsync<T>`/`SetAsync<T>`/`SetManyAsync<T>`/`RemoveAsync`/`RemoveManyAsync`/`RemoveByPatternAsync`/`ExistsAsync`. Implementation `RedisCacheService` (`BuildingBlock.Infrastructure/Caching/RedisCacheService.cs`), registered via `services.AddRedisCache(configuration)` (binds `"Cache"` section). **`CacheOptions.KeyPrefix` is bound but never actually applied** — don't rely on it; build fully-qualified keys via `CacheKeys` yourself, per `docs/reference/caching.md:9`.
- **The canonical decorator pattern to copy** (per `docs/reference/caching.md:15-37`, and direct reads of both files): `Auth.Infrastructure/Caching/RoleCacheService.cs` (get/set/remove wrapper around `CacheKeys.Roles.UserRoles(userId)`, TTL from `Caching:EntityTtl:Roles:MinutesToExpire` config with a `CacheKeys.Roles.DefaultTtlMinutes` fallback) + `CachedAuthServiceDecorator.cs` (wraps `IAuthService`, cache-then-populate on `GetUserRolesAsync`, invalidates on `DeleteUserAsync`/`AssignRoleAsync`). Registered **manually, not via Scrutor** — the decorator needs the concrete inner implementation resolved explicitly:
  ```csharp
  services.AddScoped<RoleCacheService>();
  services.AddScoped<IAuthService>(sp => {
      var inner = sp.GetRequiredService<Auth.Persistence.Services.AuthService>();
      var cache = sp.GetRequiredService<RoleCacheService>();
      return new CachedAuthServiceDecorator(inner, cache);
  });
  ```
- **What is explicitly NOT a template to copy**: `User.Infrastructure/Caching/RoleCacheReader.cs` — this is a read-only, no-write cross-service *borrower* of a cache another service (Auth) owns (doc comment: "User service never writes to this key — Auth owns population/invalidation"). User's new cache is the opposite shape: User **owns** both the write and read side of its own `UserProfile` cache, so Auth's own decorator (owning service caching its own data) is the right template, not `RoleCacheReader` (cross-service read-only borrowing).
- **`CacheKeys.cs` already has a dead, wrongly-named placeholder for this**: `CacheKeys.Users` (`BuildingBlock.SharedKernel/Constants/CacheKeys.cs:27-43`) — `Profile(Guid userId) => "auth:users:profile:{userId}"`, `Email(...)`, `DefaultTtlMinutes = 60`, explicitly commented "(for future extension)". **Confirmed via repo-wide grep: zero references anywhere** — completely unused. The `"auth:users"` prefix strongly suggests it was scaffolded with Auth's own account concept in mind, not User service's `UserProfile` aggregate. **Do not silently repurpose this key group as-is** — either rename it to something namespaced correctly (e.g. a new `CacheKeys.UserProfiles` group under a `user:` prefix) or explicitly redefine `CacheKeys.Users` and document that the prefix, despite its name, is now User-service-owned. Flag this decision to the team; don't let a future reader assume `"auth:users:*"` is already wired up when it isn't.
- **Important infrastructure note**: User and Auth are configured to share the **same physical Redis instance** (`docker-compose.override.yml:124-125` comment, confirmed by the cache-infra agent: "shared instance with Auth; role cache keys are read under the `auth:roles:` namespace regardless of `KeyPrefix` here") — so key-name collisions across services are a real, not theoretical, risk. This is exactly why the `auth:users:*` prefix must not be reused for User-owned data without renaming.
- No Redis-backed, `CacheKeys`-driven, handler-triggered cache-invalidation pattern exists anywhere in the repo today for any entity (`CacheKeys.Products`/`Categories` are equally dead/unused placeholders) — this will be the first real instance of that combination. The closest structural (but different-technology) analog is Notification's `IMemoryCache`-based `NotificationChannelCache`, invalidated explicitly from Update/Disable/Enable handlers.

## Scope

- Add a new, correctly-namespaced key group to `CacheKeys.cs` (recommend `CacheKeys.UserProfiles`, prefix `user:users:detail:{userId}` — or resolve the existing `CacheKeys.Users` naming ambiguity explicitly, per the decision flagged above) with a short TTL (`DefaultTtlMinutes`, e.g. 5-15 — "short" per the request, tune against real read/write ratio).
- `User.Infrastructure/Caching/UserProfileCacheService.cs` (or similarly named) — `GetAsync`/`SetAsync`/`RemoveAsync`, mirroring `RoleCacheService`'s exact shape (config-driven TTL with a constant fallback).
- A decorator wrapping whatever is the actual read seam for "User Detail" — likely `IUserProfileReadService.GetByIdAsync`, since that's what both `GetUserDetailHandler` and (later) Task 14's gRPC handler need cached. Register manually (not Scrutor), same pattern as Auth's `IAuthService` wrapping.
- This task is scaffolding only (cache service + decorator shell) — Task 12 wires the actual invalidation calls into Create/Update/Delete.

## Dependencies

- **Depends on:** nothing structural (independent of the name-model/ES work) — can run in parallel with Phases A/B/D per the architecture doc.
- **Blocks:** Task 12 (invalidation wiring), Task 14 (gRPC handlers read through this cache).

## Estimated complexity

Small-to-Medium — direct copy of an already-proven, well-documented pattern; the only real decision is resolving the `CacheKeys.Users` naming ambiguity.

## Risks

- Because User and Auth share one Redis instance, a careless key choice risks an actual collision (not just a style nit) — resolve the `CacheKeys.Users` ambiguity explicitly before writing any code, don't defer it.
- `CacheOptions.KeyPrefix` being bound-but-unused is a standing trap (per `docs/reference/caching.md:9`) — don't assume it provides any automatic namespacing; the fully-qualified key string is 100% the developer's responsibility.

## Completion checklist

- [ ] `CacheKeys.Users` naming ambiguity resolved and documented (renamed / redefined / retired)
- [ ] New cache key group added with a short, explicitly-chosen TTL
- [ ] `UserProfileCacheService` implemented, mirroring `RoleCacheService`'s shape
- [ ] Decorator wrapping the User Detail read seam implemented, registered manually (not Scrutor) in `User.Infrastructure/DependencyInjection.cs`
- [ ] Confirmed no key collision risk against Auth's existing `auth:roles:*`/`auth:users:*` keys on the shared Redis instance
