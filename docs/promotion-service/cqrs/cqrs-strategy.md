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

## What Phase 4.1 does not do

No business rule (discount calculation, eligibility evaluation, redemption logic) is inferred into a handler. `CreateCouponHandler`/`GetCouponHandler` are structural stubs (see "Phase 4.1 — built" above) — real workflow logic is explicitly out of scope until a future prompt requests it, same boundary Payment's `CreatePayment`/`CreatePaymentIntent`/`CreateRefund` precedent set for its own feature rollout.
