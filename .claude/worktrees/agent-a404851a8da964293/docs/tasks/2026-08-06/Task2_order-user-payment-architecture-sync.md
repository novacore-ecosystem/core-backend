# Task 2: Sync OrderService & UserService with the new PaymentService boundary

**Status:** Done
**Category:** Architecture review + refactor (no new business logic), closes part of Task 1's Phase 7 thread

## What was done

Reviewed OrderService and UserService end-to-end for anything that now duplicates PaymentService's ownership (Payment lifecycle, Payment Accounts, Payment Methods, Payment Gateway, Payment Intent, Payment Attempts, Refunds, Billing Profiles, Payment Tokens, Gateway Configuration, Payment Sessions, Settlement, Reconciliation, Payment Events, Webhooks, Payment Notifications). Found exactly two duplicated-ownership entities (both already documented as "pre-existing placeholders" for PaymentService — see `docs/services/payment-service.md`) and slimmed both to lightweight references. PaymentService itself was not touched.

- **`Order.Domain.Entities.Orders.OrderPayment`**: removed `PaymentMethod`/`PaymentProvider`/`ProviderName`/`MaskedAccount`/`ReferenceNumber` (gateway/account detail, now PaymentService's `PaymentMethod`/`PaymentGateway`/`PaymentAccount`). Renamed `PaymentReferenceId` → `PaymentId`. Kept `PaymentStatus`/`PaidAmount`/`CurrencyCode`/`PaidAt` — Order's own workflow snapshot, needed by `Order.Cancel()`. Deleted `Order.Domain.Enums.PaymentMethod`/`PaymentProvider` (dead code after the field removal, confirmed zero other references). Updated `OrderPaymentConfig.cs`, generated migration `SlimOrderPaymentToPaymentServiceReference`.
- **`User.Domain.Entities.Users.UserPaymentMethod`**: removed `Provider`/`PaymentType`/`Token`/`ExternalCustomerId`/`ExternalPaymentMethodId`/`CardInformation`/`IsVerified` (all now duplicate PaymentService's `PaymentAccount`/`PaymentToken`/`CardInformation`). Added `PaymentAccountId` (reference to PaymentService's `PaymentAccount.Id`, unique). Kept `DisplayName`/`IsDefault`. Deleted `User.Domain.Enums.PaymentProvider`/`PaymentType` and `User.Domain.ValueObjects.CardInformation` (dead code, confirmed zero other references). Updated `User.cs`'s `AddPaymentMethod` signature, `UserPaymentMethodConfig.cs`, generated migration `SlimUserPaymentMethodToPaymentServiceReference`.
- **Documentation**: `docs/services/order-service.md`/`user-service.md` gained `OrderPayment`/`UserPaymentMethod` entity bullets (both were previously undocumented despite existing in code); `docs/services/payment-service.md`'s "Why PaymentService is independent" section updated to reflect the sync; new `docs/reference/payment-ownership-boundaries.md` (Responsibility Matrix, `ReferenceType`/`ReferenceId` convention, Service Boundaries, Payment Integration Strategy) linked from `docs/01-architecture-map.md` and `docs/README.md`.

Both `Order.API.csproj` and `User.API.csproj` build clean with these changes (verified after each layer). No Docker/runtime testing performed, per the task's explicit instructions.

## Objective

Ensure OrderService/UserService have no duplicated payment ownership now that PaymentService is the platform's payment gateway, without implementing new business features or redesigning PaymentService.

## Current state (grounded findings)

- Investigation (full-codebase grep across `Order.Domain/Application/Persistence/Infrastructure/API` and `User.Domain/Application/Persistence/Infrastructure/API`) found **zero** other duplicated-ownership concerns — no payment gateway/method/account/token/session/settlement/webhook concept exists anywhere else in either service.
- `OrderPayment` had exactly one real cross-cutting consumer outside itself: `Order.cs`'s `Cancel()` reading `Payment.PaymentStatus == PaymentStatus.Paid` — unaffected, since `PaymentStatus` was retained.
- `UserPaymentMethod` had **zero** Application/API/Infrastructure references — no public surface existed for it yet, so this was a pure domain+persistence change with no cascading breakage.
- Neither service's integration events (`BuildingBlock.Contract/Events/{Order,User}/`) carried any payment fields — no contract changes were needed.
- No endpoint in either service currently calls or should call PaymentService yet (no such endpoints exist) — "prepare the architecture, don't implement integration" was satisfied by the new `payment-ownership-boundaries.md` doc's "Payment integration strategy" section, which specifies exactly what a future Order "request payment"/"query payment status" endpoint and a future consumer of `Payment*IntegrationEvent`s will look like, without building either.

## Scope

**Done:** entity/enum/VO/EF-config slimdown for `OrderPayment`/`UserPaymentMethod`, migrations, documentation (Responsibility Matrix, Service Boundaries, `ReferenceType`/`ReferenceId` convention, Payment Integration Strategy).

**Explicitly not done (matches the task's "do not implement integration yet" instruction and Task 1's Phase 7):**
- No PaymentService change.
- No real call from Order/User to PaymentService.
- No consumer of a `Payment*IntegrationEvent` (none exist yet either — that's also Phase 7).
- No `OrderPayment.RecordPayment`/`UserPaymentMethod` creation wiring — both remain unpopulated placeholders.

## Dependencies

Builds on Task 1 (`2026-08-06/Task1_paymentservice-foundation.md`) — PaymentService's `PaymentAccount`/`Payment` are the entities `UserPaymentMethod.PaymentAccountId`/`OrderPayment.PaymentId` now reference by id.

## Estimated complexity

Small — confirmed by investigation before any code was touched: both entities had a small, fully-mapped blast radius (no Application/API/event breakage in either service).

## Risks

- `UserPaymentMethod`'s new unique index is on `PaymentAccountId` alone (one reference row per PaymentAccount) rather than per-user — if a future requirement needs the same `PaymentAccountId` referenced by more than one `UserPaymentMethod` row (unlikely, since a `PaymentAccount` is already user-scoped in PaymentService), this index will need revisiting.
- Both migrations use `DropColumn`/`RenameColumn` (data-loss warnings from `dotnet ef migrations add`, expected and accepted — neither table has shipped to any real environment yet).
