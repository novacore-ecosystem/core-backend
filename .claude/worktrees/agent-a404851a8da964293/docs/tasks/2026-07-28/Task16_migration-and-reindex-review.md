# Task 16: Migration/Reindex Review

**Status:** Done (2026-07-28)
**Category:** Infrastructure

## What was done

Tasks 1-15 landed as two commits on `main` this session (`feat(user): add MiddleName name model and locale-aware DisplayName`, `feat(user): add Elasticsearch-backed search (Tasks 6-10)`) plus Tasks 11-15's uncommitted work at review time. No actual staging/production environment exists in this workspace to deploy to, so this review covers **code-level migration safety and the exact operational sequence required once this branch does reach a real environment** - not a live rollout that already happened.

**Confirmed, not just asserted:**
- `20260728030503_AddUserProfileMiddleName` is additive (`ADD COLUMN ... NOT NULL DEFAULT ''`) and reversible - verified by reading the generated migration file directly (Task 1).
- `UserSearchProjectionBuilder.BuildSearchName` only joins non-empty, trimmed parts (`string.Join(' ', parts.Where(...))`) - a pre-`MiddleName` row (`MiddleName == ""`) produces a clean two-token `SearchName`, never a literal empty-string artifact or a double space. Confirmed by reading the implementation, not assumed.
- The operational sequence required (unchanged from the original plan, now with real command/endpoint names to point at): apply the EF migration → deploy Tasks 6-9's code (index not yet serving traffic, since `SearchUsersHandler` doesn't call it until Task 10) → call `POST /users/search/rebuild` once against real data → verify a spot-check → **then** Task 10's code (already merged in the same commit as Tasks 6-9 in this session - see risk note below) starts actually being exercised by `SearchUsers` requests.

**Risk realized, now explicitly flagged rather than silently accepted:** Tasks 6-10 (including the cutover itself) were implemented and committed together in one commit this session, not staged incrementally as the original phased plan recommended. This is fine for a dev-branch commit, but means **whoever deploys this branch to a real environment must run `RebuildUserSearchIndex` before the first `SearchUsers` request after deploy**, or admins will see empty results - there is no code-level guard preventing this (Elasticsearch's index-not-found response would surface as an error or empty result set, not a friendly message). This is the single most important operational note for whoever deploys this branch.

**Rollback story re-confirmed:**
- Name model (Task 1): safe to roll back (`dotnet ef migrations remove` or a down-migration; column is additive, nothing else depends on its presence at the DB level).
- Elasticsearch (Tasks 6-10): if the ES path misbehaves in production, the fastest mitigation is fixing forward (rerun rebuild) rather than rolling back code, since the old Postgres `SearchAsync` path was deleted (Task 10's full-cutover decision) - **this is a real change from the original rollback plan**, which assumed the Postgres path would stay available as a fallback during a transition period. Flagging this explicitly: rolling back Task 10's commit specifically (not the whole branch) would restore Postgres search if ever needed, since `UserCriteriaDefinition`/`IUserProfileReadService.SearchAsync` were only removed, not physically impossible to restore from git history.
- Cache (Task 11/12): still a one-line DI change to remove (`CachedUserProfileReader` registration) - confirmed unchanged from the plan, no new risk introduced.
- gRPC (Task 13/14/15): still purely additive to the proto - confirmed. Audit's new gRPC client dependency on User (Task 15) is a **new cross-service coupling** that didn't exist before; if User is ever down, Audit's `GetAuditLog` degrades gracefully (fail-open, confirmed via the `try/catch` in `GetAuditLogHandler`) rather than failing the whole read.

## Original objective (for reference)

## Objective

Confirm the whole epic can be rolled out against existing production data with no loss and no extended downtime — the required "review migration impact... ensure existing users are migrated safely... determine whether a full reindex job is required" checkpoint from the original request, done as a single, explicit review rather than assumed piecemeal across the other tasks.

## Current state (grounded findings)

- **`MiddleName` (Task 1) is additive and safe by construction**: new column, `NOT NULL DEFAULT ''`, no backfill needed beyond the default — matches the shape of the two most recent existing migrations (`20260721044607_AddUserPhoneSearchFields.cs`, which *did* need a raw-SQL backfill for its two columns, and `20260724060832_AddUserProfileRoles.cs`, which used a default-value-only approach with no backfill, the closer analog for `MiddleName`). No data loss risk.
- **Elasticsearch requires a full reindex, not an incremental one, after the mapping/document-shape changes in Tasks 7/8**: Product's own history confirms this exact lesson the hard way — `docs/tasks/2026-07-27/Task15_product-search-missing-variation-name.md`'s "What wasn't done" section states plainly: "existing indexed documents won't retroactively gain [a new field] otherwise" after a mapping change; whoever deploys must trigger the rebuild endpoint. User's first-ever index build is even more fundamental (there's no pre-existing index at all) — the sequence must be: deploy Task 6-9's code → run `RebuildUserSearchIndex` once against production data → **then** cut `SearchUsers` over (Task 10) to read from it. Cutting over before the rebuild runs would serve an empty/incomplete index to real admin users.
- **Product's rebuild is a blocking drop+recreate** (`ElasticsearchIndexer.RecreateIndexAsync`, confirmed no blue/green alias swap exists anywhere in this codebase) — during a production rebuild, `SearchUsers` (if already cut over) would see a briefly empty or partial index. For User's *first* rebuild this doesn't matter (nothing has cut over yet); for any *future* rebuild after User is live in production, this is a real, documented, accepted limitation inherited from Product (not something this task should try to fix — matching Product's current scope, not scope-creeping into blue/green indexing, which `docs/reference/search.md` itself flags as a deliberately deferred future extension).
- **Redis cache (Task 11/12) requires no migration at all** — it's populated lazily on first read; an empty cache at deploy time is the expected, normal state (a burst of cache misses immediately after deploy, no different from a cold cache after any Redis restart).
- **gRPC (Task 13/14) is purely additive to the proto** — no migration concern, only a coordinated-deploy concern (Task 13's own risk section covers this).

## Scope

- Write a short, explicit rollout runbook (this task's deliverable) covering the exact sequence: (1) deploy Task 1-5's code (name model + locale/display-name, safe to deploy standalone, fully backward compatible); (2) deploy Task 6-9's code (ES scaffolding + config, index not yet serving traffic); (3) run `RebuildUserSearchIndex` once against production; (4) verify document count/spot-check a few real records; (5) deploy Task 10 (cutover) only after step 4 passes; (6) deploy Task 11-15 (cache + gRPC) independently, any time, since they have no ordering dependency on the ES work.
- Confirm (don't just assume) that every existing user row will produce a valid `UserSearchDocument` post-migration — in particular, rows seeded before `MiddleName` existed default to `""`, which Task 7's `SearchName` generation must handle gracefully (empty middle name → no extra token, not a literal `"null"` or double-space artifact).
- Explicit rollback plan per the architecture doc's already-stated positions (Task 1 additive/reversible; ES path stays behind the old Postgres search until Task 10's cutover is verified; cache removal is a one-line DI change; gRPC additive) — this task's job is to confirm those individual rollback stories still hold when the pieces are deployed together, not to invent new ones.

## Dependencies

- **Depends on:** effectively all other tasks (this is a review/gate, not a code-writing task) — sequence it last, immediately before or as part of the production rollout, not as a design-time exercise disconnected from the actual implementation.
- **Blocks:** nothing in the task list itself, but functionally gates the go-live decision.

## Estimated complexity

Small (as a document/checklist) — the complexity is in verifying the other tasks' claims hold up together, not in writing new code.

## Risks

- The single biggest risk this task exists to catch: someone deploys Task 10 (ES cutover) before ever running Task 9's rebuild endpoint against production — resulting in `SearchUsers` returning empty results for real admin users on day one. Make the rebuild-then-cutover ordering an explicit, checked gate, not an assumption.
- If the team decides to keep the Postgres search path around indefinitely (Task 10's dual-path option) rather than fully cutting over, this task's rollback story simplifies (just flip back), but the ongoing double-maintenance cost should be named explicitly as an accepted trade-off, not a silent default.

## Completion checklist

- [x] Rollout runbook written (see "What was done" above) - not yet reviewed by the team, since no team review cycle occurred this session
- [x] Confirmed: pre-`MiddleName` rows produce clean, artifact-free `SearchName`/`DisplayName` values (verified by reading `UserSearchProjectionBuilder`, not by running against real data)
- [ ] **Not done - flagged as the top operational risk**: `RebuildUserSearchIndex` has not been run against any real data (no Docker/Elasticsearch stack exists in this workspace) - whoever deploys this branch must run it before the first real `SearchUsers` call
- [x] Rollback story re-confirmed for each phase - one material change from the original plan recorded above (Postgres search path was deleted, not kept as a fallback)
