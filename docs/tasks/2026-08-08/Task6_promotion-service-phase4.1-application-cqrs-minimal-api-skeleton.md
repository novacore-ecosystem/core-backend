# Task 6: Promotion Service — Phase 4.1 Application / CQRS / Minimal API Skeleton

**Status:** Done — first Phase 4 (Application/CQRS/API) prompt. Phase 4 remains in progress; no real Coupon feature/business logic exists yet.
**Category:** One representative CQRS/Minimal API flow (Command + Query) for Coupon — structural skeleton only, per the phase's own explicit boundary.

## What was done

**Research** (brief, no repository-wide audit per the phase's own instruction): read Payment's `CreatePayment`/`GetPayment` Command/Query/Handler/Endpoint pair and Product's `CreateProductCategory`/`GetProductCategory` equivalent, `BuildingBlock.Application.Abstractions.CQRS` (`ICommand`/`ICommandHandler`/`IQuery`/`IQueryHandler`, all thin `IRequest`/`IRequestHandler` wrappers over MediatR), `ApiResponse<T>`, `GlobalExceptionHandler`/`IExceptionHandler` (already wired since Phase 1), and `Promotion.API/DependencyInjection.cs`/`ApplicationPipeline.cs` (confirmed Carter module auto-discovery, MediatR, exception handling, auth, CORS all already fully wired from Phase 1 bootstrap — nothing missing to add at the infrastructure level).

**CQRS flow built** (`Promotion.Application/Features/Coupons/`):
- `Commands/CreateCoupon/CreateCouponCommand.cs` — `CreateCouponCommand(string Name) : ICommand<CreateCouponResponse>`; `CreateCouponResponse(Guid CouponId)`.
- `Commands/CreateCoupon/CreateCouponValidator.cs` — `AbstractValidator<CreateCouponCommand>`, one `NotEmpty` rule on `Name` (basic input validation, not a business rule — matches Payment/Product's own Create validators).
- `Commands/CreateCoupon/CreateCouponHandler.cs` — injects `ICouponReadService`/`ICouponWriteService`/`IUnitOfWork` (proves the DI chain resolves), `Handle` throws `NotImplementedException` behind a `// TODO:` comment describing the future real implementation shape.
- `Queries/GetCoupon/GetCouponQuery.cs` — `GetCouponQuery(Guid CouponId) : IQuery<GetCouponResponse>`; `GetCouponResponse(Guid Id, string Code, string Name, CouponStatus Status)`.
- `Queries/GetCoupon/GetCouponHandler.cs` — injects `ICouponReadService`, `Handle` throws `NotImplementedException` behind a `// TODO:` comment.

**Minimal API built** (`Promotion.API/Endpoints/Coupon/`):
- `CreateCoupon.cs` — `CreateCouponEndpoint : ICarterModule`, `POST /coupons`, `.RequireAuthorization()`, `[FromBody] CreateCouponRequest(string Name)` built into the Command via a plain constructor call, `ISender.Send(...)`, `Results.Created(...)` wrapping `ApiResponse<CreateCouponResponse>`.
- `GetCoupon.cs` — `GetCouponEndpoint : ICarterModule`, `GET /coupons/{couponId}`, `.RequireAuthorization()`, `[FromRoute] Guid couponId` → `GetCouponQuery`, `ISender.Send(...)`, `Results.Ok(...)` wrapping `ApiResponse<GetCouponResponse>`. Both auto-discovered by the existing `AddCarterModules(typeof(DependencyInjection), ...)` scan — no new registration needed. Removed the now-redundant `Promotion.API/Endpoints/.gitkeep` placeholder.

**Two real gaps found and reconciled (not guessed):**

- **No Request→Command Mapster precedent exists anywhere in the platform** — searched the full codebase (`request.Adapt<`/`Request.Adapt<`), found zero hits. The only live Mapster usage anywhere is Entity→Response (`GetProductCategoryHandler`'s `category.Adapt<GetProductCategoryResponse>()`), which requires a real fetched entity — impossible here since no method exists on `ICouponReadService` to fetch one (see below). Rather than fabricate a `.Adapt<T>()` call against a path that never executes (dead code, not a demonstration), both `CreateCouponCommand` and `GetCouponQuery` are built with plain constructor calls, matching Payment's own `CreatePayment`/`GetPayment` endpoints (which also don't use Mapster). This reconciliation, and the reasoning, is documented in [cqrs-strategy.md](../../promotion-service/cqrs/cqrs-strategy.md) so a future session doesn't treat the missing Mapster call as an oversight.
- **No `Permissions.Promotion.*` constants exist** — Product's own `CreateProductCategory` endpoint uses `.RequirePermissions(Permissions.Product.Manage)`, but inventing a Promotion-specific permission constant would be exactly the invented role/permission Phase 4.1's own brief forbade (Section 13). Both endpoints use the generic `.RequireAuthorization()` instead, matching Payment's `CreatePayment`/`GetPayment` and Product's own `GetProductCategory`.

**Persistence layer**: no changes. `ICouponReadService`/`ICouponWriteService` remain the empty Phase 3.2 interfaces — the phase's own brief explicitly named `GetCouponAsync()`/`CreateCouponAsync()` as forbidden additions, so neither handler calls anything real; both just prove the dependency resolves through DI.

**Documentation**: added a "Phase 4.1 — built" section to [docs/promotion-service/cqrs/cqrs-strategy.md](../../promotion-service/cqrs/cqrs-strategy.md) (the flow diagram, what future features clone, and the two reconciliations above), refreshed its stale "Phase 5" references to match the current phase numbering. Updated `docs/promotion-service/README.md` and `planning/PROGRESS.md`. Also corrected `planning/PROGRESS.md`'s own earlier "Overall Progress" arithmetic: the "X / 7" marker every issuing prompt uses turns out to be a **current-phase position indicator**, not a completed-phase count — confirmed once this prompt's own header read "Phase 4 / 7" while Phase 4 had nothing done in it yet, which only makes sense as "we are on phase 4 of 7," not "4 phases completed."

**Build**: one `dotnet build` of `Promotion.API.csproj` after the full skeleton landed, per the phase's "one build only" policy. **Succeeded, 0 errors** — 4 new expected `CS9113` "parameter is unread" warnings on the two stub handlers' unused constructor parameters (`CreateCouponHandler`: 3, `GetCouponHandler`: 1), on top of the pre-existing 28 from Phase 3's own intentionally-empty services.

## Objective

Establish one complete, representative Coupon CQRS/Minimal API flow (Command + Query, both directions of read/write) that future Promotion features clone verbatim in shape — without implementing any real Coupon business logic, without adding methods to the Persistence layer, and without inventing a competing Application/API architecture.

## Scope

**Built/changed this task:** 5 Application files (Command/Validator/Handler + Query/Handler), 2 API endpoint files, 1 GlobalUsings addition (`NovaCore.Promotion.Domain.Enums`), 1 stale placeholder removed (`Endpoints/.gitkeep`), 1 CQRS strategy doc section + refresh, 2 other doc updates.

**Explicitly not built:** any real Coupon business logic/validation/persistence method, any second Command/Query beyond the one representative pair, any DTO beyond what the two flows need, any Promotion-specific exception/middleware/authorization/tenant mechanism.

## Dependencies

Phase 3 (all of 3.1-3.5, `2026-08-08`) — this task's handlers depend on `ICouponReadService`/`ICouponWriteService` existing (even though unused). Future feature phases depend on this skeleton's shape being the one they clone.

## Estimated complexity

Small — a single representative feature slice (7 files), most of the effort spent reconciling two real gaps (Mapster precedent, permission constants) rather than writing code.

## Risks

- The `NotImplementedException` stub bodies are deliberate placeholders, not a defect — documented explicitly in `cqrs-strategy.md` so a future session replaces them with real logic rather than treating an unhandled 500 as broken infrastructure.
- The Mapster and authorization reconciliations are the two departures from literal Product precedent this task introduced — both documented so a future cross-feature review recognizes them as intentional, not inconsistency.
