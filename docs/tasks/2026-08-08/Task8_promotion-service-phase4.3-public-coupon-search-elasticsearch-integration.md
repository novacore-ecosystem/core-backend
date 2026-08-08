# Task 8: Promotion Service — Phase 4.3 Public Coupon Search + Elasticsearch Integration

**Status:** Done — third Phase 4 (Application/CQRS/API) prompt. Phase 4 remains in progress; no Promotion-engine business logic (eligibility, discount calculation, stacking, campaign/voucher/point/reward workflows) exists yet.
**Category:** First real Elasticsearch-backed feature — connects Phase 3.4's search infrastructure to a real public Coupon discovery endpoint, using ProductSearch as the primary reference.

## What was done

**Application layer** (`Promotion.Application/Features/Coupons/Queries/SearchCoupons/`):
- `SearchCouponsQuery(Search?, SortBy?, SortDescending, Page, PageSize) : IQuery<PaginatedResult<SearchCouponsItemResponse>>` — only the fields `SearchProductsQuery` itself exposes (search text, sort, page), no speculative filters.
- `SearchCouponsHandler(ICouponSearchRepository)` — thin: builds `CouponSearchCriteria` and delegates to the existing `ICouponSearchRepository.SearchAsync` (built Phase 3.4). No Elasticsearch DSL, index name, analyzer, or fuzziness knowledge in the Handler, matching `SearchProductsHandler`'s exact shape.
- `Status`/`Visibility`/`AvailableAsOf` are fixed by the Handler — `nameof(CouponStatus.Active)`, `nameof(CouponVisibility.Public)`, `DateTime.UtcNow` — never taken from the request. Search answers "which Coupons can be found?", never "can this User apply this Coupon?" (eligibility), which stays out of scope per the phase's own Section 4.
- `TenantId` is read from `RequestContext.Current.TenantId` (`NovaCore.BuildingBlock.SharedKernel.Context`) — the same ambient mechanism `ModelBuilderExtensions`' EF tenant query filter and `TenantAssignmentInterceptor` already use. This is the first Application-layer Handler in the platform to read it directly; every prior Handler relied on EF's automatic query filter, which doesn't exist for Elasticsearch, so an explicit value was genuinely needed here for the first time. No Promotion-specific tenant resolution was invented — `RequestContext`'s own doc comment already names exactly this kind of caller as a legitimate consumer.

**Persistence layer** — one small, precedented extension to the existing Phase 3.4 file, `Promotion.Persistence/Contexts/Coupons/Search/Repositories/CouponSearchRepository.cs`: added an unconditional `isEnabled == true` term filter in `BuildBoolQuery`, alongside the existing unconditional `tenantId` filter. A disabled Coupon (`Coupon.Disable()`, `IsEnabled = false`) must never surface in public search regardless of caller-supplied criteria — same trust boundary as tenant isolation — and `CouponSearchCriteria` never exposed `IsEnabled` as a client-controllable field to begin with, so this stays a repository-level constant, not a new criteria field. No other Phase 3.4 file was touched: fuzzy code/name/translated-name matching (`.Fuzziness("AUTO")`), tenant isolation, and availability-window filtering (`StartTime`/`EndTime` vs. `AvailableAsOf`) all already existed and needed no changes.

**API layer** (`Promotion.API/Endpoints/Coupon/SearchCoupons.cs`): one new Carter endpoint, `GET /coupons/search` (deliberately distinct from `GET /coupons`, the Phase 4.2 administrative EF-backed `ListCoupons` endpoint — the two are different read models with different backing stores). `.RequireAuthorization()`, matching `SearchProductsEndpoint`'s own precedent (Product's "public" search endpoint also requires authentication — not actually anonymous).

**Mapster**: not used — `SearchCouponsHandler` maps `CouponSearchDocument` → `SearchCouponsItemResponse` via a plain `Select(...)` projection, matching `SearchProductsHandler`'s own precedent exactly (ProductSearch's item mapping is also manual, not `.Adapt<T>()`).

**Deliberately not implemented, per this phase's explicit scope**: any User/Order eligibility check, discount calculation, coupon stacking, redemption-limit validation, product/payment-method applicability, or per-user redemption state — all explicitly named as out of scope in the issuing prompt's Section 4. No CRUD/index synchronization (create/update/disable a Coupon does not yet sync into Elasticsearch) — deferred to a future event/outbox phase per Section 25, not invented as a temporary direct-indexing shortcut.

**Documentation**: added a "Phase 4.3 — Public Coupon Search + Elasticsearch Integration (built)" section to [cqrs/cqrs-strategy.md](../../promotion-service/cqrs/cqrs-strategy.md) (retitled "What Phase 4.1/4.2 do not do" → "What Phase 4.1/4.2/4.3 do not do"). Updated `docs/promotion-service/README.md` and `planning/PROGRESS.md`.

**Build**: one `dotnet build` of `Promotion.API.csproj` after the full feature landed, per the phase's "one build only" policy. **Succeeded, 0 errors**, no new warnings introduced (same pre-existing 24 from Phase 4.2, plus 2 `NU1510` restore-advisory warnings that appear on a non-incremental build).

## Objective

Connect the Phase 3.4 Elasticsearch search infrastructure and the Phase 4.2 Coupon Management foundation into a real public Coupon discovery feature (Minimal API → `SearchCouponsQuery` → `SearchCouponsHandler` → `ICouponSearchRepository` → Elasticsearch), using `ProductSearch` as the mandatory pattern reference, without redesigning the search architecture and without implementing Coupon applicability/eligibility.

## Scope

**Built/changed this task:** 2 new Application files (`SearchCouponsQuery.cs`, `SearchCouponsHandler.cs`), 1 new Minimal API endpoint (`SearchCoupons.cs`), 1 small edit to the existing Phase 3.4 `CouponSearchRepository.cs` (one unconditional filter), 1 CQRS strategy doc section, 2 other doc updates.

**Explicitly not built:** eligibility/discount/stacking/redemption logic, CRUD→Elasticsearch index synchronization, any new search abstraction, any change to `CouponSearchDocument`/`CouponSearchCriteria`/`CouponSearchIndexMapping`/`CouponSearchIndexer`.

## Dependencies

Phase 3.4 (Task 4, `2026-08-08`) — the search infrastructure this phase connects to. Phase 4.2 (Task 7, `2026-08-08`) — the Coupon Management foundation this phase's endpoint sits alongside.

## Estimated complexity

Small — the search infrastructure already existed in full; this phase is almost entirely a thin CQRS/API wiring pass plus one narrow, well-justified repository filter addition.

## Risks

- `GET /coupons/search` currently returns nothing meaningful against a real Elasticsearch cluster, since no CRUD→index synchronization exists yet (Section 25's deferred item) — the query/handler/endpoint chain is real and correct, but the index itself stays empty until a future event/outbox phase wires `CreateCoupon`/`UpdateCoupon`/`DisableCoupon`/`TranslateCoupon` to call `ICouponSearchIndexer`. Flagged here so a future session doesn't mistake an empty search result for a bug in this phase's own code.
- The unconditional `isEnabled == true` filter added to `CouponSearchRepository` is the only Phase 3.4 file this task touched — documented here and in `cqrs-strategy.md` so it isn't mistaken for scope creep into "redesigning the search architecture."
