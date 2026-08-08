# Promotion Service — Search Strategy

**Scope:** The Elasticsearch integration Promotion Service adopts. Mirrors the platform's existing Product/User Elasticsearch architecture; does not invent a new search pattern. See [../../services/product-service.md](../../services/product-service.md) and the User Service ES build-out ([../../tasks/2026-07-28/Task6_elasticsearch-scaffolding.md](../../tasks/2026-07-28/Task6_elasticsearch-scaffolding.md) through [Task10](../../tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md)) as the model copied.

## Phase 3.4 — Coupon search infrastructure (built)

The first searchable resource is public **Coupon** discovery: `CouponSearchDocument` (`Promotion.Application/Abstractions/Search/`) + `CouponSearchIndexMapping`/`CouponSearchIndexer`/`CouponSearchRepository` (`Promotion.Persistence/Contexts/Coupons/Search/`), registered via `AddPromotionSearchServices` in `Promotion.Persistence/DependencyInjection.cs`, index-ensured at `Promotion.API` startup (non-fatal on failure, same as Product). Document fields: `CouponId`, `Code`, `Name`, `Description`, `TranslatedNames` (flattened `CouponTranslation.Name` values — no generic Translation search service, per this phase's own constraint), `Status`, `Visibility`, `StartTime`/`EndTime`/`TimeZone`, `IsEnabled`, `TenantId`, `UpdatedAt`. Deliberately excludes `PromotionId`/`CampaignId`/`BatchId`/usage-accounting fields (`MaxUsage`/`CurrentUsage`/etc.) and `CouponType` — not needed for the discovery use cases this phase scopes, addable later without redesign.

Two deliberate divergences from literal ProductSearch/UserSearch precedent, both reconciled with the architect rather than silently decided:

- **Fuzziness**: neither ProductSearch nor UserSearch actually implements fuzzy matching (`.Fuzziness(...)` doesn't appear in either's query construction, despite this doc's own now-corrected earlier claim otherwise) — verified by reading both repositories directly. Since this phase named fuzzy discovery its primary purpose, `CouponSearchRepository`'s `MultiMatch` query adds `.Fuzziness(new Fuzziness("AUTO"))` on the `code`/`name`/`translatedNames` fields — a standard, minimal Elasticsearch query parameter on the same Bool/MultiMatch shape Product/User already use, not a new abstraction. This is the platform's first real fuzzy-search implementation.
- **Tenant isolation**: neither `ProductSearchDocument` nor `UserSearchDocument` carries a tenant field, even though both source entities (`Product`, `UserProfile`) implement `ITenantEntity` — a pre-existing gap in those two services, not corrected here (out of this task's scope). `CouponSearchDocument` adds a `TenantId` field; `CouponSearchRepository` always applies it as a mandatory `Term` filter (never optional/skippable), using the same filter mechanism already applied to `Status`/`Visibility`.

Not built this phase (deliberately, per Phase 3.4's own CQRS/business-logic exclusion): the projection builder (Domain → `CouponSearchDocument` mapping), sync/rebuild domain events and their handlers, and the public search Query/endpoint — all of which require the CQRS layer Phase 5 introduces. `ICouponSearchIndexer`/`ICouponSearchRepository` are the ready extension points those future handlers will call.

## Components

- **Documents** — `PromotionSearchDocument`, a flat, denormalized projection of whatever fields Promotion's search/browse experience needs (name, description, status, date range, applicability). Built by a Projection Builder off the Domain aggregate, not a 1:1 mirror of the EF schema.
- **Index Configuration** — accent-insensitive/case-insensitive analyzer settings matching the platform convention already established for Product/User search (see [../../tasks/2026-07-28/Task7_search-document-and-accent-insensitive-mapping.md](../../tasks/2026-07-28/Task7_search-document-and-accent-insensitive-mapping.md)), plus a `RebuildPromotionSearchIndex` command mirroring [../../tasks/2026-07-28/Task9_rebuild-command-and-es-config.md](../../tasks/2026-07-28/Task9_rebuild-command-and-es-config.md) for blue/green reindex support.
- **Indexer** — sync events (internal domain events → projection → ES upsert/delete) triggered from the same transaction boundary as the aggregate write, following [../../tasks/2026-07-28/Task8_projection-builder-and-sync-events.md](../../tasks/2026-07-28/Task8_projection-builder-and-sync-events.md)'s shape.
- **Search Service** (`IPromotionSearchService`) — the query-side abstraction a Query handler depends on; wraps the ES client, never exposes NEST/Elastic.Clients types past the Infrastructure boundary.
- **Autocomplete** — prefix/edge-ngram-backed suggestion query for the admin/browse UI, same mechanism Product/User already use.
- **Fuzzy Search** — typo-tolerant matching (Levenshtein-distance fuzziness) on the primary text fields, same mechanism Product/User already use.

## Phase mapping

Phase 4 builds all of the above in isolation (indexable/searchable, no API route yet). Phase 5 is what wires a Query handler to `IPromotionSearchService` and exposes it through an endpoint — this strategy doc does not itself authorize adding that endpoint early.

## What this phase does not do

No cutover decision (ES-only vs. ES-plus-Postgres-fallback) is made here — that mirrors whatever decision the architect's design specifies; if unspecified, default to the platform's existing precedent (full cutover, per [../../tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md](../../tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md)) rather than inventing a hybrid approach.
