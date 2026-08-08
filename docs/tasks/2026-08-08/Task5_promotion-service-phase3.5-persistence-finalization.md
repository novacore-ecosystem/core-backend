# Task 5: Promotion Service — Phase 3.5 Persistence Finalization

**Status:** Done — fifth and final Phase 3 (Persistence) prompt. **Phase 3 is now fully closed.** No CQRS, migration, or search endpoint exists yet.
**Category:** Verification and close-out pass — no large audit, no new architecture, per the prompt's own explicit instruction.

## What was done

- Read the relevant existing documentation ([persistence/persistence-strategy.md](../../promotion-service/persistence/persistence-strategy.md), [search/search-strategy.md](../../promotion-service/search/search-strategy.md), [conventions/persistence-coding-conventions.md](../../conventions/persistence-coding-conventions.md), the current `Promotion.Persistence`/`Promotion.Application` structure) briefly, per the prompt's explicit "do not spend the majority of the prompt scanning unrelated files" instruction — no repository-wide audit was performed.
- Ran a baseline `Promotion.API` build **before** making any change to confirm the state left by Phase 3.4 was already clean (0 errors, 28 pre-existing expected warnings) — establishing there was nothing broken to fix.
- Verified project references stay minimal, per the phase's explicit boundary: `Promotion.Domain.csproj` references only `BuildingBlock.Domain`/`BuildingBlock.Application` (no EF Core/Elasticsearch/PostgreSQL); `Promotion.Application.csproj` references only `Promotion.Domain`/`BuildingBlock.Application`/`BuildingBlock.SharedKernel`/Mapster (no EF Core/Elasticsearch client either). Both already correct — no changes needed.
- Verified DI/UnitOfWork/DbContext/Search wiring built across 3.1-3.4 is complete and connected: `AddRepositories` (11 roots + generic scan), `AddUnitOfWork` (`EfUnitOfWork<PromotionDbContext>`, no new abstraction), `AddOutboxAndInbox`, `AddAuditHierarchy` (all 103 entities), `AddPromotionSearchServices` (`ICouponSearchIndexer`/`ICouponSearchRepository`) are all registered in `Promotion.Persistence/DependencyInjection.cs` and chained from `AddPersistence` — confirmed by reading the file, not by rebuilding it. **Nothing genuinely missing was found**, so no new repository/service/registration code was added this prompt (matches the phase's "only make changes required by the existing architecture" instruction — there was nothing to add).
- Confirmed Translation persistence (composite `ParentId + LanguageCode` key, entity-specific, no generic mechanism) and mapping-entity persistence (composite two-FK key for pure joins like `PromotionExclusion`, real child entities keep their own `Guid` key) are both unchanged since Phase 3.1 — no redesign, matching the phase's explicit "do not introduce a generic translation persistence mechanism" / "do not convert every child entity into a composite-key mapping table" constraints.
- No Domain changes were needed — the build was already clean, so the phase's "smallest possible correction if a compiler issue directly requires one" clause never triggered.

**Migration task update**: rewrote `Promotion.Persistence/Storage/Migrations/TODO.md` — it previously said migration generation should happen "when Phase 3 completes," which is now true but the phase's own instruction (Section 16) explicitly forbids running or generating a migration in this prompt. Reworded to state generation is deliberately deferred to an explicit future prompt (not auto-triggered by Phase 3 closing), still pointing at `PromotionDbContextFactory.cs`/Phase 7 as before. Does not require Docker/PostgreSQL running now.

**Documentation**: added a concise "Phase 3 close-out" section to [persistence/persistence-strategy.md](../../promotion-service/persistence/persistence-strategy.md) — the two final architecture diagrams (PostgreSQL→DbContext→Repository→Read/Write Service→Application, and Application→Search abstraction→Elasticsearch) plus a short bullet summary of Translation/mapping-entity/no-speculative-methods/ProductSearch-reference policy, without duplicating the detailed per-topic docs already written across 3.1-3.4. Updated `docs/promotion-service/README.md` and `planning/PROGRESS.md` (Phase 3 moved to Completed Phases; the phase renumbering from the original 8-item roadmap — old Phase 4 Search folded into 3.4, old Phase 5 CQRS now "Phase 4" — recorded, not silently absorbed).

**Build**: one final `dotnet build` of `Promotion.API.csproj`, per the phase's "one relevant build only" policy. **Succeeded, 0 errors**, same 28 warnings as the Phase 3.4 baseline — no regression, nothing to fix.

## Objective

Close Phase 3 (Persistence): verify the structure built across 3.1-3.4 is genuinely complete and connected, add only what's actually missing (nothing was), update the deferred-migration task record, and write a concise final Persistence documentation summary — without a large audit, without new architecture, without any business logic.

## Scope

**Built/changed this task:** 1 migration TODO rewrite, 1 doc section added (persistence-strategy.md close-out), 2 other doc updates (README.md, planning/PROGRESS.md). No source code changes — verification found nothing genuinely missing.

**Explicitly not built:** any repository/service/DI code (none was missing), any EF Core migration, any CQRS/Application/API code, any business logic — all per the phase's own explicit boundary.

## Dependencies

Phase 3.1-3.4 (`2026-08-08`) — this task verifies and closes out their combined output. Phase 4 (Application/CQRS/API Skeleton, renumbered from the original roadmap's Phase 5) depends on Phase 3's Persistence layer being stable, which this task confirms.

## Estimated complexity

Small — a verification/documentation pass with no source code changes, since the prior four Phase 3 sub-prompts had already left the Persistence layer complete and correctly wired.

## Risks

- None new. This task's own risk was scope creep (re-auditing everything already verified in 3.1-3.4) — avoided per the phase's explicit "do not perform a large repository-wide audit" instruction; verification was targeted at the specific checklist the phase itself listed (DI, project references, Translation/mapping policy, search wiring), not a fresh full review.
