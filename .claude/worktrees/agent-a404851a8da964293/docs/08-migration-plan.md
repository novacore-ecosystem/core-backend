# Documentation Migration Plan

**Scope:** what happened to every file from the previous `/docs` tree (plus the 3 misplaced Authorization docs in `src/`) during this reorganization, and why. Old files are preserved under `docs/_archive/` (original relative path kept) for history — they are **not** part of the live documentation set and should not be read for current guidance; several were stale or duplicated.

## Why this reorganization happened

The old tree had grown to 24 files with heavy duplication (three separate "how to add a service" docs with overlapping checklists), significant staleness (the two largest docs, `SERVICE_TEMPLATE.md` at 827 lines and `AUTH_CONFIG.md`, described a pre-`BuildingBlock.Web` architecture with the wrong port convention), conflicting facts across files (ports, credentials), no documentation of `BuildingBlock.Web` at all despite it being the most consequential recent architectural change, and no deterministic way to know which files a given task actually required — every implementation risked re-deriving architecture from scratch by exploring source. See [05-context-loading-map.md](05-context-loading-map.md) for the fix.

## Disposition of every old file

| Old file | Disposition | Where its content lives now |
|---|---|---|
| `docs/README.md` | **Replaced** | This tree's `docs/README.md` |
| `architecture/EVENT_ARCHITECTURE.md` | **Condensed, superseded** | [reference/events.md](reference/events.md) — content was accurate, trimmed of redundant framing and cross-linked into the new structure |
| `architecture/DOCKER_CONFIGURATION.md` | **Merged** | [setup/docker.md](setup/docker.md) (merged with `DOCKER_COMPLETE.md`) |
| `architecture/NETWORK.md` | **Archived as-is** | Content still accurate; not yet promoted into the numbered core docs — referenced from [01-architecture-map.md](01-architecture-map.md#networking), open `docs/_archive/architecture/NETWORK.md` directly for full port tables if needed |
| `building-blocks/EXCEPTIONS.md` | **Merged, corrected** | [reference/exceptions.md](reference/exceptions.md) — corrected the stale "per-service GlobalExceptionHandler" setup instructions (now centralized in `BuildingBlock.Web`) |
| `building-blocks/EXCEPTION_PATTERNS.md` | **Merged** | [reference/exceptions.md](reference/exceptions.md) (`ExceptionFactory` table) |
| `building-blocks/CACHING.md` | **Merged, extended** | [reference/caching.md](reference/caching.md) — added the Gateway's separate minimal Redis path and User's read-only role-cache consumer, neither of which the old doc covered |
| `building-blocks/INFRASTRUCTURE.md` | **Removed (stale example)** | Its "full `Program.cs` example" showed hand-wired Swagger/CORS/exception-handling that no longer matches either service; superseded by [06-implementation-templates.md](06-implementation-templates.md) and [workflows/new-service-scaffold.md](workflows/new-service-scaffold.md) |
| `building-blocks/GRPC.md` | **Pruned, condensed** | [reference/grpc.md](reference/grpc.md) — dropped unconfirmed streaming/retry/service-mesh sections the original doc itself flagged as unverified |
| `building-blocks/SAGA.md` | **Condensed, re-flagged** | [reference/saga.md](reference/saga.md) — shortened from 480 lines; "currently unused by any service" now stated up front instead of only implied |
| `building-blocks/SERIALIZATION.md` | **Condensed** | [reference/serialization.md](reference/serialization.md) |
| `guides/SERVICE_TEMPLATE.md` | **Replaced (was heavily stale)** | [workflows/new-service-scaffold.md](workflows/new-service-scaffold.md) — old doc showed manual Swagger/CORS/Carter/health-check wiring (pre-`BuildingBlock.Web`) and the wrong port convention (5101/5003 instead of 8080/5002) |
| `guides/NEW_SERVICE_WORKFLOW.md` | **Merged into the replacement above** | [workflows/new-service-scaffold.md](workflows/new-service-scaffold.md) — its accurate parts (compose/gateway/env wiring) were folded in; overlapping "checklist" content with `SERVICE_TEMPLATE.md` de-duplicated |
| `guides/DEVELOPMENT_CRITERIA.md` | **Redistributed** | Its per-layer checklist content is now the specific workflow docs (`workflows/add-new-api.md`, `add-new-domain-entity.md`, `add-new-repository.md`, etc.) and [04-coding-rules.md](04-coding-rules.md), each scoped to one task instead of one giant checklist |
| `guides/ENV_CONFIGURATION.md` | **Condensed** | [setup/environment-config.md](setup/environment-config.md) — trimmed CI/CD boilerplate and repeated DO/DON'T lists that overlapped `DOCKER_CONFIGURATION.md` |
| `guides/ROLE_CACHING.md` | **Merged** | [reference/caching.md](reference/caching.md) — merged with `CACHING.md`; dropped PR-description-style "Files Created" section |
| `services/AUTH_CONFIG.md` | **Replaced (internally contradictory)** | [services/auth-service.md](services/auth-service.md) — old doc had two eras of content mixed (stale boilerplate + an accurate late-added section) and self-contradicted on ports (8080 vs 5000) |
| `services/USER_SERVICE.md` | **Replaced (stale routes)** | [services/user-service.md](services/user-service.md) — old doc documented `/users/...` routes that don't exist; real routes are `/profiles/...` |
| `services/GATEWAY.md` | **Replaced, extended** | [services/gateway.md](services/gateway.md) — added the JWT-simplification and refresh-token-filter middleware this session introduced, which the old doc predates entirely |
| `setup/DOCKER_COMPLETE.md` | **Merged** | [setup/docker.md](setup/docker.md) |
| `setup/CREDENTIALS.md` | **Replaced (stale password, dead link)** | [setup/credentials.md](setup/credentials.md) — had `Postgres2024` vs actual `Postgres2026`, and linked to a `DOCKER_TROUBLESHOOT.md` that never existed; also referenced a PgAdmin/Mongo Express setup no longer in `docker-compose.yml` |
| `setup/DATABASE_SPLIT_GUIDE.md` | **Condensed, kept accurate content** | [setup/database-split.md](setup/database-split.md) |
| `troubleshooting/SEQ.md` | **Condensed, fixed dead link** | [troubleshooting/seq.md](troubleshooting/seq.md) |
| `decisions/EVENT_MESSAGING_REFACTOR.md` | **Copied as-is (content was already good)** | [decisions/event-messaging-refactor.md](decisions/event-messaging-refactor.md) |
| `src/.../Authorization/README.md` | **Moved into `docs/`, merged** | [reference/authorization.md](reference/authorization.md) — was misplaced in the source tree, undiscoverable from `docs/README.md` |
| `src/.../Authorization/EXAMPLES.md` | **Merged** | [reference/authorization.md](reference/authorization.md) — representative examples kept, redundant ones with README dropped |
| `src/.../Authorization/IMPLEMENTATION_SUMMARY.md` | **Not merged — dropped** | Read as a PR/commit description ("Files Created", "Modified Services"), not durable reference material; that kind of content belongs in commit history, not a permanent doc |

## New content with no prior equivalent

- [05-context-loading-map.md](05-context-loading-map.md) — did not exist in any form; the single biggest gap the old tree had
- [03-building-blocks-reference.md](03-building-blocks-reference.md) — no doc previously covered `BuildingBlock.Web`, `BuildingBlock.SharedKernel`, `BuildingBlock.Application`, or `BuildingBlock.Messaging`/`Messaging.Kafka` as reference entries (only `EVENT_ARCHITECTURE.md` touched Messaging, narratively)
- [02-architecture-rules.md](02-architecture-rules.md) — dependency-direction and layer rules were previously only inferable from scattered checklist bullets
- [04-coding-rules.md](04-coding-rules.md) and [06-implementation-templates.md](06-implementation-templates.md) — no prior doc had accurate, current copy-paste templates
- [07-solid-recommendations.md](07-solid-recommendations.md) — new, per this task's explicit requirement
- [decisions/buildingblock-web-extraction.md](decisions/buildingblock-web-extraction.md) — the extraction had zero documentation before this
- Workflow docs for fix-bug, refactor-existing-code, performance-optimization, production-incident, add-background-job, add-integration-event, add-new-domain-entity, add-new-repository, project-initialization — none existed before

## Known issues surfaced during the audit, not fixed here (docs-only constraint)

- `User.Application`'s `GetUserQueryHandler`/`UpdateUserCommandHandler` throw raw `InvalidOperationException` instead of `NotFoundException` — documented in [services/user-service.md](services/user-service.md#known-issues), not fixed (would be a code change, out of scope for this task).
- `scripts/startup.sh`'s printed summary banner and `scripts/health-check.sh`'s port/service assumptions are stale (wrong ports, wrong Postgres compose-service name, checks for services that don't exist yet) — flagged in [setup/docker.md](setup/docker.md), scripts themselves not modified (out of scope).
- Mapster is registered in both services but never used — documented as the current accepted convention in [04-coding-rules.md](04-coding-rules.md#mapping) rather than silently treated as a pattern to follow.
