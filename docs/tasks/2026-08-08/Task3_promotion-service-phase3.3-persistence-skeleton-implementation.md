# Task 3: Promotion Service — Phase 3.3 Persistence Skeleton Implementation

**Status:** Done — third Phase 3 (Persistence) prompt. Phase 3 remains in progress; no CQRS or migration exists yet.
**Category:** Convention verification + one real correction to the Phase 3.2 skeleton — no new structure, per this phase's own "do not spend time on a large audit" instruction.

## What was done

- Read [docs/conventions/persistence-coding-conventions.md](../../conventions/persistence-coding-conventions.md) — the platform's settled Read/Write persistence-service shape, explicitly built from and binding on 7 reference services (User, Audit, Auth, Inventory, Product, Notification, Order), naming Promotion as "service #8" to follow it. **Payment is not one of the 7** — it predates this convention's finalization.
- Compared it against what Phase 3.2 actually built (which had cloned `Payment.Persistence`'s literal shape as its template) and found one real deviation: the convention doc requires `<Aggregate>ReadService` to inject `TDbContext` directly and stay **completely independent of the repository** ("it never delegates to it, even when a query shape happens to duplicate something the repo could theoretically answer"). Phase 3.2's 11 `{Root}ReadService` classes instead injected `I{Root}Repository`, matching `PaymentReadService`'s actual code rather than the documented convention.
- Fixed all 11 Read Service constructors (`Campaign`, `Promotion`, `Coupon`, `Voucher`, `LoyaltyProgram`, `RewardProgram`, `DistributionJob`, `RecommendationProgram`, `ProductSet`, `GiftProgram`, `ApprovalWorkflow`) to inject `PromotionDbContext` instead of the repository interface, and swapped the now-unneeded `...Repositories` using directive for `...Engine` (the `PromotionDbContext` namespace). Write Services were left unchanged — the doc's Write Service responsibility section confirms injecting the repository (optionally alongside `IUnitOfWork` for the simple self-committing case) is correct there.
- Verified everything else Phase 3.2 built already matches the convention doc with no changes needed: empty `I{Aggregate}Repository` marker interfaces (kept only so Scrutor has a stable specific-interface slot), `<Aggregate>Repo`/`I<Aggregate>Repository` vs. full-word `<Aggregate>ReadService`/`I<Aggregate>ReadService` naming asymmetry, interface ownership split (Read/Write interfaces in `Application/Abstractions/Persistence/<Aggregate>/`, repository interfaces entirely inside `Persistence`), plural per-aggregate folder segments, and the DI registration shape (`AddScopedByInterface(typeof(IRepository<>), ...)` + explicit per-aggregate Read/Write registrations).
- Confirmed the established Persistence flow this phase asked me to state: **Minimal API → Request/Response → `ISender` → CQRS Command/Query → Application Handler → Read/Write Persistence Service → Repository → EF Core/DbContext → UnitOfWork → PostgreSQL.** Every layer up to and including UnitOfWork/DbContext is now wired for all 11 Aggregate Roots; CQRS/Minimal API layers remain Phase 5/6 work.
- No new structural decisions, no Domain changes, no DbContext/UnitOfWork/tenant/audit/concurrency changes — all were already correct from Phase 3.1/3.2 and confirmed by reading, not rebuilt.

**Documentation:** added a short correction note to [../../promotion-service/persistence/persistence-strategy.md](../../promotion-service/persistence/persistence-strategy.md)'s "Read Service" role definition explaining the Payment-vs-convention-doc distinction, so a future session doesn't reintroduce the same deviation by copying Payment again. Updated `docs/promotion-service/README.md` and `docs/promotion-service/planning/PROGRESS.md`.

**Build**: one `dotnet build` of `Promotion.API.csproj` after the 11-file correction, per the phase's "one relevant build" policy. **Succeeded, 0 errors** — same 26 expected `CS9113` warnings as Phase 3.2, now pointing at the `dbContext` parameter instead of the repo parameter.

## Objective

Verify Phase 3.2's persistence skeleton actually matches the project's established, documented Read/Write persistence-service convention (not just Payment's precedent, which Phase 3.2 had used as its literal template) and correct any real deviation found — without a large repository-wide audit and without adding any new structure.

## Scope

**Built/changed this task:** 11 file edits (Read Service constructors), 1 doc correction note, 2 other doc updates.

**Explicitly not built:** any new repository, service, or DI registration (Phase 3.2 already created the full skeleton) — this phase only verified and corrected it.

## Dependencies

Phase 3.2 (Task 2, `2026-08-08`) — this task corrects that phase's output. Phase 5 (CQRS Skeleton) depends on the Read Service's `PromotionDbContext` dependency being correct before real query methods are added to it.

## Estimated complexity

Small — a targeted, convention-driven correction to 11 already-existing files, not new implementation.

## Risks

- None new. The correction closes the one gap between Phase 3.2's Payment-copied shape and the platform's actual documented convention; no other gap was found.
