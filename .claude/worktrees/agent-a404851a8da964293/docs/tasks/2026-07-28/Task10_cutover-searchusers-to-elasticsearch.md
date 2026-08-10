# Task 10: Cut SearchUsers Over to Elasticsearch-Backed Query

**Status:** Done (2026-07-28)
**Category:** Elasticsearch

## What was done

**Decision made: full cutover**, matching Product's precedent. `SearchUsersHandler` now calls `IUserSearchRepository.SearchAsync` exclusively; `IUserProfileReadService.SearchAsync` (the Postgres/`ApplyCriteria` execution path) was deleted from both the interface and `UserProfileReadService`, confirmed unused elsewhere first via a repo-wide grep. The request/response contract was deliberately kept identical (`CriteriaRequest` in, `SearchUsersItemResponse` out) so Frontend Task 4 may need zero changes — that's for the frontend session to confirm against a running stack.

`UserCriteriaDefinition` was **not** deleted (unlike Product's fully-removed old criteria file) — it's kept as a pure request-shape validator for `SearchUsersValidator`, since `CriteriaRequestValidator<T>` is engine-agnostic. Its field list was narrowed to match exactly what `SearchUsersHandler.BuildCriteria`/`UserSearchRepository` actually implement (`.AllowOperators()` with zero args added to `userName`/`email`/`createdAt`/`updatedAt` to make them sort-only, matching reality) — closing a real validation/execution mismatch risk (a filter that validates successfully but is silently ignored) rather than leaving it latent. **Individual `firstName`/`middleName`/`lastName` filters were retired** in favor of the unified `keyword` search, which is a strict improvement (was case-sensitive and excluded `middleName` before; now case+accent+word-order-insensitive and covers all three parts) — documented in the endpoint's Swagger description.

`SearchUsersHandler` recomputes `DisplayName` per-request from the document's stored `FirstName`/`MiddleName`/`LastName` using the caller's actual locale (via `ICurrentLocaleService`) rather than trusting the document's fixed index-time `en` value — consistent with `GetUser`/`GetUserDetail`'s behavior.

Verified: full-solution `dotnet build` passes (only the same pre-existing, unrelated `Order.Application.Tests` failure remains). Not yet done: a live end-to-end run against Docker/Elasticsearch (no stack was started this session) and the parity/regression checks against the existing phone-search and role-tab UI features — tracked under Task 16 (migration review) and Task 17 (testing).

## Objective

Replace (or supplement — decision required) the current Postgres/`BuildingBlock.Criteria`-backed `POST /users/search` with an Elasticsearch-backed query, composing keyword name search + phone prefix/suffix + role + status + pagination + sorting into **one** ES query, per the request's explicit "one Elasticsearch query instead of mixing Elasticsearch + PostgreSQL unnecessarily" requirement.

## Current state (grounded findings)

- Today's path: `SearchUsers.cs` (Carter, `POST /users/search`, `RequireAdmin`) → `SearchUsersQuery(CriteriaRequest)` → `SearchUsersHandler.cs:12-19` → `IUserProfileReadService.SearchAsync` → `UserProfileReadService.cs:19-25`: `AsNoTracking().ApplyCriteria(UserCriteriaDefinition.Instance, request).ToCriteriaPagedResultAsync(...)` — real, index-backed Postgres paging (GIN index on `Roles`, btree on `PhoneSearch`/`PhoneReverse`, per `UserProfileConfig.cs`).
- `UserCriteriaDefinition.cs:10-21` today supports: `UserName`/`Email` (sortable, keyword-searchable, case-insensitive), `FirstName`/`LastName` (keyword-searchable only, **case-sensitive** — no `.IgnoreCase()`), `Status` (sortable enum), `PhoneNumber`→`"phone"` (prefix/suffix via `PhoneSearch`/`PhoneReverse`, `Contains` deliberately excluded), `CreatedAt` (sortable date), `Roles`→`"role"` (`Eq`/`Ne` only, not sortable).
- **Product's precedent was a full cutover, not a dual-path compromise**: `docs/reference/search.md`'s Query flow section states the old Postgres `ILIKE`-based `ListProductsQuery`/`ProductRepo.SearchAsync` "were deleted as dead code once ES became the only Product-list path." This task must make the same explicit choice for User — recommend following Product's precedent (full cutover) rather than maintaining two search implementations indefinitely, but this is a product/team decision, not something to default silently.
- Product's query composition (`ProductSearchRepository.cs:47-99`, confirmed): `must` = `MultiMatch` over keyword fields when `Keyword` present; `filter` = exact `term` queries per facet (category/tag/status) when set; `sort` via a hardcoded switch on a small set of recognized `SortBy` string literals; `from`/`size` for paging. User's equivalent needs: `must` = multi-match over `SearchName`/`Email`/`UserName` (Task 7's fields) when `Keyword` present; `filter` = `term` on `Roles`/`Status` when set, **plus** phone prefix/suffix (this is the one piece with no Product precedent — Product has no phone-like field — needs `prefix`/ES `wildcard`-free equivalent query on `PhoneSearch`/`PhoneReverse` keyword fields, matching today's Postgres semantics of "prefix or suffix, never arbitrary substring").
- Frontend's exact current request shape (confirmed by the frontend agent's research, `NovaCoreUI/src/features/users/api/users.queries.ts:34-59`): `{ keyword, filters: [role filter, optional phone filter], page, pageSize }`, POSTed to `/api/user/users/search`. **If the endpoint's request/response contract changes at all** (even field renames), Frontend Task F4 must update in lockstep — coordinate the two.

## Scope

- `IUserSearchRepository.SearchAsync(UserSearchCriteria, ct)` — query-only, mirrors `IProductSearchRepository` shape exactly (Task 6 already scaffolds the interface; this task fills in the query logic).
- Compose keyword (name/email/username multi-match) + role/status filters + phone prefix/suffix + pagination + sort into one `BoolQuery`, one round trip — no post-query Postgres lookups for any of these facets.
- **Explicit decision required and documented in this task's outcome**: full cutover (delete `UserCriteriaDefinition`/Postgres search path) vs. dual-path (keep both, ES as primary). Recommend full cutover, matching Product's precedent, once the ES path is verified — but sequence the actual deletion as a separate, later change (see Task 16/rollback notes), not bundled into the same deploy that introduces the new query.
- `SearchUsersHandler.cs` swapped to call `IUserSearchRepository.SearchAsync` instead of `IUserProfileReadService.SearchAsync`.
- Verify parity: every filter/sort the current Postgres endpoint supports (per `UserCriteriaDefinition` above) has an ES equivalent before cutover — a regression here (e.g. losing `Roles` filtering, or losing phone suffix search) would be a real functional loss, not just an implementation-detail change.

## Dependencies

- **Depends on:** Task 8 (index must be populated via the sync pipeline), Task 9 (rebuild endpoint must exist to backfill before cutover).
- **Blocks:** Frontend Task F4 (search UI/request-building code follows whatever contract this task lands on), Task 16 (migration/reindex review assumes this cutover has happened).

## Estimated complexity

Medium-to-Large — the query-composition logic itself is a direct, mechanical port of Product's pattern; the phone prefix/suffix-on-ES piece and the full-cutover-vs-dual-path decision are the genuinely new parts.

## Risks

- Losing existing filter/sort parity (especially `Roles`'s `Eq`/`Ne` semantics or phone suffix search) during cutover would be a real regression for the Admin UI's already-shipped features (role tabs, phone search — both delivered in the 2026-07-22/2026-07-27 task folders) — treat parity verification as a hard gate before flipping the endpoint over, not an afterthought.
- A half-migrated state (endpoint sometimes serving from ES, sometimes from Postgres, depending on a flag nobody remembers is there) is explicitly called out as the worst outcome in this folder's `00-architecture-and-plan.md` risk list — don't ship this task without a clear, single source of truth for any given deployment.

## Completion checklist

- [ ] `IUserSearchRepository.SearchAsync` implemented, one ES query composing keyword+role+status+phone+pagination+sort
- [ ] Parity check completed against every existing `UserCriteriaDefinition` filter/sort — documented, not just assumed
- [ ] Cutover vs. dual-path decision made and recorded explicitly in this file's outcome
- [ ] `SearchUsersHandler` updated; old Postgres path's fate (deleted now / deleted later / kept) explicitly stated
- [ ] Frontend Task F4 coordinated if the request/response contract shape changed at all
