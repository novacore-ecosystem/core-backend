# Task 4: `POST /users/search` already exists — frontend Task 8's premise was stale; the real gap is `Roles`

**Scope:** NovaCoreUI's `docs/tasks/2026-07-22/Task8_users-page-role-tabs.md` asked to split the admin Users page into Admin/Normal-User tabs, and reported it as **blocked** because "User service has no list/search endpoint at all." That's no longer true — investigating this task found the endpoint already built, just undocumented (`docs/services/user-service.md`'s routes table only lists 4 routes and predates it). This task corrects the record and scopes what's actually still missing.

## What already exists

- `POST /users/search` — `User.API/Endpoints/SearchUsers.cs`. `RequireAdmin`. Body is a `CriteriaRequest` (keyword + `filters`/`sorts`/`page`/`pageSize`), same shape used elsewhere in this codebase (e.g. Product search).
- `SearchUsersQuery`/`SearchUsersHandler` (`User.Application/Features/Users/Queries/SearchUsers/`) → `IUserProfileReadService.SearchAsync(CriteriaRequest, ct)` → `UserProfileReadService.SearchAsync` (`User.Persistence/UserProfiles/Read/UserProfileReadService.cs:19-25`), which is just `dbContext.UserProfiles.AsNoTracking().ApplyCriteria(UserCriteriaDefinition.Instance, request).ToCriteriaPagedResultAsync(request, ct)` — real Postgres-index-backed paging, not an in-memory scan.
- Whitelisted/searchable fields (`UserCriteriaDefinition`, `User.Application/Features/Users/Search/UserCriteriaDefinition.cs`): `userName`, `email` (sortable, keyword-searchable), `firstName`, `lastName` (keyword-searchable), `status` (sortable), `phone` (prefix/suffix search via `UsePhoneSearch`), `createdAt` (sortable).
- Response item, `SearchUsersItemResponse` (`SearchUsersQuery.cs:8-17`): `Id, Email, UserName, PhoneNumber, FirstName, LastName, Status, CreatedAt, UpdatedAt`.

## What's actually still missing: `Roles`

`SearchUsersItemResponse` has **no `Roles` field**, and `UserCriteriaDefinition` has no `role` filter. This is the one real gap for the frontend's ask (Admin/Normal-User tabs needs to know each row's role to bucket it).

Why it's missing, not just an oversight: roles aren't owned by User at all — `RoleCacheReader`/`IRoleCacheReader` (`User.Infrastructure/Caching/RoleCacheReader.cs`) reads a **per-user** Redis cache key (`CacheKeys.Roles.UserRoles(userId)`) that Auth's `RoleCacheService` populates, falling back to a gRPC call to Auth on a cache miss. There is no batch/multi-user variant of this lookup anywhere in the codebase today (`IAuthClientService.GetUserRolesAsync` is single-`userId` too, `User.Infrastructure/GrpcClients/AuthClientService.cs:26-33`). `GetUserDetailHandler` gets away with one lookup because it only ever resolves the *current* user; a paginated search result needs one row's worth of roles per page (bounded by `pageSize`, default 20 — not unbounded), which is a different, currently-unbuilt shape of call.

## Options for adding `Roles` to search results (not decided here — needs a call)

1. **Fan out per row on the User side, after paging.** `SearchUsersHandler` already has the paged `SearchUsersItemResponse` list (≤ `pageSize` rows) — call `roleCacheReader.GetUserRolesAsync` once per row (parallelized, e.g. `Task.WhenAll`) before returning. Cheapest to build, no schema change, no cross-service contract change. Downside: can't **filter or sort by role** server-side this way (roles are attached after the DB query already ran) — a role tab would have to fetch all pages and bucket client-side, or the frontend fetches once per role via repeated non-role filtered calls, which doesn't actually solve "server-side role filter."
2. **Denormalize roles onto `UserProfile` itself**, kept in sync via an Auth-originated integration event (Auth already knows every role change — same pattern User already uses for `RoleCacheReader` invalidation, just persisted instead of cached). Enables a real `role` field on `UserCriteriaDefinition` (filterable + sortable, index-backed). Bigger lift: new column + migration + a new/extended event consumer on the User side, and a decision about whether User should have any locally-persisted opinion about roles at all (today it deliberately doesn't — "User service never writes to this key, Auth owns population/invalidation").
3. **Punt entirely — keep tabs client-side, unfiltered.** Frontend fetches one page via `/users/search` with no role filter, then does its own per-row role fetch (existing `GetUserDetail`-style single-user role lookup, or a new lightweight batch endpoint) to bucket into tabs client-side. Works for small user counts, doesn't scale to "give me page 3 of Admins" with real pagination.

Recommend picking based on how "real" the pagination needs to be for the Admin/Normal split — if it's just a small admin user base being eyeballed, option 1 is enough; if this needs to paginate correctly per-role at scale, option 2 is the only one that actually delivers that.

## Also worth fixing regardless of the above

- `docs/services/user-service.md`'s routes table (lines 13-20) is stale — missing `POST /users/search` entirely. Should be added once whichever option above is picked (so it's documented once, not twice).

## Status

Done — option 2 (denormalize) was picked. `UserProfile.Roles` (`string[]`, GIN-indexed, migration `AddUserProfileRoles`) is a write-once snapshot populated by both profile-creation paths (`CreateUserHandler` from its `Roles` input; `OnUserInitiatedHandler` hardcoded to `[AppRole.User]`, since Auth's self-registration flow never grants anything else). No new cross-service event was needed — both paths already know the roles at creation time, so this stayed a plain domain/persistence change, not the event-driven sync originally sketched in option 2. `UserCriteriaDefinition` gained a `role` field (`Eq`/`Ne`, via a new reusable `StringCollectionContainsStrategy<TEntity>` in `BuildingBlock.Criteria.Strategies`) and `SearchUsersItemResponse` gained a `Roles` field. `docs/services/user-service.md` updated (routes table + new "Denormalized Roles" section).

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-22/Task8_users-page-role-tabs.md`.
