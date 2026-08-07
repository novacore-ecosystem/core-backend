# Promotion Service — Entity Configuration Conventions

**Scope:** The binding rules `Promotion.Persistence/Configs/*.cs` follow, frozen during Phase 3.1 (Entity Configuration + Domain Correction). Grounded in the actual EF Core conventions already used by Payment/Order/Product's own `Configs/` folders — nothing here invents a new platform pattern.

## File/folder convention

One file per entity: `Promotion.Persistence/Configs/{Entity}Config.cs`, `public sealed class {Entity}Config : IEntityTypeConfiguration<{Entity}>`. No per-entity base config class — shared behavior comes from static extension methods on `EntityTypeBuilder<T>`: `ConfigureCommonFields()` (audit timestamps + `xmin` concurrency token, mutable aggregates/children) or `ConfigureAuditFields()` alone (append-only logs — History/Audit/Execution-style entities, no concurrency token). `ApplyConfigurationsFromAssembly` is already called centrally in the shared `DbContextBase.OnModelCreating` — `PromotionDbContext` never overrides `OnModelCreating` itself, it only declares `DbSet<T>` properties.

Configure method body order: `// Table` → `// Properties` → `// Relationships` → `// Indexes`, matching every existing `Configs/` file in the platform.

## Primary key policy

**Every entity keeps a `Guid` primary key** (or a composite key, see below) — reviewed per Phase 3.1's own primary-key-strategy checklist (aggregate root vs. child, external identity, lifecycle, distributed-identity need), but `long`/`int` surrogate keys were **not** adopted anywhere: zero instances exist across Payment/Order/Product's combined ~80 entity configs, and introducing one in Promotion alone would be a new pattern, not a correction. `Guid` values are generated in the Domain (`Guid.CreateVersion7()` inside each `Create` factory), so no config ever calls `ValueGeneratedNever()` — the value is already non-empty by the time EF sees it.

## Translation entities — composite key, no surrogate Id

**Corrected in Phase 3.1.** Every `{Entity}Translation` entity's identity is `{Parent}Id + LanguageCode`, not a reused/aliased `Id`. Concretely:
- The entity inherits plain `BaseEntity` (not `BaseEntity<Guid>`) — it carries no independent identity column.
- It declares an explicit `{Parent}Id` property (e.g. `CouponTranslation.CouponId`) and a `{Parent}` navigation (e.g. `Coupon Coupon`).
- EF config: `builder.HasKey(x => new { x.CouponId, x.LanguageCode });`, `builder.HasOne(x => x.Coupon).WithMany(c => c.Translations).HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Cascade);`.

This is a deliberate departure from the pre-existing `ProductBrandTranslation` precedent (`Product.Persistence/Configs/ProductBrandTranslationConfig.cs`), which reuses the parent's own `Id` as both the PK component and the FK (`HasKey(x => new { x.Id, x.LanguageCode })`, `HasForeignKey(x => x.Id)`) rather than a distinctly-named `BrandId`. Phase 3.1's own prompt was explicit and repeated on this point — an explicit, separately-named parent FK — so Promotion Service's Translation entities diverge from `ProductBrandTranslation` on this one dimension while keeping everything else (composite `(ParentId, LanguageCode)` key, single upsert `Translate(...)`, no generic translation abstraction) identical to platform convention.

## Mapping (pure join) entities

An entity that exists only to represent a relationship between two local entities, with no independent lifecycle, gets a composite key of its two FK components and **no surrogate `Id`** — same as `OrderTag` (`HasKey(x => new { x.OrderId, x.TagId })`). `PromotionExclusion` (`PromotionId` + `ExcludedPromotionId`, both self-referencing `Promotion`) is this service's example.

Real child entities with their own data/lifecycle/history (e.g. `PromotionRule`, `CouponUsage`) keep a normal `Guid Id` — composite keys are reserved for Translation and pure-mapping entities only, never applied to every child indiscriminately.

## Local vs. external navigation

A navigation property is added only when the FK target is another entity **inside `Promotion.Domain`**. FKs referencing another microservice's data (`UserId`, `ProductId`, `VariantId`, `OrderId`, `WarehouseId`, `OperatorId`, `ReviewerId`, and similar) stay scalar-only — never a local navigation, never a cross-service EF relationship. Genuinely polymorphic references (`PromotionAudit.AggregateId`, `RuleAudit.RuleId`, `ExecutionAudit.ExecutionId` — could point at more than one entity type depending on context) also stay scalar-only even though the *target* is local, since a single typed navigation can't represent "one of several possible types."

## Bidirectional navigation (Phase 3.1 correction)

Every FK to a local entity now gets a forward reference navigation on the FK-holding side, and the referenced parent gets the matching reverse `ICollection<T>` where the relationship is genuinely one-to-many. This is a broader navigation-property sweep than Phase 2.6's more conservative pass, which had deliberately left many FK-only relationships (`CouponUsage→Coupon`, `RewardDistribution→RewardProgram`, etc.) one-directional on the reasoning that the existing pattern was already consistent platform-wide. Phase 3.1's own prompt made this an explicit, high-priority requirement ("ensure the corresponding navigation property exists... prefer bidirectional"), so the sweep was redone under that mandate. A small number of relationships were deliberately kept one-directional where a prior phase's reasoning still applies directly (e.g. `ApprovalStep→ApprovalWorkflow`, to preserve the documented uniformity of the Audits group) — see the per-aggregate docs for any such exceptions.

## Enum underlying type + conversion

Every entity-related enum explicitly declares `: byte` (none of this service's enums has anywhere close to 256 values). EF configuration widens by one integer size on conversion — `HasConversion<short>()` — matching Payment Service's own `byte` enum → `short` column convention (this project's explicit foundation-phase precedent). `IsRequired()` is chained for every non-nullable enum property.

## Value Object mapping

- **Single-scalar VOs** (`EntityCode`, `Currency`, `LanguageCode`, `Money`, `Quantity`, `PromotionPriorityValue`) are mapped as a plain `Property(...).HasConversion(vo => vo.Value, raw => Vo.Create(raw))` with `HasMaxLength`/`HasColumnType` as appropriate — never `OwnsOne`, matching this platform's `OrderNumber`/`PhoneNumber` precedent (`OwnsOne` is reserved for genuinely multi-column VOs).
- **`Period`** (`StartTime`/`EndTime`/optional `TimeZone` — 3 columns) is the one multi-column VO in this service, mapped via the shared `ValueObjectConfigurationExtensions.OwnsPeriod<TEntity>()` helper (`Promotion.Persistence/Configs/ValueObjectConfigurationExtensions.cs`), mirroring Payment's own `OwnsMoney` helper.
- `CouponUsageLimit` has no config — it is not currently used by any entity property (see [../value-objects/README.md](../value-objects/README.md)'s "reserved, not adopted" note); a config will be added if/when it is ever adopted.

## Indexes

Applied per the approved Domain design's index catalogue (see each aggregate's `## Indexes` section under [../aggregates/](../aggregates/)) — unique business codes, tenant+code combinations, status+time-window composites, and FK lookup indexes where a real query pattern justifies one. `TenantId` indexing is automatic (Entity Convention scan, see `ModelBuilderExtensions.ApplyEntityConventions`) and never configured per-entity.

## XML documentation style (Phase 3.1 correction)

Class/interface-level XML docs always use the multiline `/// <summary>\n/// ...\n/// </summary>` form, even for a single sentence — left open for future expansion without reformatting. Property-level docs stay single-line (`/// <summary>Maximum uses per user.</summary>`) unless the property genuinely needs multi-sentence explanation. Methods/constructors use multiline docs only when the name doesn't already say everything (parameters, non-obvious behavior, side effects) — an obvious method (`Enable()`, `Translate(...)`) gets no comment at all.
