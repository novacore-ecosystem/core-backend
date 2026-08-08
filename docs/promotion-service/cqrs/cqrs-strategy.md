# Promotion Service — CQRS Strategy

**Scope:** The CQRS shape every Promotion feature follows. Commits Promotion Service to the platform's existing CQRS conventions, binding in [../../conventions/application-coding-conventions.md](../../conventions/application-coding-conventions.md) and [../../04-coding-rules.md](../../04-coding-rules.md), with a copy-paste starting point in [../../06-implementation-templates.md](../../06-implementation-templates.md). Phase 4.1 built the first real, representative Coupon skeleton against this shape — see "Phase 4.1 — built" below.

## Phase 4.1 — Application / CQRS / Minimal API skeleton (built)

```text
Minimal API (Carter, ICarterModule)
    ↓
Request DTO
    ↓
ISender.Send(...)
    ↓
Command / Query
    ↓
Handler
    ↓
Read / Write Persistence Service
    ↓
Repository
    ↓
DbContext
```

One representative Command (`CreateCouponCommand` → `CreateCouponHandler`) and one representative Query (`GetCouponQuery` → `GetCouponHandler`) under `Promotion.Application/Features/Coupons/{Commands,Queries}/`, with matching Carter endpoints under `Promotion.API/Endpoints/Coupon/` (`POST /coupons`, `GET /coupons/{couponId}`). Both handlers inject `ICouponReadService`/`ICouponWriteService` (proving the DI/dependency chain resolves end-to-end) but their `Handle` bodies throw `NotImplementedException` behind a `// TODO:` comment — no real persistence call exists yet, since `ICouponReadService`/`ICouponWriteService` are still the empty Phase 3.2 interfaces and adding a method to them here would be exactly the speculative method this phase's own brief forbade. The global `IExceptionHandler` (already wired since Phase 1) converts that exception into a normal structured error response — no Promotion-specific error handling was added.

**New features clone this skeleton — the structure, not the stub bodies.** Add the aggregate's real fields to a new Command/Query, add the real method to its Read/Write Persistence Service, replace the `NotImplementedException` with the real call. Don't create a second Application/CQRS/API pattern.

**Mapster reconciliation**: the platform's only live precedent for Mapster is Entity→Response (`GetProductCategoryHandler`'s `category.Adapt<GetProductCategoryResponse>()`) — there is no existing Request→Command Mapster usage anywhere in the codebase to clone instead. Since Entity→Response mapping needs a real fetched entity, and no real persistence call exists in this skeleton (see above), no `.Adapt<T>()` call was added — fabricating one against a call path that never executes would be dead code, not a demonstration. `CreateCouponCommand`/`GetCouponQuery` are built with plain constructor calls instead (matching Payment's own `CreatePayment`/`GetPayment` endpoints, which also don't use Mapster). Mapster's DI/scan infrastructure (`AddMapster()`) has been wired since Phase 1 and needs no further registration once a real Entity→Response mapping is added alongside a real persistence method.

**Authorization**: both endpoints use `.RequireAuthorization()` (the generic authenticated-user requirement Payment/most `Get*` endpoints already use) rather than a specific permission constant like Product's `RequirePermissions(Permissions.Product.Manage)` — Promotion has no `Permissions.Promotion.*` constants defined yet, and inventing one would be exactly the invented-role/permission this phase's brief forbade.

## Per-feature shape

Every feature is a Feature-First folder under `Promotion.Application/Features/{Aggregate}/{Commands|Queries}/{FeatureName}/`, containing:

- **Commands** — `{Action}Command : ICommand<TResult>` (e.g. `CreatePromotionCommand`). One command = one write intent, no CRUD-generic command types.
- **Queries** — `{Action}Query : IQuery<TResult>` (e.g. `GetPromotionQuery`). Read-only, never mutates.
- **Validators** — `{Action}CommandValidator : AbstractValidator<TCommand>` (FluentValidation), colocated with the command/query it validates.
- **Handlers** — `{Action}Handler : ICommandHandler<TCommand, TResult>` / `IQueryHandler<TQuery, TResult>`. Thin orchestration only — persistence access goes through a Persistence Service (see [../persistence/persistence-strategy.md](../persistence/persistence-strategy.md)), not a raw `DbContext`.
- **DTOs** — `{Name}Dto`/`{Name}Result` per [04-coding-rules.md](../../04-coding-rules.md) naming, mapped from Domain via Mapster, never the Domain entity returned directly across the API boundary.
- **Contracts** — request/response shapes consumed by the API layer, kept separate from internal DTOs where the two diverge (matches the platform's existing Contract-vs-DTO split).
- **Persistence Services** — `I{Aggregate}ReadService`/`I{Aggregate}WriteService` the handler depends on; see [../persistence/persistence-strategy.md](../persistence/persistence-strategy.md) for the full split.
- **Repositories** — `I{Aggregate}Repository`, near-empty markers beyond `IRepository<T, TId>` unless the aggregate genuinely needs an extra query method (same precedent as Payment Service's `IPaymentRepository.GetByIdempotencyKeyAsync`).
- **API Endpoints** — one Carter endpoint per route under `Promotion.API/Endpoints/{Aggregate}/`, calling `ISender` only — no business logic in the endpoint itself.

## Scope discipline for future feature passes

Not every aggregate gets a full CQRS surface at once. Follow Payment Service's precedent: implement Create + Get for the aggregates a given feature pass marks in-scope; every other aggregate keeps its EF mapping (from Phase 3) but has no repository/handler/endpoint until a later prompt's design calls for it. This is a deliberate scope decision, not an oversight — record it the same way [../../services/payment-service.md](../../services/payment-service.md#persistence-readwrite-services) does.

## Phase 4.2 — Coupon Management Foundation (built)

The first real Promotion feature, implemented against the Phase 4.1 skeleton with no architecture change:

```text
Coupon Management
    ├── Create   — POST   /coupons
    ├── Get      — GET    /coupons/{couponId}
    ├── Update   — PUT    /coupons/{couponId}
    ├── Disable  — POST   /coupons/{couponId}/disable
    ├── List     — GET    /coupons
    └── Translate (upsert) — PUT /coupons/{couponId}/translations/{languageCode}
```

`Promotion.Application/Features/Coupons/{Commands,Queries}/` now has one folder per operation (`CreateCoupon`, `UpdateCoupon`, `DisableCoupon`, `TranslateCoupon`, `GetCoupon`, `ListCoupons`), each following the Phase 4.1 flow end to end — no stubs, real `ICouponReadService`/`ICouponWriteService` methods, real Domain calls:

- **Create** — `CreateCouponHandler` calls `Coupon.Create(...)` directly (the Handler constructs the aggregate, matching `CreateProductCategoryHandler`'s pattern — not `CreateAsync(primitives...)` building the entity inside the Persistence layer, which is Payment's different, older pattern) then `couponWriteService.CreateAsync(coupon, ct)` persists it.
- **Get** — `GetCouponHandler` fetches via `couponReadService.GetByIdAsync` (throws `NotFoundException` when missing) and maps via `.Adapt<GetCouponResponse>()` — Mapster's Entity→Response pattern is now genuinely exercised (unlike Phase 4.1's necessarily-unreachable stub), backed by a new `EntityCode`/`LanguageCode` → `string` `MapsterConfig : IRegister` under `Features/Coupons/Mapping/`.
- **Update** — `UpdateCouponHandler` loads then calls `couponWriteService.UpdateDetailsAsync(...)`, which invokes `Coupon.UpdateDetails`/`Reschedule`/`ChangeVisibility`/`ChangeUsageLimits` inside one `repo.UpdateAsync` callback, wrapped in `uow.ExecuteTransactionAsync` by the Handler (matches `UpdateProductCategoryHandler`'s transaction-ownership split). Deliberately does not touch `Status` or reassign `CampaignId`/`BatchId` — no status-transition method (`Activate`/`Approve`/...) is invoked here, and campaign/batch reassignment was scoped out as beyond "administrative foundation."
- **Disable** — `Coupon.Disable()` (`IsEnabled = false`), never a physical delete — Coupon has no `Delete()` method, and its lifecycle is designed to retain historical data. `Cancel()`/`Archive()`/other status transitions were **not** wired to any endpoint this phase — no requested operation mapped to them.
- **Translate** — one `TranslateCouponCommand`/endpoint, upserting via `Coupon.Translate(...)` (already upsert-shaped in the Domain) — no separate Create/Update translation operations.
- **List** — `ListCouponsQuery(Status?, Page, PageSize)` → `PaginatedResult<CouponSummaryResponse>`, matching `ListAuditLogsQuery`'s shape (the platform's plain EF/Mongo pagination pattern) — not Elasticsearch, which stays the separate public-search concern Phase 3.4 built.

**Deliberately not implemented, per this phase's explicit scope**: `PromotionId`/`CampaignId`/`BatchId` existence checks against their own aggregates (would require reaching into `IPromotionReadService`/`ICampaignReadService` outside this feature's own scope, and isn't literally required by the Domain model itself); Coupon-code uniqueness checking (explicitly named as forbidden business validation); any status-transition endpoint beyond Disable.

## Phase 4.3 — Public Coupon Search + Elasticsearch Integration (built)

Connects the Phase 3.4 Elasticsearch infrastructure to a real Application/API flow, cloning `SearchProductsQuery`/`SearchProductsHandler`/`SearchProductsEndpoint` exactly:

```text
Public Minimal API — GET /coupons/search
    ↓
SearchCouponsQuery
    ↓
SearchCouponsHandler
    ↓
ICouponSearchRepository (Phase 3.4)
    ↓
Elasticsearch
```

`SearchCouponsQuery(Search?, SortBy?, SortDescending, Page, PageSize)` — only the fields ProductSearch itself exposes for its own search+sort+page contract. `SearchCouponsHandler` builds `CouponSearchCriteria` and delegates straight to the existing `ICouponSearchRepository.SearchAsync` — no Elasticsearch DSL, index name, or analyzer knowledge in the Handler, matching `SearchProductsHandler`'s own shape. Fuzzy code/name/translated-name matching, tenant isolation, and availability-window filtering are all the Phase 3.4 `CouponSearchRepository` already implements — this phase adds no new search logic there beyond one filter (below).

**Search is not eligibility** (Section 4 of the issuing prompt): `Status`/`Visibility`/`AvailableAsOf` are fixed by the Handler to "publicly discoverable right now" (`CouponStatus.Active`, `CouponVisibility.Public`, `DateTime.UtcNow`) — never taken from the request, so a caller cannot ask to see Draft/Cancelled/Expired Coupons. `TenantId` comes from `RequestContext.Current.TenantId` (`NovaCore.BuildingBlock.SharedKernel.Context`), the same ambient mechanism EF's own tenant query filter and `TenantAssignmentInterceptor` already use — the first Application-layer Handler in the platform to read it directly, since this is the first time a Handler needs an explicit `TenantId` value rather than relying on EF's automatic query filter (Elasticsearch has no such filter). No Promotion-specific tenant resolution was invented.

**One small, precedented extension to `CouponSearchRepository`** (Phase 3.4): added an unconditional `isEnabled == true` filter, the same trust boundary as the existing unconditional `tenantId` filter — a disabled Coupon must never surface in public search regardless of what a caller supplies, and `CouponSearchCriteria` never exposed `IsEnabled` as a client-controllable field to begin with. This is the only Phase 3.4 file touched this phase.

No User/Order/Product/inventory eligibility, discount calculation, stacking, redemption-limit, or applicability logic was added anywhere — those stay Promotion Engine business logic for a future phase.

## Phase 4.4 — Coupon Lifecycle / Redemption Foundation (built)

```text
Coupon Discovery (Phase 4.3)
    ↓
Coupon Validation — GET /coupons/validate
    ↓
Coupon Redemption — POST /coupons/redeem
    ↓
CouponUsage (the existing Phase 2.2 redemption record)
```

**Validation** (`ValidateCouponQuery` → `ValidateCouponHandler`) is a read-only decision operation — never mutates. Returns `ValidateCouponResponse(CanProceed, Reason)`, a public-safe result: no database IDs, no exceptions, no internal state. A missing/invalid Coupon is a normal `CanProceed: false` answer (200 OK), not an error response — matching how a checkout flow expects to ask "is this code usable?" without treating "no" as a failure.

**Redemption** (`RedeemCouponCommand` → `RedeemCouponHandler`) is the state-changing counterpart — it establishes the Coupon's usage/claim state only, never a discount amount, order total, or stacking outcome (Promotion Engine logic, still out of scope). It reuses `Coupon.RecordUsage(userId, orderId)` — the Domain's own existing usage-recording method — and persists into `CouponUsage`, the aggregate's **existing** Phase 2.2 redemption entity (no new entity was created, per the issuing prompt's explicit instruction to stop and document rather than invent one if the record didn't already exist — it did).

**Validation and redemption share one rule set** (`CouponRedemptionEligibility.Evaluate`, `Features/Coupons/Shared/`): `IsEnabled`, `Status == Active`, the `StartTime`/`EndTime` window, `MaxUsage`, and `MaxUsagePerUser` — the only fields the Coupon Domain actually exposes for this (`Coupon.RecordUsage`'s own doc comment explicitly disclaims eligibility/limit enforcement, so this is genuinely an Application-layer responsibility, not Domain logic pulled up into the Handler). `RedeemCouponHandler` re-runs this same check live on every attempt rather than trusting a prior `ValidateCoupon` call, since a validation result can go stale between the two calls.

**Concurrency**: `Coupon` already carries PostgreSQL `xmin`-based optimistic concurrency (`CouponConfig.ConfigureCommonFields()`, wired since Phase 3.1) — no new concurrency token was added. `EfUnitOfWork.ExecuteTransactionAsync` already translates a `DbUpdateConcurrencyException` into `ConflictException`. `RedeemCouponHandler` wraps its whole "reload → re-check → mutate" sequence in a new `OptimisticConcurrencyRetry` (`Promotion.Application/Abstractions/Persistence/`, cloned from Inventory's own local helper of the same name and shape — not a shared BuildingBlock type, so each service that needs it has its own copy) so a losing concurrent redemption re-evaluates against the winner's committed state instead of blindly retrying stale data. Directly mirrors Inventory's `DeductStockHandler`.

**Idempotency, two layers, matching Payment's `CreatePayment` precedent**: the `RedeemCoupon` endpoint requires the platform's existing `Idempotency-Key` header (`.RequireIdempotency()`, the same HTTP-level middleware/store every idempotent write endpoint already uses) as the primary mechanism. As defense-in-depth once the HTTP cache's TTL passes, `RedeemCouponHandler` also checks for an existing `CouponUsage` matching the same (Coupon, User, Order) natural key and replays it instead of redeeming twice — this works whenever a caller supplies `OrderId` (the common case once OrderService integrates), and is a natural-key check rather than a synthetic `IdempotencyKey` field added to `CouponUsage`, since the Domain wasn't touched this phase.

**Errors**: no new exception types. `NotFoundException` (Coupon not found), `ForbiddenException` (no current user), and `BusinessRuleException` (Coupon not currently redeemable) — all pre-existing `BuildingBlock.Application`/`BuildingBlock.Domain` exception types, all already mapped by the existing `GlobalExceptionHandler`/`ExceptionHandlerHelper`. One small additive extension to the shared `MessageCode` enum (`BuildingBlock.Domain.Enums`): a new "Promotion Service (900-999)" range (`CouponDisabled`, `CouponNotActive`, `CouponUsageLimitReached`) — the same per-service-range convention every other service (Product 400s, Inventory 500s, Order 600s, ...) already uses, giving `BusinessRuleException` an accurate client-facing message instead of reusing an unrelated existing code.

**Deliberately not implemented, per this phase's explicit scope**: Coupon Reservation (`Coupon.AddReservation`/`ReleaseReservation` already exist on the Domain from Phase 2.2, but no Application feature wires them — the issuing prompt's own concrete requirements (Sections 5-26) and final-report checklist only named Validation and Redemption, not Reservation, so this stays an available-but-unused Domain capability, same kind of deliberate boundary Phase 4.2 drew around `AssignCampaign`/`AssignBatch`); no Elasticsearch sync from the redemption Handler; no Order/User service calls of any kind; no discount/stacking/eligibility-beyond-the-Coupon-itself logic anywhere.

## What Phase 4.1/4.2/4.3/4.4 do not do

No business rule (discount calculation, eligibility evaluation, promotion stacking, campaign/voucher/point/reward logic) is inferred into any handler. Coupon Management Foundation, Public Coupon Search, and Coupon Lifecycle/Redemption are deliberately just the administrative CRUD+translate lifecycle, the public discovery read path, and the validate/redeem lifecycle operations the Domain/Phase-3.4 infrastructure already expose — real Promotion-engine workflow logic is explicitly out of scope until a future prompt requests it, same boundary Payment's `CreatePayment`/`CreatePaymentIntent`/`CreateRefund` precedent set for its own feature rollout.
