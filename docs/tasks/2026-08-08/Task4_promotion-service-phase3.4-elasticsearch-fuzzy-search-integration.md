# Task 4: Promotion Service — Phase 3.4 Elasticsearch + Fuzzy Search Integration

**Status:** Done — fourth Phase 3 (Persistence) prompt, absorbing what the original roadmap planned as a separate top-level Phase 4 (Search Integration). Phase 3 remains in progress; no CQRS or migration exists yet.
**Category:** Search-infrastructure scaffolding for public Coupon discovery — index/document/indexer/repository/DI only, no CQRS, no endpoint, no business eligibility logic, per the phase's own explicit boundary.

## What was done

**Research (read-only, before any file was written):** read ProductSearch in full (`BuildingBlock.Search`'s generic `IElasticsearchIndexer<TDocument>`/`ElasticsearchIndexer<TDocument>` with its alias-based blue/green reindexing, `Product.Persistence/Contexts/Products/Search/*`, `Product.Application/Abstractions/Search/*`, DI registration, startup `EnsureIndexAsync` wiring in `Product.API/Program.cs`) as the phase's named primary reference, plus `User.Persistence/Contexts/Users/Search/*` for a second data point on fuzzy search and tenant handling specifically.

**Two real discrepancies found between the prompt's assumptions and actual precedent — both confirmed by reading the actual code, both reconciled with the architect rather than silently decided:**

- **Fuzzy search**: [docs/promotion-service/search/search-strategy.md](../../promotion-service/search/search-strategy.md) (written at Phase 0) claimed fuzzy search was "the same mechanism Product/User already use," but neither `ProductSearchRepository` nor `UserSearchRepository` has any `.Fuzziness(...)` call anywhere — both use a plain `.MultiMatch()` with no fuzziness. Since Phase 3.4's own brief names fuzzy discovery "the primary purpose of this integration" with concrete typo examples, the architect confirmed adding `.Fuzziness(new Fuzziness("AUTO"))` to `CouponSearchRepository`'s `MultiMatch` query on `code`/`name`/`translatedNames` — a standard, minimal Elasticsearch query parameter on the exact same Bool/MultiMatch shape Product/User already use, not a new abstraction or competing pipeline. This is the platform's first real fuzzy-search implementation.
- **Tenant isolation**: neither `ProductSearchDocument` nor `UserSearchDocument` carries a tenant field or filter, even though both source entities (`Product`, `UserProfile`) implement `ITenantEntity` — a pre-existing gap in those two services. The architect confirmed adding a `TenantId` field to `CouponSearchDocument` with a mandatory (never optional/skippable) `Term` filter in `CouponSearchRepository`, using the same filter mechanism already applied to `Status`/`Visibility` — closes the gap for Promotion going forward without touching Product/User (out of this task's scope).

**Coupon search document design** (`CouponSearchDocument`, `Promotion.Application/Abstractions/Search/`): `CouponId`, `Code`, `Name`, `Description`, `TranslatedNames` (flattened `CouponTranslation.Name` values across languages — no generic Translation search service or index, per the phase's explicit constraint), `Status`, `Visibility`, `StartTime`/`EndTime`/`TimeZone`, `IsEnabled`, `TenantId`, `UpdatedAt`. Deliberately excludes `PromotionId`/`CampaignId`/`BatchId` (FK linkage not needed for the stated discovery use cases), `MaxUsage`/`MaxUsagePerUser`/`CurrentUsage` (internal accounting, explicitly excluded by the phase), and `CouponType` (not called for in the phase's field list) — all addable later as a plain new field, no redesign, matching Product's own documented extension pattern.

**Index mapping** (`CouponSearchIndexMapping`): `Code`/`Name` as `Text` with a `keyword` sub-field (the same multi-field technique ProductSearch already uses for its own `Name`, applied to `Code` too so it supports both fuzzy full-text matching and exact/sort access); `TranslatedNames` as plain `Text`; `Status`/`Visibility`/`TenantId` as `Keyword`; `StartTime`/`EndTime`/`UpdatedAt` as `Date`; `TimeZone` as a non-indexed `Keyword` (display-only, matching Product's `Thumbnail` `Index(false)` pattern); `IsEnabled` as `Boolean`. No custom analyzer added — Product (the phase's stated primary reference) has none; User's accent-insensitive analyzer was consciously not pulled in, since Product is the named template.

**Repository query** (`CouponSearchRepository`): same `Bool`/`Must`/`Filter` composition shape as `ProductSearchRepository`/`UserSearchRepository`. Mandatory `TenantId` Term filter always applied (not conditional). Keyword search via `MultiMatch` + `Fuzziness("AUTO")` across `code`/`name`/`translatedNames`. Optional `Status`/`Visibility` Term filters. Optional `AvailableAsOf` date-range filter (`StartTime <= AvailableAsOf <= EndTime`) supporting a future "currently published" query without embedding that business definition into this infrastructure layer — the caller decides what "published" means, this just exposes the raw filter.

**Infrastructure wiring**: `Promotion.Persistence.csproj` referenced `BuildingBlock.Search`; `Promotion.Persistence/DependencyInjection.cs` gained `AddPromotionSearchServices` (calls `AddElasticsearchClient` + registers `ICouponSearchIndexer`/`ICouponSearchRepository`), chained onto the end of `AddPersistence` exactly where Product chains `AddProductSearchServices`; `Promotion.API/Program.cs` gained the startup `EnsureIndexAsync()` call wrapped in try/catch (index failure degrades search, never blocks API boot — identical to Product's own startup block); `.env.template` gained `PROMOTION_ELASTICSEARCH_URL`; `docker-compose.override.yml`'s `promotion-api` service gained the `Elasticsearch__Url`/`Username`/`Password` environment block, matching Product/User's exact 3-line shape.

**Deliberately not built** (per the phase's explicit CQRS/business-logic exclusion): the projection builder (Domain `Coupon` → `CouponSearchDocument` mapping — `ProductSearchProjectionBuilder`'s equivalent lives under `Product.Application/Features/Products/Search/`, a CQRS-adjacent Features folder), the sync/rebuild domain events and their handlers (`OnCouponSearchSyncRequired`/`OnCouponSearchRemovalRequired`), the `RebuildCouponSearchIndex` command, and the public search Query/endpoint. `ICouponSearchIndexer`/`ICouponSearchRepository` are the ready extension points those future handlers will call once Phase 5 (CQRS) begins.

**API discovery note (not guessed, verified via reflection):** the installed `Elastic.Clients.Elasticsearch` 8.19.0 client's `QueryDescriptor<T>` has no direct `.DateRange(...)` method — date-range filtering goes through `.Range(r => r.DateRange(dr => dr.Field(...).Lte(...)))`, discovered by probing the actual installed package assembly with a throwaway console app (Windows PowerShell 5.1's desktop CLR can't load the netcore assembly for in-session reflection, so a real `dotnet run` probe was used instead) rather than guessing at the fluent API shape.

**Build**: one `dotnet build` of `Promotion.API.csproj` after the full search layer landed, per the phase's "one relevant build only" policy — first attempt surfaced 2 real compile errors on the date-range query (the `.DateRange` API mismatch above), fixed after the reflection probe, then rebuilt. **Final build succeeded, 0 errors** (28 total warnings — the same pre-existing 26 `CS9113` warnings on intentionally-empty Phase 3.2/3.3 services, plus 2 duplicate `NU1510` restore-warning lines, nothing new or concerning).

## Objective

Add Elasticsearch-backed public Coupon search infrastructure — document, index mapping, indexer, query repository, DI, startup wiring — cloning ProductSearch's established architecture exactly, without inventing a competing search pipeline and without implementing CQRS, the search endpoint, or any Coupon business/eligibility logic.

## Scope

**Built/changed this task:** 4 Application abstraction files (`CouponSearchDocument`, `CouponSearchCriteria`, `ICouponSearchIndexer`, `ICouponSearchRepository`), 4 Persistence files (`CouponSearchIndexNames`, `CouponSearchIndexMapping`, `CouponSearchIndexer`, `CouponSearchRepository`), `Promotion.Persistence.csproj`/`DependencyInjection.cs` updates, `Promotion.API/Program.cs` startup wiring, `.env.template`/`docker-compose.override.yml` config wiring, 1 search-strategy doc rewrite section, 2 other doc updates.

**Explicitly not built:** projection builder, sync/rebuild domain events and handlers, rebuild command, search Query/handler, search API endpoint — all Phase 5 (CQRS) work.

## Dependencies

Phase 3.1-3.3 (`2026-08-08`) — this task's `CouponSearchDocument` is built from the `Coupon`/`CouponTranslation` entities and enums those phases finalized. Phase 5 (CQRS Skeleton) depends on `ICouponSearchIndexer`/`ICouponSearchRepository` existing before it can wire a search Query handler.

## Estimated complexity

Medium — a bounded, single-aggregate search slice (8 new files + wiring), but required two real architecture-gap reconciliations with the architect and one live API-signature discovery before the code compiled.

## Risks

- The fuzzy-search and tenant-isolation additions are the first departures from literal ProductSearch/UserSearch precedent introduced by this phase — both documented explicitly in [search-strategy.md](../../promotion-service/search/search-strategy.md) so a future cross-service review recognizes them as intentional corrections, not inconsistency. Product/User's own tenant-isolation gap remains unfixed - flagging it here doesn't fix it there.
- `search-strategy.md`'s Phase 0 draft asserted fuzzy search was already proven elsewhere in the platform; that claim was factually wrong and is now corrected in the doc itself so it stops misleading future phases.
