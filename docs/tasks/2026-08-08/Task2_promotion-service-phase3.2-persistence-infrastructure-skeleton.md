# Task 2: Promotion Service — Phase 3.2 Persistence Infrastructure Skeleton

**Status:** Done — second Phase 3 (Persistence) prompt. Phase 3 remains in progress; no CQRS or migration exists yet.
**Category:** Persistence-layer scaffolding (repository + Read/Write Persistence Service skeleton, DI registration, audit hierarchy) — no business queries/commands, per the phase's own explicit "structure only" boundary

## What was done

**Repository granularity decision (blocking ambiguity, resolved with the architect before writing any files):**

- Computed the full ownership graph across all 103 `Promotion.Domain` entities (which entities are reached only through a parent's own `.Include()` vs. genuinely independent/unowned) via a dedicated research pass, cross-referenced against [../../promotion-service/aggregates/*.md](../../promotion-service/aggregates/). The mechanical "every unowned entity gets its own repository" rule (matching `Order.Persistence`'s loosest precedent, e.g. `ReturnStatusHistory`) would have produced **~76 repository classes** for this service, far more than any sibling service's ratio (Payment 3/27, Product 3/38, Order 7/18) — a direct consequence of Promotion's Domain (frozen since Phase 2) deliberately leaving most non-core entities unowned (public `Create`, id-only relation) rather than a signal that they all need independent query access.
- Presented three options to the architect (roots-only / roots-plus-independent-lookups / full mechanical rule) rather than silently picking one, given the 10x file-count spread between them. **Confirmed: repository + Read/Write Persistence Service only for the 11 true Aggregate Roots** — `Campaign`, `Promotion`, `Coupon`, `Voucher`, `LoyaltyProgram`, `RewardProgram`, `DistributionJob`, `RecommendationProgram`, `ProductSet`, `GiftProgram`, `ApprovalWorkflow`. Every other entity (owned children, and the ~47 flat/unowned entities) resolves through the generic `IRepository<TEntity, Guid>` binding when a future feature needs it, matching Payment/Product's tightest precedent and avoiding the "dozens of unused APIs" the phase's own prompt explicitly warned against.

**Persistence infrastructure skeleton (the phase's main deliverable):**

- 22 empty Application-layer interfaces: `I{Root}ReadService`/`I{Root}WriteService` under `Promotion.Application/Abstractions/Persistence/{Group}/` for all 11 roots — zero members each, matching the phase's own explicit example (`public sealed class PromotionWritePersistenceService : IPromotionWritePersistenceService { }` is acceptable).
- 44 Persistence-layer files under `Promotion.Persistence/Contexts/{Group}/`: `I{Root}Repository` (`IRepository<T, Guid>`, no extra methods), `{Root}Repo` (`PromotionBaseRepository<T, Guid>`), `{Root}ReadService`, `{Root}WriteService` (both empty impls) — one set per root, following the exact file/class-naming convention verified across Payment/Order/Product (`{Root}Repo.cs` filename with class `{Root}Repo`, confirmed via `ProductRepo`/`OrderRepo`/`PaymentIntentRepo`).
- `Promotion.Persistence/DependencyInjection.cs` rewritten: `AddRepositories()` keeps the existing generic `IRepository<>` assembly scan (unchanged) and adds 22 explicit `services.AddScoped<I{Root}{Read,Write}Service, {Root}{Read,Write}Service>()` registrations. `AddAuditHierarchy()` replaced its Phase-1 placeholder (`// TODO (Phase 2+): ...`) with the full registration for **all 103 entities** — `IsRoot(x => x.Id)` for the 11 roots plus the 47 flat/unowned entities (58 total), `BelongsTo<TParent>(x => x.ParentId)` for the 45 owned entities, using the exact FK property names verified via a full grep pass rather than assumed from memory. One entity (`PromotionExclusion`, a pure mapping row with no surrogate `Id`) uses a composite-tuple `IsRoot` selector (`x => new { x.PromotionId, x.ExcludedPromotionId }`) instead. This mirrors Payment Service's own exhaustive per-entity audit-hierarchy precedent (registers every `IAuditable` entity regardless of whether it has a repository yet), verified against the interceptor's actual gating logic (`AuditInterceptor.CollectAndEnqueueAuditEvents` requires both `IAuditable` and hierarchy registration — an unregistered `IAuditable` entity silently drops out of the audit trail rather than failing loudly).
- `UnitOfWork`, `PromotionBaseRepository`, `PromotionDbContext`, tenant/audit/concurrency infrastructure were all already correctly wired from Phase 1 (bootstrap) and Phase 3.1 (entity configuration) — confirmed by reading each file, no changes needed. No new UnitOfWork/transaction/caching abstraction was introduced anywhere.
- `Promotion.Application/GlobalUsings.cs` updated to import the 11 Domain entity-group namespaces + the `PromotionEntity` alias (previously commented out as "no entities exist yet") — needed for the new Read/Write service interfaces to reference root entity types.

**Zero speculative methods added** — every one of the 22 Read/Write service interfaces and their implementations has no members beyond the empty class/interface declaration, per the phase's explicit "do not create fake business APIs" instruction. The resulting 26 `CS9113` ("parameter is unread") build warnings on the empty service constructors are expected and intentional, not a defect.

**Documentation:** updated [../../promotion-service/persistence/persistence-strategy.md](../../promotion-service/persistence/persistence-strategy.md) — added a "Repository granularity" section recording the Phase 3.2 decision and reasoning, and rewrote "Phase mapping" to reflect that the repository/Read/Write skeleton now exists ahead of CQRS (previously documented as a Phase 5 deliverable) so Phase 5 only adds methods to already-created interfaces, not new ones. Updated `docs/promotion-service/README.md` and `docs/promotion-service/planning/PROGRESS.md`.

**Build**: single `dotnet build` of `Promotion.Persistence.csproj` after all files landed (per the phase's own "one appropriate build only" policy), then one more of `Promotion.API.csproj` to confirm the full transitive chain (Domain → Application → Persistence → Infrastructure → API) still compiles. **Both succeeded, 0 errors** (26 expected `CS9113` warnings, described above).

## Objective

Prepare the Persistence layer's structural skeleton — repository interfaces/base classes, Read/Write Persistence Service interfaces/implementations, DI registration, UnitOfWork/DbContext/audit integration — so future CQRS features (Phase 5) only need to add methods to already-existing interfaces, without inventing repository/service shape decisions mid-feature.

## Scope

**Built/changed this task:** 22 Application interface files, 44 Persistence files (11 roots × 4), `DependencyInjection.cs` rewritten (`AddRepositories` + `AddAuditHierarchy`), `Promotion.Application/GlobalUsings.cs` updated, 1 persistence-strategy doc section added + phase-mapping rewritten, 2 other doc updates.

**Explicitly not built:** any query/command method on any Read/Write service, any entity-specific repository beyond the 11 roots, CQRS handlers, API endpoints, search integration, EF Core migration, caching — this phase's own strict scope boundary.

## Dependencies

Phase 3.1 (Task 1, `2026-08-08`) — this task builds directly on its `PromotionDbContext`/`IEntityTypeConfiguration<T>` work. Phase 5 (CQRS Skeleton) depends on this task's repository/service interfaces existing.

## Estimated complexity

Large — 66 new files following a strict, repeated 4-file-per-root template, plus a 103-entity audit-hierarchy registration requiring exact FK property-name verification, and a consequential granularity decision requiring architect input before any file could be written.

## Risks

- The repository-granularity decision (roots-only, not the full 76-entity mechanical rule) is a deliberate scope choice, not a default — documented explicitly in `persistence-strategy.md` so a future phase doesn't mistake the missing non-root repositories for an oversight. If a future feature needs direct, repeated querying of a non-root entity beyond what its root's `Include()` can give it, that is the intended point to add that entity's own repository, not a signal this phase under-built.
- The audit hierarchy's `IdAccessor` (from `IsRoot(idSelector)`) was verified, by reading `AuditInterceptor`/`AuditHierarchyRegistry` source, to be currently unused at runtime (own-entity id always comes from EF's primary-key metadata instead) — this means `PromotionExclusion`'s synthetic composite-tuple `IsRoot` selector has no observable effect today, but keeping it type-accurate (rather than picking one arbitrary FK) avoids a latent trap if that field is ever wired up for a future purpose.
