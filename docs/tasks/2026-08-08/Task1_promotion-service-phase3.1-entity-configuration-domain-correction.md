# Task 1: Promotion Service — Phase 3.1 Entity Configuration + Domain Correction

**Status:** Done — first Phase 3 (Persistence) prompt. Phase 3 remains in progress; no repository, persistence service, CQRS, or migration exists yet.
**Category:** Persistence-layer scaffolding (EF Core entity configuration for all 103 entities) combined with targeted Domain corrections surfaced while configuring, per the phase's own explicit instruction

## What was done

**Domain corrections (made only because EF configuration required them, per this phase's explicit "combine Domain correction with Persistence configuration" instruction):**

- **Translation entity identity redesign** — all 10 `{Entity}Translation` entities (`CampaignTranslation`, `PromotionTranslation`, `CouponTranslation`, `VoucherTranslation`, `LoyaltyProgramTranslation`, `RewardProgramTranslation`, `GiftProgramTranslation`, `RecommendationProgramTranslation`, `ProductSetTranslation`, `ProductBundleTranslation`) previously reused the parent's own `Id` as their primary key (`Id = parentId`) — inserting a second language for the same parent would collide on that same `Id`. Corrected: each now inherits plain `BaseEntity` (no surrogate `Id`), declares an explicit `{Parent}Id` property, and exposes a `{Parent}` navigation; EF composite key is `(ParentId, LanguageCode)`. This is a deliberate departure from the pre-existing `Product.Persistence/Configs/ProductBrandTranslationConfig.cs` precedent (which reuses `Id` as both PK component and FK) — the current phase's prompt was explicit and repeated on requiring a distinctly-named parent FK, so Promotion Service's Translation entities now diverge from that one precedent while keeping everything else (composite key, single upsert `Translate(...)`, no generic abstraction) identical.
- **Enum underlying type** — all 32 Domain enums now declare `: byte` explicitly (previously implicit `int`), matching Payment Service's own foundation-phase convention (`byte` enum, widened to `short` on EF conversion via `HasConversion<short>()`).
- **Navigation-completeness sweep** — every entity with a `Guid`/`Guid?` FK to a local (same-service) entity now has a forward reference navigation, and the referenced parent has the matching reverse `ICollection<T>` where missing. This reverses Phase 2.6's more conservative stance (which left many FK-only relationships one-directional, reasoning that the existing pattern was already internally consistent) — the current phase's prompt made bidirectional navigation an explicit, high-priority requirement ("ensure the corresponding navigation property exists... prefer bidirectional"), so the sweep was redone under that mandate. A handful of relationships stayed one-directional where a still-applicable documented reason exists: `ApprovalStep.WorkflowId` (preserves the deliberate uniform one-directional pattern used by every other owned child in the project) and all four Audit entities' FKs (`PromotionAudit.AggregateId`/`RuleAudit.RuleId`/`ExecutionAudit.ExecutionId` are genuinely polymorphic; `ApprovalAudit.WorkflowId`, though concretely typed, was kept scalar-only to preserve the documented uniformity across the 4-entity Audit group).
- Four background research/execution agents ran the initial navigation sweep in parallel (grouped the same way as Phase 2.6's audit); three finished their full scope before a session rate limit interrupted them mid-task on the fourth (an automated failure notification, not a design decision) — the remaining ~40% of the sweep (all of Vouchers, most of Recommendations/ProductSets/Gifts/Approvals/Validations) was completed directly afterward, verified file-by-file against what the agents had already landed.

**Persistence layer (the phase's main deliverable):**

- Wrote `IEntityTypeConfiguration<T>` for all **103 entities** under `Promotion.Persistence/Configs/*.cs`, following the exact conventions already established by Payment/Order/Product's own `Configs/` folders (researched via a dedicated read-only agent pass before writing anything): `Guid` primary keys throughout (reviewed per the phase's own primary-key-strategy checklist — no `long`/`int` keys introduced, matching the zero-instances-found precedent across all three sibling services' ~80 combined entity configs), `HasConversion<short>()` on every enum property, `EntityCode`/`Currency`/`LanguageCode`/`Money`/`Quantity`/`PromotionPriorityValue` mapped inline via `HasConversion` (single-scalar VOs, matching the `OrderNumber`/`PhoneNumber` precedent — never `OwnsOne`), `Period` mapped via the new shared `ValueObjectConfigurationExtensions.OwnsPeriod<TEntity>()` helper (the one genuinely multi-column VO in this service, mirroring Payment's own `OwnsMoney`), composite keys for the 10 Translation entities and the one pure-mapping entity (`PromotionExclusion`), and `ConfigureCommonFields()`/`ConfigureAuditFields()` chosen per entity based on whether it has a real `Update`-shaped method (mutable) or is append-only.
- **Every relationship is configured exactly once**, from the FK-holding child entity's own config — an early pass accidentally declared several relationships from both the parent and child config (redundant, though EF Core tolerates it when consistent); cleaned up to match the platform's actual single-source convention before finishing the remaining groups, and fixed retroactively in the groups written first (Campaigns/Promotions/Coupons/Vouchers/Loyalty/Rewards/Distributions/Recommendations/ProductSets/Gifts/Approvals).
- `PromotionDbContext` now declares all 103 `DbSet<T>` properties, grouped by aggregate cluster with comment headers (matching Payment's `PaymentDbContext` style) — no `OnModelCreating` override needed, since `ApplyConfigurationsFromAssembly` is already called centrally in the shared `DbContextBase`.
- `Promotion.Persistence/GlobalUsings.cs` now imports every Domain entity/enum/Value-Object namespace, plus the `PromotionEntity` alias for the same root-namespace collision Domain's own `GlobalUsings.cs` already documents.

**Documentation:** wrote [docs/promotion-service/persistence/entity-configuration-conventions.md](../../promotion-service/persistence/entity-configuration-conventions.md), freezing every policy this phase established (Translation/mapping composite-key rules, local-vs-external navigation policy, enum underlying-type policy, Value Object mapping rules, primary-key strategy) for every future Phase 3 sub-prompt to follow without re-deriving it. Updated `Promotion.Domain/TODO.md`, `docs/promotion-service/README.md`, and `docs/promotion-service/planning/PROGRESS.md`.

**Build**: `dotnet build src/Services/Promotion/Promotion.Domain/Promotion.Domain.csproj` and `dotnet build src/Services/Promotion/Promotion.Persistence/Promotion.Persistence.csproj` — the phase's own "do not build after every entity, one appropriate build" policy was followed: Domain was built once after all Domain corrections landed, Persistence was built once after all 103 configs + DbContext wiring landed (plus one immediate follow-up build after fixing a single nullability warning). **Both succeeded, 0 errors.**

## Objective

Start Phase 3 (Persistence) by giving every Domain entity its EF Core mapping, correcting any structural gap the mapping work itself surfaces along the way (translation identity, enum types, navigation completeness) — without redesigning architecture, without starting repositories/CQRS/API/migrations.

## Scope

**Built/changed this task:** 10 Translation entities redesigned, 32 enum files, ~60 entity files touched for navigation additions (all 13 aggregate groups), 103 new EF config files + 1 new shared VO config helper, `PromotionDbContext` + `GlobalUsings.cs` rewritten, 1 new conventions doc, 3 other doc updates.

**Explicitly not built:** any repository, persistence service, CQRS handler, API endpoint, search integration, or EF Core migration — this phase's own strict scope boundary.

## Dependencies

Phase 2.6 (Task 4, `2026-08-07`). The rest of Phase 3 (repositories, persistence services once CQRS begins in Phase 5) depends on this entity-configuration layer.

## Estimated complexity

Very large — the biggest single task in this roadmap so far: 103 entities read and mapped, a full navigation-completeness sweep across the entire Domain, a Translation-entity identity redesign touching 10 aggregates, and two full project builds.

## Risks

- The Translation-entity identity redesign is a deliberate, intentional divergence from `ProductBrandTranslation`'s existing precedent elsewhere in the platform — flagged explicitly in the new conventions doc so it isn't mistaken for an inconsistency in a future cross-service review.
- The relationship-configuration duplication that had to be cleaned up mid-task (parent and child config both declaring the same FK relationship) was caught and fixed before the final build, but is a reminder that this scale of mechanical config-writing benefits from an explicit "single source per relationship" rule stated up front — now captured in the conventions doc for future phases.
- The Validation/Audit groups' missing `ITenantEntity` (flagged in Phase 2.6, re-flagged again there) remains unresolved — Phase 3.1 did not revisit this decision, since it is a Domain-shape question outside this phase's "EF configuration + surfaced corrections" scope, not a persistence-mapping concern.
