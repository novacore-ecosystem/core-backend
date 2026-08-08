# Task 7: Promotion Service — Phase 4.2 Coupon Management Foundation

**Status:** Done — second Phase 4 (Application/CQRS/API) prompt. Phase 4 remains in progress; no Promotion-engine business logic (eligibility, discount calculation, stacking, campaign/voucher/point/reward workflows) exists yet.
**Category:** First real Promotion feature implementation — administrative Coupon CRUD + translate, using the Phase 4.1 skeleton and the existing Coupon Domain model verbatim.

## What was done

**Persistence Service extensions** (only the methods each of the 6 operations needs, per the phase's explicit "no speculative methods" constraint):
- `ICouponReadService` gained `GetByIdAsync(couponId, ct)` and `SearchAsync(status?, page, pageSize, ct)`.
- `ICouponWriteService` gained `CreateAsync(Coupon, ct)`, `UpdateDetailsAsync(...)`, `DisableAsync(couponId, ct)`, `TranslateAsync(couponId, languageCode, name, description, ct)`.
- `CouponReadService` implemented against `PromotionDbContext.Coupons` directly (`AsNoTracking`, `.Include(c => c.Translations)` for `GetByIdAsync`, `.Where`/`.Skip`/`.Take`/`.CountAsync` for `SearchAsync`) — stays independent of the repository, per the Phase 3.3 correction to `persistence-coding-conventions.md`.
- `CouponWriteService` implemented against `ICouponRepository`'s inherited generic `IRepository<Coupon, Guid>` members (`AddAsync`, `UpdateAsync` with an `Action<Coupon>` callback) plus `IUnitOfWork` (newly injected) — `CreateAsync`/`DisableAsync`/`TranslateAsync` self-commit via bare `SaveChangesAsync` (single simple mutations, matching `ProductCategoryWriteService.DeleteAsync`'s shape); `UpdateDetailsAsync` does not commit itself, since `UpdateCouponHandler` owns the `ExecuteTransactionAsync` call (matching `UpdateProductCategoryHandler`'s split).
- `ICouponRepository`/`CouponRepo` were **not** touched — every operation is satisfiable through the Read Service's direct `DbContext` access or the generic `IRepository<T,TId>` members already on the base class, so no entity-specific repository method was needed (Section 17's own preference: "prefer existing generic repository methods when they already satisfy the operation").

**CQRS features** (`Promotion.Application/Features/Coupons/`), one folder per operation:
- **`Commands/CreateCoupon/`** — rewrote the Phase 4.1 placeholder command to the real `Coupon.Create(...)` parameter set (`PromotionId`, `Code`, `Name`, `CouponType`, `StartTime`, `EndTime`, `TimeZone`, `Description?`, `Visibility`, `CampaignId?`, `BatchId?`, `MaxUsage?`, `MaxUsagePerUser?`). `CreateCouponHandler` constructs the `Coupon` aggregate itself via `Coupon.Create(...)` then calls `couponWriteService.CreateAsync(coupon, ct)` — matches `CreateProductCategoryHandler`'s "Handler constructs the aggregate, WriteService just persists it" shape, not Payment's older "WriteService constructs the aggregate from primitives" shape (Payment predates the finalized convention, per the Phase 3.3 finding). `CreateCouponValidator` checks only basic input shape (`NotEmpty`/`EndTime > StartTime`) — no business rules, no code-uniqueness check (explicitly named as forbidden in the phase's own brief).
- **`Queries/GetCoupon/`** — rewrote the Phase 4.1 placeholder query into a full administrative detail response (identity, code, status, visibility, type, dates, limits, current usage, enabled flag, translations, audit timestamps). `GetCouponHandler` now genuinely fetches and maps via `.Adapt<GetCouponResponse>()` — the Mapster gap Phase 4.1 flagged as "necessarily unreachable" is now real, backed by a new `EntityCode`/`LanguageCode` → `string` `MapsterConfig : IRegister` under `Features/Coupons/Mapping/` (mirrors `Product.Application/Common/Mapping/MapsterConfig.cs`'s pattern).
- **`Commands/UpdateCoupon/`** (new) — `UpdateCouponCommand(CouponId, Name, Description, StartTime, EndTime, TimeZone, Visibility, MaxUsage, MaxUsagePerUser)`. Handler loads (404 if missing) then wraps `couponWriteService.UpdateDetailsAsync(...)` in `uow.ExecuteTransactionAsync`, which internally calls `Coupon.UpdateDetails`/`Reschedule`/`ChangeVisibility`/`ChangeUsageLimits` in one `repo.UpdateAsync` callback. Deliberately does not touch `Status` (no status-transition method invoked) or `CampaignId`/`BatchId` reassignment (`AssignCampaign`/`AssignBatch` exist on the Domain but were scoped out as beyond "administrative foundation" — not requested by name in the phase's operation list).
- **`Commands/DisableCoupon/`** (new) — calls `Coupon.Disable()` (`IsEnabled = false`) only. Coupon has no `Delete()` method and its lifecycle is designed to retain historical data, so this maps directly to the phase's "prefer Domain state transition... do not physically delete" instruction. `Cancel()`/`Archive()`/other status transitions exist on the Domain but were **not** wired to any endpoint — "Delete/Disable" was the only operation named in scope.
- **`Commands/TranslateCoupon/`** (new) — one command/endpoint, `TranslateCouponCommand(CouponId, LanguageCode, Name, Description)`, calling `Coupon.Translate(...)` directly — the Domain method is already upsert-shaped (updates the existing translation for that language if present, otherwise adds one), so no separate Create/Update translation operations were built, per the phase's explicit instruction.
- **`Queries/ListCoupons/`** (new) — `ListCouponsQuery(Status?, Page = 1, PageSize = 20) : IQuery<PaginatedResult<CouponSummaryResponse>>`, mirrors `ListAuditLogsQuery`'s shape (the platform's plain EF/Mongo pagination pattern, reusing the existing `PaginatedResult<T>` — no new pagination abstraction). Explicitly not Elasticsearch-backed — that stays Phase 3.4's separate public-search concern, per the phase's own instruction not to duplicate search logic inside CRUD handlers.

**Minimal API** (`Promotion.API/Endpoints/Coupon/`): `CreateCoupon.cs` rewritten with the real request contract; `GetCoupon.cs` unchanged (already correct from Phase 4.1); four new endpoints — `UpdateCoupon.cs` (`PUT /coupons/{couponId}`), `DisableCoupon.cs` (`POST /coupons/{couponId}/disable`, matching `CancelOrder`'s action-suffix route convention), `ListCoupons.cs` (`GET /coupons`, mirrors `ListAuditLogsEndpoint`'s query-param shape), `TranslateCoupon.cs` (`PUT /coupons/{couponId}/translations/{languageCode}`, a REST upsert-by-key route — no existing platform precedent for a translation endpoint to clone, so this is the first one, designed to match the established action/resource route style rather than inventing something novel). All six use `.RequireAuthorization()` (no `Permissions.Promotion.*` constants exist yet, and inventing one was out of scope, matching the Phase 4.1 precedent).

**Documentation**: added a "Phase 4.2 — Coupon Management Foundation (built)" section to [cqrs/cqrs-strategy.md](../../promotion-service/cqrs/cqrs-strategy.md) (also fixed an unrelated duplicate heading left over from the Phase 4.1 edit). Updated `docs/promotion-service/README.md` and `planning/PROGRESS.md`.

**Build**: one `dotnet build` of `Promotion.API.csproj` after the full feature landed, per the phase's "one build only" policy. Two real compile errors were found and fixed along the way (`Promotion.Domain.Enums`/`ValueObjects` weren't yet globally imported in `Promotion.Application`/`Promotion.API`'s `GlobalUsings.cs` — added both). **Final build succeeded, 0 errors**, 24 total warnings — the 4 Phase 4.1 stub-handler `CS9113` warnings are gone now that those handlers do real work; one pre-existing `CS0618` obsolete-API warning from Phase 3.4's `CouponSearchRepository` (`DateRange(...)` → `Date(...)`) was left untouched as out of this phase's scope (not introduced here, not a Coupon-management concern).

## Objective

Implement the first real Promotion feature — the administrative Coupon management lifecycle (Create/Get/Update/Disable/List/Translate) — using the existing Domain model, Persistence infrastructure, and Phase 4.1 CQRS/Minimal API skeleton exactly as designed, without inventing new architecture and without expanding into Promotion-engine business logic.

## Scope

**Built/changed this task:** 2 Persistence Service interfaces extended, 2 Persistence Service implementations rewritten, 6 CQRS feature folders (4 new, 2 rewritten from Phase 4.1 placeholders) totaling 16 Application files, 1 new Mapster config, 6 Minimal API endpoint files (5 new, 1 rewritten), 2 `GlobalUsings.cs` fixes, 1 CQRS strategy doc section + cleanup, 2 other doc updates.

**Explicitly not built:** any Promotion-engine business rule, any status-transition endpoint beyond Disable, any FK-existence validation against Promotion/Campaign/Batch, code-uniqueness validation, any Elasticsearch wiring inside the CRUD handlers.

## Dependencies

Phase 4.1 (Task 6, `2026-08-08`) — this task fills in its skeleton's stub bodies and extends its pattern to 4 more operations. Phase 3 (all sub-phases) — the Domain/Persistence layer this feature is built on.

## Estimated complexity

Large — 6 full CQRS operations, 2 Persistence Service implementations, 6 API endpoints, all real (not stubs), requiring careful reconciliation of which Domain methods each operation should and should not invoke.

## Risks

- The Update/Disable scope decisions (no status transitions, no campaign/batch reassignment) are deliberate boundaries drawn from the phase's own operation list, not oversights — documented in `cqrs-strategy.md` so a future session doesn't treat the missing `Activate`/`Cancel`/`AssignCampaign` wiring as incomplete work.
- The `TranslateCoupon` route (`PUT /coupons/{couponId}/translations/{languageCode}`) has no prior platform precedent to clone exactly — it's this codebase's first translation endpoint, designed to match the established REST/action-route style rather than being copied verbatim from an existing file. A future entity's translation endpoint should clone this one directly rather than re-deriving the shape.
- The pre-existing `CS0618` obsolete-API warning in Phase 3.4's search code was left untouched (out of scope for this task) — flagged here so it isn't mistaken for something this task introduced or missed.
