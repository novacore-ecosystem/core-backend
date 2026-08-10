# Task 9: RebuildUserSearchIndex Command + ES Config/Docker Wiring

**Status:** Done (2026-07-28)
**Category:** Elasticsearch / Infrastructure

## What was done

`RebuildUserSearchIndexCommand`/`Handler` added, reusing `UserSearchProjectionBuilder`/`IUserSearchIndexer` exactly like Product's rebuild handler (paged 200/batch via the new `IUserProfileReadService.GetAllAsync`, `RecreateIndexAsync` then `BulkIndexAsync` per batch). `POST /users/search/rebuild` endpoint added (`RequireAdmin`), with the auth requirement spelled out in the Swagger description from day one — closing the exact gap Product's equivalent endpoint left undocumented for a while. `User.API/Program.cs` gained the fail-open `EnsureIndexAsync()` bootstrap call (try/catch around Elasticsearch connectivity, logs but doesn't crash the API on failure, identical to Product's). `.env`/`.env.template` gained `USER_ELASTICSEARCH_URL`; `docker-compose.override.yml`'s `user-api` service gained the 3-line `Elasticsearch__Url`/`Username`/`Password` block, reusing the same shared `elastic` app-user credentials Product already uses (no new per-service credential introduced). `user-api`'s `depends_on: elasticsearch: condition: service_healthy` already existed in `docker-compose.yml` (previously only for the shared logging sink) — no change needed there.

## Objective

Add the admin-triggered full-reindex command/endpoint (mirroring `RebuildProductSearchIndex`), provision User's own Elasticsearch connection configuration (which doesn't exist today), and bootstrap the index at startup — with the auth requirement documented explicitly this time, unlike Product's own gap here.

## Current state (grounded findings)

- Product's rebuild flow (`RebuildProductSearchIndexHandler.cs:13-44`, confirmed): `searchIndexer.RecreateIndexAsync(ct)` (blocking drop+create against a single, unversioned index name constant) → paged `GetAllAsync(skip, 200, ct)` loop → `ProjectionBuilder.BuildManyAsync(batch)` → `IProductSearchIndexer.BulkIndexAsync(documents)` — runs **synchronously inside the request handler**, no background job (documented as "appropriate for current catalog scale" in `docs/reference/search.md:101`, with a Hangfire job flagged as the natural extension point if it ever grows too slow — not needed now for User's presumably-similar or smaller scale).
- Endpoint: `POST /products/search/rebuild`, `RequireAdmin` (`RebuildProductSearchIndex.cs:23`) — but `docs/tasks/2026-07-27/Task11_rebuild-search-index-auth-undocumented.md` records that this requirement was **undocumented** even though the code already enforced it. This task's endpoint must document the auth requirement in its Swagger `API_DESC` from day one, closing the same gap proactively.
- **Elasticsearch config: User has zero wiring today**, confirmed by grepping `.env`/`.env.template`/`docker-compose.override.yml` for any `USER.*ELASTIC` key — none exist. Only `product-api` has `Elasticsearch__Url`/`Username`/`Password` env vars (`docker-compose.override.yml:214-217`) and a corresponding `PRODUCT_ELASTICSEARCH_URL` in `.env:314`. The `elasticsearch` container itself is already shared infrastructure (`docker-compose.yml:205-230`), and every service already depends on it in compose — but **that dependency today is solely for the centralized Serilog logging sink** (`Logging__Elasticsearch__Url`), unrelated to the Product search feature's own connection. User needs its own `Elasticsearch__Url`/`Username`/`Password` block, exactly mirroring product-api's 3 lines.
- Startup bootstrap: `Product.API/Program.cs:42-58` calls `IProductSearchIndexer.EnsureIndexAsync()` in a `try/catch` that only logs on failure — documented rationale: "Elasticsearch is a read-model dependency, not a hard requirement to serve traffic — don't let a transient ES outage/misconfiguration take down the whole API on boot." User's `Program.cs` needs the identical fail-open bootstrap call.
- **No ES health check exists anywhere in the repo** (`BuildingBlock.Web/HealthChecks/HealthCheckExtensions.cs`'s `AddHealthCheckServices()` registers zero checks — an intentional, documented gap per `docs/reference/search.md:36-38`). Not this task's job to add one; note it as a shared, pre-existing gap if the team wants it fixed later (applies to Product too, not User-specific).

## Scope

- `User.Application/Features/Users/Commands/RebuildUserSearchIndex/` (Command + Handler), reusing Task 8's `UserSearchProjectionBuilder`/`IUserSearchIndexer` — identical shape to Product's.
- `User.API/Endpoints/RebuildUserSearchIndex.cs` — `POST /users/search/rebuild`, `RequireAdmin`, with the auth requirement spelled out in `API_DESC` explicitly (the gap Product's equivalent endpoint left undocumented).
- `.env.template`/`.env`: add `USER_ELASTICSEARCH_URL` (and username/password if a distinct app-user is warranted, or reuse the same `elastic`/`ELASTICSEARCH_PASSWORD` as Product — decide based on whether per-service ES credentials are a security requirement here; Product's `es-init` shared-credentials model, per prior memory, uses one shared `elastic` app user across services already, so reusing it is consistent with existing practice).
- `docker-compose.override.yml`: add the 3-line `Elasticsearch__Url`/`Username`/`Password` block to `user-api`'s env, mirroring `product-api`'s exact shape.
- `User.API/Program.cs`: add the fail-open `EnsureIndexAsync()` bootstrap call after migrations, before `app.Run()`.

## Dependencies

- **Depends on:** Task 6 (scaffolding must exist), Task 8 (projection builder/indexer must exist for the rebuild handler to call).
- **Blocks:** Task 10 (query cutover needs an index that's actually been populated at least once), Task 16 (post-deploy reindex is literally this endpoint, called once in production).

## Estimated complexity

Small-to-Medium — mechanical replication of Product's proven shape; the only real decision is the credentials question above.

## Risks

- Forgetting the fail-open `try/catch` around `EnsureIndexAsync()` would make User's whole API fail to boot if Elasticsearch is briefly unavailable — copy Product's exact defensive pattern, don't skip it.
- If User's rebuild also runs synchronously in the request (matching Product), a large user base could make this endpoint slow/timeout-prone — acceptable for now per Product's own documented reasoning, but don't silently assume User's user count will always be small; note it as the same accepted trade-off Product made, not a newly-introduced one.

## Completion checklist

- [ ] `RebuildUserSearchIndexCommand`/`Handler` implemented, reusing Task 8's projection builder
- [ ] `POST /users/search/rebuild` endpoint added, `RequireAdmin`, auth requirement documented in Swagger from day one
- [ ] `.env`/`.env.template`/`docker-compose.override.yml` updated with User's Elasticsearch config
- [ ] `Program.cs` bootstrap call added, fail-open behavior verified (simulate ES down at boot, confirm API still starts)
- [ ] Manual test: rebuild endpoint run against a populated Postgres table, verify all documents land in the index
