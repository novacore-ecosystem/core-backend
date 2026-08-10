# Task 18: Documentation Updates

**Status:** Done (2026-07-28)
**Category:** Documentation

## What was done

All six docs named in this file's own scope were updated, plus one extra found while touching the frontend doc:

- `docs/services/user-service.md` — routes table (ES-backed `SearchUsers`, new `POST /users/search/rebuild`), a fully rewritten "User-specific building blocks" section (gRPC `GetUser`/`GetUsers`, Elasticsearch search, User Detail cache, locale-aware DisplayName, the two new self-consuming search-sync consumers), a note on `Roles` now also flowing into the ES document, and two new "Known issues" entries (`DeleteUserCommand`/`DeleteUserHandler` dead code, the inherited blocking-rebuild limitation).
- `docs/reference/search.md` — scope line and architecture diagram updated to include User as a second full implementation (no longer "Product... the only full implementation"); the `IElasticsearchIndexer` settings-aware overload noted; a full new "User Search" section covering the three deliberate differences from Product (custom analyzer, retired per-field name filters, four sync trigger paths) and the still-open operational risk (rebuild not yet run against real data).
- `docs/reference/caching.md` — new "User Detail cache" section, explaining the DTO-based deviation from the `Cached{X}ServiceDecorator` template (and why: `UserProfile`'s private setters) and pointing at the correct `CacheKeys.UserProfiles` group vs. the pre-existing dead `CacheKeys.Users`.
- `docs/reference/grpc.md` — "two call chains" corrected to three (Audit → User added); new "Batch RPCs: never a loop of single calls" section documenting `GetUsers`'s never-omit convention and Audit's fail-open, display-only consumer.
- `docs/reference/events.md` — one-sentence addition noting `UserProfileUpdatedIntegrationEvent` as a recent example of the "new event added purely for search sync" pattern.
- `NovaCoreUI/docs/backend/user/README.md` — endpoints table (rebuild endpoint), a new "Locale" section (the `Accept-Language` header already flows, no frontend change needed), the `SearchUsers` field-support changes, and `middleName`/`displayName` on the DTOs.
- **Extra, not originally listed**: `NovaCoreUI/docs/backend/auth/README.md`'s `RegisterRequest` DTO line — a one-line fix for the same `middleName` addition (Task 3), found while touching the sibling User doc.

Not done, out of this task's scope: NovaCoreUI's own `docs/frontend/i18n.md`/ADR (Frontend Task 5's concern, not this one), and no team review cycle occurred (this was a solo session).

## Objective

Keep `docs/` in sync as each piece of this epic lands — per this repo's docs-first convention, docs describe the settled shape of the system, not a stale snapshot from before the refactor.

## Current state (grounded findings — exact docs that will go stale)

- `docs/services/user-service.md` — currently documents User's routes table (no `MiddleName`, no search-rebuild endpoint, no new gRPC RPCs), the "User-specific building blocks" section (would gain: Elasticsearch search, User Detail cache, new gRPC read RPCs), and the "Denormalized Roles" section (unaffected, but adjacent — don't conflate). Needs the full set of additions this epic introduces.
- `docs/reference/search.md` — currently Product-only ("Product Search (the only full implementation today)"); needs a new "User Search" section once Tasks 6-10 land, following its own documented pattern of describing each service's search implementation as a repetition of the shared architecture — plus removing/updating the line that currently says Product is "the only full implementation."
- `docs/reference/caching.md` — currently documents only Auth's role cache and the Gateway's separate minimal path as concrete examples; needs a new "User Detail cache" example once Task 11/12 land, since this is the first time an *owning* service (not a cross-service read-only borrower) implements the full decorator pattern for its own entity outside of Auth.
- `docs/reference/grpc.md` — currently states "Two call chains today: Auth → User `CreateUserProfile`, and Order → Inventory" — needs updating once Task 13-15 land (a third call chain, plus User gaining its first read-oriented RPCs).
- `docs/reference/events.md` — if Task 8 adds `UserProfileUpdatedIntegrationEvent` (a genuinely new integration event, not present in the "Known gaps" or implementation-status lists today), it should be reflected wherever this doc enumerates integration events, consistent with how it already tracks additions like the Product Category/Tag events added for search sync.
- `NovaCoreUI/docs/backend/user/README.md` (the frontend repo's mirror of the User contract) — per the frontend research agent's findings, this doc already lists known TODOs (GetUserDetail route mismatch, roles/tempPassword optionality) but mentions nothing about `MiddleName`, locale headers, or Elasticsearch — needs a new section once the backend contract is finalized (coordinate with Frontend Task F6).

## Scope

- Update each doc listed above incrementally, as the corresponding backend task actually ships — not as one big end-of-epic doc dump, so docs never drift far from code for long.
- Follow each doc's existing structure/tone (e.g. `search.md`'s "reusable vs. service-specific" framing, `caching.md`'s "decorator pattern" section) rather than introducing a new documentation style for User's instance of an already-documented pattern.
- Do not create new standalone docs for concepts already covered by an existing reference doc (per this repo's docs-first rule — check before creating) — User's search/cache/gRPC additions are new *sections within* `search.md`/`caching.md`/`grpc.md`, not new files.

## Dependencies

- **Depends on:** the respective implementation tasks (this task tracks alongside, updates land with each phase, not after).
- **Blocks:** nothing functionally, but stale docs actively mislead future work (as this epic's own research repeatedly found pre-existing doc/code mismatches to correct) — treat this as equally load-bearing as the code changes it describes.

## Estimated complexity

Small, spread across the epic — a paragraph or section per doc, per phase.

## Risks

- If deferred to "after everything ships," the natural failure mode (per this repo's own history — e.g. `docs/tasks/2026-07-27/Task11_rebuild-search-index-auth-undocumented.md`, an auth requirement that existed in code but was undocumented for a while) is a repeat of exactly that gap for User's own rebuild endpoint (Task 9 already flags this explicitly and should document its own auth requirement from day one, not rely on this task to catch it later).

## Completion checklist

- [x] `docs/services/user-service.md` updated (routes, new building blocks, search/cache/gRPC sections)
- [x] `docs/reference/search.md` gains a "User Search" section, "only full implementation today" language corrected
- [x] `docs/reference/caching.md` gains a User Detail cache example
- [x] `docs/reference/grpc.md` updated with the new call chain(s) and User's first read RPCs
- [x] `docs/reference/events.md` updated for `UserProfileUpdatedIntegrationEvent`
- [x] `NovaCoreUI/docs/backend/user/README.md` updated (backend contract already landed this session, no need to wait for Frontend Task F6)
