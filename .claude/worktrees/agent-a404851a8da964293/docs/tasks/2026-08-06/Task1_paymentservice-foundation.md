# Task 1: PaymentService — Production-Ready Foundation

**Status:** Done (foundation phase only — see Scope)
**Category:** New service, architecture/domain-model foundation, no business logic

## What was done

Built a brand-new `PaymentService` (`src/Services/Payment/`) as the platform's independent, reusable payment gateway — 5-project Clean Architecture split (`Payment.Domain/Application/Infrastructure/Persistence/API`), wired into `NovaCore.sln`, following Order Service's conventions (the most recently updated service) and reusing every applicable BuildingBlock (`AggregateRoot`/`BaseEntity`/`ValueObject`, MediatR `ICommand`/`IQuery`, `IRepository`/`IUnitOfWork`/`EfUnitOfWork`, Outbox/Inbox, Idempotency+DistributedLock framework, FluentValidation, `BuildingBlock.Web`).

- **Domain**: 30 aggregates/entities across 7 groups (core lifecycle, catalog, accounts/billing, operations, webhooks, scheduling/notifications), 23 enums, 4 PaymentService-local Value Objects (`Money` w/ `Currency`, `BillingAddress`, `CardInformation`) — the shared `BuildingBlock.Domain.ValueObjects.Money` has no currency field and was deliberately left untouched.
- **Persistence**: `PaymentDbContext` (Outbox+Inbox), complete `IEntityTypeConfiguration<T>` for all 30 entities (relational FKs, `OwnsOne` for multi-property VOs, full index/constraint coverage), `dotnet ef migrations add InitialCreate` generated and verified (30 domain tables + Outbox/Inbox/InboxRetryHistory), startup seeder (`PaymentSeeder`) for the `PaymentMethod`/`PaymentGateway` catalog.
- **Application/API**: full CQRS + Carter endpoint vertical slice for the **core lifecycle** aggregates only — `Payment`/`PaymentIntent`/`Refund`, Create+Get each, `.RequireIdempotency()` on all three Create endpoints.
- **Cross-cutting**: `payment_db` added to `scripts/postgres/init.sql`; `PAYMENT_*` block added to `.env`/`.env.template`; `payment-api` service added to `docker-compose.yml`/`docker-compose.override.yml`; `"Payment"` route added to the YARP gateway (`src/ApiGateways/YarpApiGateway/appsettings.json`), port `5109`.
- **Docs**: `docs/services/payment-service.md` (full structure, aggregate model, planned-phases list), linked from `docs/README.md`/`docs/01-architecture-map.md`.

Every project builds clean (`dotnet build` per-project, verified after each layer); the EF migration was generated and the whole `Payment.Persistence` project rebuilt with it in place. No Docker/runtime verification performed, per the task's own instructions (build-only, no `docker-compose up`).

## Objective

Establish the architecture + domain model + migration-ready schema so future phases (Order Payment integration, wallet top-up, subscription billing, refund processing, capture/authorization, webhook processing, gateway integration, recurring payments, settlement, financial reconciliation) can be implemented without any schema refactor or architectural redesign.

## Current state (grounded findings)

- `Order.Domain.Entities.Orders.OrderPayment` (`src/Services/Order/Order.Domain/Entities/Orders/OrderPayment.cs`) and `User.Domain.Entities.Users.UserPaymentMethod` are pre-existing placeholders pointing at this service — neither was modified; `OrderPayment.RecordPayment` remains uncalled, reserved for a future integration-event consumer.
- Error codes 800-899 were already reserved for "Payment Service" in `docs/reference/exceptions.md`/`MessageCode.cs` before this task — still unused (this phase's exceptions are all generic `NotFoundException`/`ExceptionFactory.*`).
- `NovaCore.sln` now has a `Payment` solution folder with 5 projects (no test projects added yet — `tests/unit/Payment.Application.Tests`/`tests/integration/Payment.IntegrationTests` don't exist).

## Scope

**Built this phase:**
- Full Domain + EF configuration for all 30 aggregates (migration-ready).
- Full CQRS + API for `Payment`/`PaymentIntent`/`Refund` (Create + Get only).
- Catalog seeding (`PaymentMethod`/`PaymentGateway`).
- All cross-cutting infra wiring (DB, env, compose, gateway).

**Explicitly not built (see `docs/services/payment-service.md`, "Planned phases"):**
- Any real gateway call (Stripe/PayPal/VNPay/MoMo) — `PaymentAttempt`/`RefundAttempt` are schema-only.
- Accounts/Billing/Invoice CQRS (`PaymentAccount`/`PaymentToken`/`BillingProfile`/`Invoice`).
- Webhook processing, settlement/reconciliation/payout workflows, recurring payment dispatch, notifications.
- `Payment*IntegrationEvent` contracts / cross-service integration (Order's `OrderPayment.RecordPayment` wiring).
- Hangfire background job scheduling (cleanup jobs are registered but not scheduled — no `PAYMENT_HANGFIRE_DB_CONNECTION`).
- Secret-store integration for `GatewayConfiguration` (no such abstraction exists anywhere in the solution yet).
- Tests (`Payment.Application.Tests`/`Payment.IntegrationTests` projects not created).

## Dependencies

None — this is a new, independent service. Future phases that touch Order (`OrderPayment.RecordPayment`) or User (`UserPaymentMethod`) depend on this foundation but this task didn't touch either.

## Estimated complexity

Foundation itself: Large (single extended session, ~150 new files). Each follow-up phase in "Planned phases" is its own Medium-to-Large task.

## Risks

- The 27 aggregates without CQRS have no repository yet — if a future phase needs cross-aggregate transactional writes spanning one with a repo and one without, that phase must add the missing repository first (empty marker + generic `IRepository<T>`, cheap).
- `GatewayConfiguration.ApiKeyRef`/`SecretRef`/`WebhookSecretRef` are opaque strings with no real secret-store behind them yet — do not put real credentials in them until that abstraction exists.
- No integration tests exist — the EF migration was verified to *generate* and the projects to *compile*, but no runtime/Docker verification has been done (matches the task's explicit "no Docker" instruction, but means the first real run against Postgres is still unverified).
