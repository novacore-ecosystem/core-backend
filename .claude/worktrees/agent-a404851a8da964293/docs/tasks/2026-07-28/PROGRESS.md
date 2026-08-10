# Progress — 2026-07-28

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

Source: planning-only request — "User Service Search, Elasticsearch, Localization & Cache Layer." Full read-only impact analysis performed across Product's Elasticsearch implementation, User service's current state, cache infrastructure, gRPC consumers, and (in the sibling repo) frontend screens/locale handling. See [00-architecture-and-plan.md](./00-architecture-and-plan.md) for architecture notes, dependency graph, implementation order, and risks. Tasks 1-16 and 18 (name model, locale/DisplayName, Elasticsearch search, cache, gRPC, migration review, docs) implemented 2026-07-28, same session as the planning pass. Task 17 (testing) remains not started. Task 15's consumer choice (Audit) was made autonomously per the task's own recommendation - flag for team confirmation.

- [x] Task 1 — Add MiddleName: Domain + Persistence (`Task1_middlename-domain-and-persistence.md`) — done: entity, EF config, migration, seeder.
- [x] Task 2 — Add MiddleName: Application layer, User service (`Task2_middlename-application-layer.md`) — done: Commands/Validators/Queries/Criteria/endpoints.
- [x] Task 3 — Propagate MiddleName: cross-service contracts, Auth + events + proto (`Task3_middlename-cross-service-contracts.md`) — done: proto, integration events, full Auth Register chain.
- [x] Task 4 — Build `ICurrentLocaleService` (`Task4_current-locale-service.md`) — done: HeaderKeys.Locale (reuses Accept-Language), interface + Infrastructure impl, wired into User's DI.
- [x] Task 5 — Build locale-aware DisplayName formatter (`Task5_displayname-formatter.md`) — done: IUserDisplayNameFormatter, wired into GetUser/GetUserDetail/SearchUsers.
- [x] Task 6 — Scaffold User Elasticsearch search, mirror Product architecture (`Task6_elasticsearch-scaffolding.md`) — done.
- [x] Task 7 — Design UserSearchDocument + accent-insensitive mapping (`Task7_search-document-and-accent-insensitive-mapping.md`) — done via an additive BuildingBlock.Search overload; live-ES verification of diacritic handling still open (Task 17).
- [x] Task 8 — ProjectionBuilder + sync events, self-consumption (`Task8_projection-builder-and-sync-events.md`) — done, including the previously-missing `UserProfileUpdatedIntegrationEvent`; two triggers dispatch inline rather than via self-consumption (documented deviation).
- [x] Task 9 — RebuildUserSearchIndex command + ES config/docker wiring (`Task9_rebuild-command-and-es-config.md`) — done.
- [x] Task 10 — Cut SearchUsers over to Elasticsearch-backed query (`Task10_cutover-searchusers-to-elasticsearch.md`) — done: full cutover; live E2E run and parity checks still open (Tasks 16/17).
- [x] Task 11 — User Detail cache: CacheKeys + decorator scaffold (`Task11_user-detail-cache-scaffold.md`) — done, via a DTO-based reader instead of literally decorating IUserProfileReadService (private-setter constraint).
- [x] Task 12 — Wire cache invalidation into Create/Update/Delete (`Task12_cache-invalidation-wiring.md`) — done.
- [x] Task 13 — Extend `user.proto` with GetUser/GetUsers RPCs (`Task13_grpc-proto-getuser-getusers.md`) — done.
- [x] Task 14 — Implement server-side GetUser/GetUsers, cache-backed (`Task14_grpc-server-implementation.md`) — done.
- [x] Task 15 — First real gRPC consumer (`Task15_first-grpc-consumer.md`) — done: Audit chosen autonomously (task's own recommendation) - **flag for team confirmation**.
- [x] Task 16 — Migration/reindex review (`Task16_migration-and-reindex-review.md`) — done as a code-level review; top operational risk flagged: `RebuildUserSearchIndex` must run before the first real `SearchUsers` call once deployed.
- [ ] Task 17 — Testing, threaded through all phases (`Task17_testing.md`)
- [x] Task 18 — Documentation updates (`Task18_documentation-updates.md`) — done: user-service.md, search.md, caching.md, grpc.md, events.md, and both NovaCoreUI backend/user + backend/auth README.md updated.
- [x] Task 19 — Search relevance audit (`Task19_search-relevance-audit-and-plan.md`) — audit-only, done: root cause is `multi_match`/`standard` analyzer producing whole-token-only matching, plus `Roles` being an unanalyzed `Keyword` (exact-match only). Decisions made 2026-07-28 (email whole-token is enough, index all variation SKUs, defer Brand, alias infra first) — implementation (Tasks 20-27) not started.
- [x] Task 20 — Alias-based blue/green reindex infra (`Task20_alias-based-blue-green-reindex.md`) — done: `ElasticsearchIndexer<TDocument>` now treats every index name as an ES alias with versioned backing indices and an atomic swap on rebuild; zero caller/interface signature changes. `dotnet build` verified for `BuildingBlock.Search`/`Product.API`/`User.API`; no live-ES run (same caveat as the rest of this epic). Tasks 21-27 remain not started.

## Verification notes (Tasks 1-16)

`dotnet build` on `User.API`, `Auth.API`, `Audit.API`, and `Product.API` (sanity check for the `BuildingBlock.Search` change) all succeed cleanly, as does a full-solution build except one pre-existing, unrelated failure in `tests/unit/Order.Application.Tests/CancelOrderHandlerTests.cs` (references a removed `Order.CustomerId` — confirmed via `git status` that this test file was not touched this session). EF migration `20260728030503_AddUserProfileMiddleName` generated via `dotnet ef migrations add`, additive and reversible. No Docker/Elasticsearch stack was started this session, so live-ES behavior (accent-folding, real query results, index rebuild against real data) is unverified — only compile-time correctness is confirmed, and Task 16 flags running the rebuild endpoint before go-live as the top operational risk. Application-level unit/integration tests for the new formatter/locale service/search/cache/gRPC behavior are not yet written — tracked under Task 17.

## Key findings that reshaped the original request's assumptions

1. **gRPC "optimization" (Tasks 13-15) is greenfield, not a fix** — `user.proto` has exactly one RPC today (`CreateUserProfile`, write-only), zero services currently consume User via gRPC reads. Order/Audit both denormalize a name snapshot instead.
2. **The frontend already sends `Accept-Language` on every request** (`NovaCoreUI/src/shared/lib/api/client.ts:23`) — Task 4 needs zero frontend work to get a header flowing; only a backend reader is missing.
3. **Product's Elasticsearch mapping has no accent-folding** — only case-insensitivity via ES's default analyzer. Task 7 (accent-insensitive User search) has no in-repo precedent to copy; budget a spike.

## Cross-repo pairing

NovaCoreUI's `docs/tasks/2026-07-28/` folder has 7 frontend tasks (F1-F7), each cross-referencing the backend task it pairs with. Frontend Tasks 1-3 (forms/types) are blocked on backend Tasks 2/3/5; Frontend Task 4 (search UI) is blocked on backend Task 10; Frontend Task 5 (locale switcher UI) is explicitly optional/out-of-scope for this epic to function.
