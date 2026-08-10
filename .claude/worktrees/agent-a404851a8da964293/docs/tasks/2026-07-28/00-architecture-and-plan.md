# User Service Refactor — Architecture Notes & Task Breakdown

**Source:** Planning-only request, 2026-07-28 — "User Service Search, Elasticsearch, Localization & Cache Layer." No implementation performed. This document is the required pre-implementation deliverable (architecture notes, affected modules, dependency graph, implementation order, risks, rollback) referenced by every `TaskN_*.md` file in this folder. Read `docs/reference/search.md`, `docs/reference/caching.md`, `docs/reference/grpc.md`, `docs/services/user-service.md` first — this doc doesn't repeat their content, it applies them to the new scope.

## What this refactor actually touches (confirmed by full-repo research, not assumption)

Six areas were independently audited end-to-end. The three most consequential findings that reshape the original request's assumptions:

1. **gRPC "optimization" is greenfield, not a fix.** `user.proto` has exactly one RPC (`CreateUserProfile`, write-only), and grepping every service confirms **zero** current consumers of User via gRPC reads. Order and Audit both avoid the problem today by denormalizing a name/actor snapshot at write time (`OrderOwner.CustomerName`, `AuditTrailMetadata.Actor`) instead of calling User. There is no N+1 pattern to eliminate — Task 13/14/15 below are new capability, not a refactor.
2. **The locale header already flows from the frontend.** `NovaCoreUI/src/shared/lib/api/client.ts:23` unconditionally sends `Accept-Language` on every request today, sourced from `useLocaleStore` (`locale.store.ts`). Nothing reads it on the backend (zero hits for `RequestLocalization`/`CultureInfo`/`Accept-Language` anywhere in `NovaCore/src`). **This means Section 5 of the original ask needs zero frontend work to get a header sent** — only a backend `ICurrentLocaleService` (Task 4) is required. The value will always be `"en"` in practice until a locale switcher UI is separately built (Frontend Task 5, explicitly out of scope unless the business wants it).
3. **Product's Elasticsearch mapping has no accent-folding.** It relies solely on ES's default `standard` analyzer (case-insensitive, not accent-insensitive). The "search regardless of locale/accent" requirement (e.g. `café` ≈ `cafe`) cannot be satisfied by copying Product's mapping verbatim — Task 7 is genuinely new ground (a custom analyzer/normalizer with `asciifolding`), the one piece of this whole epic without an in-repo precedent to lift.

## Affected modules (complete list)

**Backend — NovaCore:**
- `User.Domain` (`UserProfile.cs`), `User.Application` (Commands/Queries/Events/Search/Abstractions), `User.Persistence` (Configs, Contexts/UserProfiles, Migrations, Seeder), `User.API` (Endpoints, GrpcServices, ApplicationPipeline)
- `BuildingBlock.Contract` (`user.proto`, `Events/User/UserProfileCreatedIntegrationEvent.cs`) — shared wire contracts
- `Auth.Application`/`Auth.Infrastructure`/`Auth.API` — Register flow mirrors User's name fields end-to-end (`RegisterCommand`, `RegisterValidator`, `OnUserRegisteredEvent`, `OnUserCreatedEvent`, `UserProfileServiceClient`, `Register.cs`)
- `Notification.Infrastructure/Messaging/Consumers/NotificationTriggerConsumer.cs` — greets by `FirstName` only today
- `BuildingBlock.SharedKernel` (`CacheKeys.cs`, new `HeaderKeys.Locale`), `BuildingBlock.Application` (new `ICurrentLocaleService`), `BuildingBlock.Infrastructure` (new `CurrentLocaleService`, User Detail cache decorator precedent), `BuildingBlock.Search` (reused as-is, no changes expected), `BuildingBlock.Grpc` (reused as-is)
- Order/Audit — **not modified in this epic's core scope**, only candidates for Task 15's "first consumer" decision

**Frontend — NovaCoreUI:**
- `src/features/users/*` (UsersPage, CreateUserForm, EditUserForm, users.schema.ts), `src/features/auth/*` (RegisterForm, auth.schema.ts, auth.queries.ts's `toSessionUser`), `src/services/user/*.ts`, `src/services/auth/register.ts`, `src/i18n/messages/en/{users,auth}.json`

## Dependency graph

```
Task 1 (MiddleName: Domain+Persistence)
  → Task 2 (MiddleName: Application layer, User service only)
      → Task 3 (MiddleName: cross-service contracts — Auth, events, proto)
          → Frontend F1/F2/F3 (forms + types + toSessionUser)

Task 4 (ICurrentLocaleService)
  → Task 5 (DisplayName formatter, consumes locale + MiddleName)
      → needs Task 2 done first (MiddleName must exist to format it)
      → Frontend F3 partial (UsersPage name column can switch to server DisplayName)

Task 6 (ES scaffolding, mirrors Product)
  → Task 7 (UserSearchDocument + accent-insensitive mapping — new ground)
      → Task 8 (ProjectionBuilder + sync events, needs Task 2's MiddleName + Task 5's SearchName inputs)
          → Task 9 (Rebuild command + ES config/docker wiring)
              → Task 10 (Cut SearchUsers over to ES) → Frontend F4
                  → Task 16 (migration/reindex) — last, needs everything above deployed once

Task 11 (Cache: CacheKeys + decorator scaffold)
  → Task 12 (wire invalidation into Create/Update/Delete handlers)

Task 13 (proto: GetUser/GetUsers RPCs)
  → Task 14 (server impl, depends on Task 11/12's cache existing)
      → Task 15 (first real consumer — decision + implementation)

Task 17 (testing) — cuts across all of the above, write incrementally per task, not after
Task 18 (docs) — update reference docs as each piece lands, not a single end-of-project pass
```

Task 1→2→3 (name model) and Task 4→5 (locale/display name) are the two hard prerequisites almost everything else assumes: Task 8 (search projection) needs both MiddleName and the SearchName-normalization approach from Task 5/7; Task 10 needs Task 8's index populated. Task 11/12 (cache) and Task 13/14/15 (gRPC) are independent of the name-model work and can proceed in parallel with it. Elasticsearch (6–10) is the longest chain and the highest-effort area — start it first if wall-clock time matters.

## Suggested implementation order (phased)

1. **Phase A — Name model** (Tasks 1, 2, 3): smallest blast radius per step, unlocks everything else. Ship Auth/Notification changes in the same phase since they share the same wire contract.
2. **Phase B — Locale + DisplayName** (Tasks 4, 5): independent of Elasticsearch; can run in parallel with Phase C.
3. **Phase C — Cache + gRPC** (Tasks 11, 12, 13, 14, 15): independent of search; can run in parallel with Phase B. Task 15's consumer choice needs a product decision before starting (see Task 15's own file).
4. **Phase D — Elasticsearch** (Tasks 6–10, then 16): start once Phase A/B land, since the search document needs MiddleName + the DisplayName/SearchName formatting rules as inputs. This is the largest phase; Task 7 (accent-insensitive mapping) is the one piece with no in-repo template, budget extra time for it.
5. **Phase E — Testing & docs** (Tasks 17, 18): threaded through every phase above, not a final step; listed last here only to describe the "done" bar for the whole epic.
6. **Frontend** tracks Phase A immediately (F1–F3), Phase D for search UI changes if the request/response contract shifts (F4), and treats locale-switcher UI (F5) as a separate, optional follow-up since the wire mechanism already works without it.

## Risks

- **Accent-insensitive ES mapping (Task 7) has no in-repo precedent** — highest-uncertainty item in the whole epic; likely needs an ES custom analyzer (`asciifolding` token filter) or ICU plugin, neither used anywhere else in this stack today. Budget a spike before committing to an approach.
- **Dual-write period during Elasticsearch cutover**: Product's precedent was a full migration (old Postgres `ILIKE` path deleted once ES became authoritative) — Task 10 needs an explicit decision on whether User keeps a Postgres fallback or fully cuts over; a half-migrated state (some clients on old search, some on new) is the worst outcome.
- **`CacheKeys.Users` in `BuildingBlock.SharedKernel` is dead, unused, and wrongly namespaced** (`"auth:users:*"` — clearly scaffolded for Auth's own account concept, not User service's `UserProfile`). Task 11 must not silently repurpose it; decide whether to delete it or rename it, otherwise a future reader will assume it's already wired up and it isn't.
- **`DeleteUserCommand`/`DeleteUserHandler` appear to be dead code** (no caller anywhere in the repo — the real deletion path is `UserAccountDeletionIntegrationEventConsumer` → `OnUserDeletionEvent`). Any task touching deletion (cache invalidation, search-index removal) must target the real path, not the unused command.
- **DB column length (256) vs. FluentValidation length (50) already disagree** for `FirstName`/`LastName` — Task 1 must pick one convention for `MiddleName` and document it, not silently introduce a third inconsistency.
- **Blocking index rebuild, no blue/green alias swap** — this is a known, accepted limitation Product already carries (`docs/reference/search.md`'s documented future extension point); User inherits the same limitation unless Task 9 explicitly decides to fix it now (not recommended — match Product's current scope, don't scope-creep into a harder problem this task didn't ask for).
- **Locale will always resolve to `"en"` in production** until someone builds a frontend locale switcher (Frontend Task 5) — don't let Task 5's (backend) formatter design assume real multi-locale traffic will exercise it; test the vi-VN path explicitly since production traffic won't exercise it by accident.

## Rollback considerations

- **Name model (Phase A)**: `MiddleName` is additive and defaults to empty everywhere (DB default `""`, DTOs optional) — safe to deploy and safe to roll back; no destructive migration.
- **Elasticsearch (Phase D)**: keep the Postgres-backed `SearchUsers` path deployable behind a feature flag/config toggle until the ES path is verified in production (mirrors the "fail-open, don't let ES take down the API" pattern Product already uses at startup) — full cutover (deleting the old path) should be a separate, later change once ES is proven, not bundled into the same deploy.
- **Cache (Phase C)**: the cache is purely additive read-through — if it misbehaves, removing the decorator registration and falling back to direct `IUserProfileReadService` calls is a one-line DI change, no data risk.
- **gRPC (Phase C)**: new RPCs are additive to the proto (never removing `CreateUserProfile`) — safe for old/new client version skew during rollout.

## Task index

See `PROGRESS.md` in this folder for live status. Task files:

| # | Title | Category |
|---|---|---|
| 1 | Add MiddleName — Domain + Persistence | Backend |
| 2 | Add MiddleName — Application layer (User service) | Backend |
| 3 | Propagate MiddleName — cross-service contracts (Auth, events, proto) | Backend |
| 4 | Build `ICurrentLocaleService` (locale-from-header ambient context) | Localization |
| 5 | Build locale-aware DisplayName formatter | Localization |
| 6 | Scaffold User Elasticsearch search (mirror Product architecture) | Elasticsearch |
| 7 | Design UserSearchDocument + accent-insensitive mapping | Elasticsearch |
| 8 | ProjectionBuilder + sync events (self-consumption) | Elasticsearch |
| 9 | RebuildUserSearchIndex command + ES config/docker wiring | Elasticsearch / Infrastructure |
| 10 | Cut SearchUsers over to Elasticsearch-backed query | Elasticsearch |
| 11 | User Detail cache — CacheKeys + decorator scaffold | Cache |
| 12 | Wire cache invalidation into Create/Update/Delete | Cache |
| 13 | Extend `user.proto` with GetUser/GetUsers RPCs | gRPC |
| 14 | Implement server-side GetUser/GetUsers (cache-backed) | gRPC |
| 15 | First real gRPC consumer (decision + implementation) | gRPC |
| 16 | Migration/reindex review | Infrastructure |
| 17 | Testing (unit + integration, threaded through all phases) | Testing |
| 18 | Documentation updates | Documentation |

NovaCoreUI's paired folder (`docs/tasks/2026-07-28/` in that repo) has Frontend Tasks F1–F7, cross-referenced from the relevant backend tasks above.
