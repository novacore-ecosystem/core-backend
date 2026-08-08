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

## What Phase 4.1/4.2 do not do

No business rule (discount calculation, eligibility evaluation, redemption logic, stacking, campaign/voucher/point/reward logic) is inferred into any handler. Coupon Management Foundation is deliberately just the administrative CRUD+translate lifecycle the Domain already exposes — real Promotion-engine workflow logic is explicitly out of scope until a future prompt requests it, same boundary Payment's `CreatePayment`/`CreatePaymentIntent`/`CreateRefund` precedent set for its own feature rollout.
