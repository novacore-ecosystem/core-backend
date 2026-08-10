# Payment Service

**Scope:** Payment-specific facts. This is a brand-new service (added 2026-08-06) — general patterns still live in [conventions/](../conventions/) and are followed as-is; this doc only records what's Payment-specific: the domain model, what's actually wired up in this foundation phase, and what's deliberately postponed.

## Why PaymentService is independent from OrderService

PaymentService is the platform's central payment gateway, reusable by any business module (Order, Subscription, Wallet, Booking, Marketplace, Membership, Donation, Invoice, ...) — not an Order Payment module. It **never references OrderService** (or any other business service) — no project reference, no shared database, no direct call. Every payment is linked to its business context purely through `ReferenceType` (`NovaCore.Payment.Domain.Enums.ReferenceType`) + `ReferenceId` (a `Guid` the consuming module owns the meaning of). This mirrors how `Order.Domain.Entities.Orders.OrderPayment` already documented itself as "not the payment's system of record" — that entity, and `User.Domain.Entities.Users.UserPaymentMethod`, were the two pre-existing placeholders this service is the real implementation behind.

**2026-08-06 (architecture sync):** both placeholders were slimmed to match this boundary literally — `OrderPayment` now only carries `PaymentId`/`PaymentStatus`/`PaidAmount`/`CurrencyCode`/`PaidAt` (the `PaymentMethod`/`PaymentProvider`/`ProviderName`/`MaskedAccount`/`ReferenceNumber` fields were removed, since those are gateway/account details PaymentService owns), and `UserPaymentMethod` now only carries `PaymentAccountId`/`DisplayName`/`IsDefault` (the `Token`/`ExternalCustomerId`/`ExternalPaymentMethodId`/`CardInformation`/`Provider`/`PaymentType` fields were removed, since those duplicate PaymentService's own `PaymentAccount`/`PaymentToken`/`CardInformation`). This was a reference-*shape* cleanup only — the actual event-driven sync (`OrderPayment.RecordPayment` wired to consume a real `PaymentCompletedIntegrationEvent`, `UserPaymentMethod` rows created when a `PaymentAccount` is added) is still Phase 7 future work, unchanged. See [reference/payment-ownership-boundaries.md](../reference/payment-ownership-boundaries.md) for the full responsibility matrix.

## Projects

`Payment.Domain`, `Payment.Application`, `Payment.Persistence`, `Payment.Infrastructure`, `Payment.API` — same 5-layer split as every other service, `src/Services/Payment/`.

## Aggregate model

30 aggregates/entities across 7 groups, all implemented as full Domain entities + complete EF Core configuration (migration `InitialCreate` creates all of them) — but only the **core lifecycle** group has a Repository/Read/Write-service/CQRS/API surface this phase. See "Planned phases" below for the rest.

**Core lifecycle** (`Payment.Domain/Entities/Payments/`) — full CQRS this phase:
- `PaymentIntent` — Stripe-style entry point (`PaymentIntent → Payment → PaymentAttempt`). `ReferenceType`/`ReferenceId`, `RequestedAmount` (Money), `Status`, `ClientSecret`, `ExpiresAt`.
- `Payment` — the aggregate root of the payment lifecycle. `ReferenceType`/`ReferenceId`, `Amount`, `Status`, `GatewayId`, `PaymentMethodId`, `IdempotencyKey`; owns `PaymentItem`/`PaymentAttempt` children.
- `PaymentItem` — breakdown line (Product/Shipping/Tax/Discount/Insurance/Fee/Tip).
- `PaymentAttempt` — one gateway-facing attempt; a Payment may have several (timeout → retry → success).
- `Refund` — aggregate root, references `Payment.Id` plus its own denormalized `ReferenceType`/`ReferenceId` (so Refund is queryable without a Payment join); owns `RefundAttempt` children.
- `RefundAttempt` — retry support for refund processing.

**Catalog** (`Entities/Catalogs/`) — domain + EF config + startup seed (`Storage/Seeders/PaymentSeeder.cs`), no CQRS (reference data, not commands):
- `PaymentMethod` — Visa/MasterCard/PayPal/VNPay/MoMo/Apple Pay/Google Pay/COD, seeded with deterministic GUIDs.
- `PaymentGateway` — Stripe/PayPal/VNPay/MoMo/Manual, seeded the same way; owns `GatewayConfiguration`/`GatewayStatusMapping` children.
- `GatewayConfiguration` — merchant config per environment (Sandbox/Production). **Never stores plaintext secrets** — `ApiKeyRef`/`SecretRef`/`WebhookSecretRef` are opaque key references. No secret-storage abstraction exists anywhere in the solution yet; integrating one (Data Protection, Vault, ...) is a documented postponed extension point.
- `GatewayStatusMapping` — maps a gateway's own status string to this service's `PaymentStatus`.

**Accounts & billing** (`Entities/Accounts/`, `Entities/Billing/`) — domain + EF config only, no CQRS yet:
- `PaymentAccount` — user-linked payment account (card/bank/PayPal/wallet/Apple Pay/Google Pay). PaymentService owns this data; UserService only ever keeps a reference. Never stores a real PAN/CVV — only `Token`/`MaskedNumber`/`HolderName`/expiration/issuer, same constraint `User.Domain.ValueObjects.CardInformation` already documents for `UserPaymentMethod`.
- `PaymentToken` — gateway tokenization record for a `PaymentAccount`.
- `BillingProfile` — billing identity (legal name, tax id, `BillingAddress`).
- `Invoice` — payment invoice against `ReferenceType`/`ReferenceId`; does not depend on Order.

**Operations** (`Entities/Operations/`) — domain + EF config only:
`Settlement`, `Reconciliation`, `Payout`, `PaymentFee` (linked to either a `Payment` or a `Settlement`), `ExchangeRate`, `PaymentEventLog` (append-only business event history), `PaymentAudit` (append-only, payment-domain-scoped audit — explicitly **not** a replacement for the platform AuditService), `IdempotencyRecord` (durable, business-level dedup of gateway-facing requests — distinct from and complementary to the Redis-backed transport-level `IIdempotencyStore` framework already reused at the API layer for HTTP dedup).

**Webhooks** (`Entities/Webhooks/`) — domain + EF config only:
`WebhookEvent` (incoming, raw payload capture), `WebhookDelivery` (outgoing delivery tracking).

**Scheduling & notifications** (`Entities/Scheduling/`) — domain + EF config only:
`PaymentSession` (redirect/checkout session), `ScheduledPayment` (recurring payment schedule — tracks the schedule only, no dispatch/execution logic yet), `PaymentNotification` (dispatch status only; actual sending stays NotificationService's job).

**Shared enum:** `ReferenceType` (Order/Subscription/WalletTopup/Invoice/Booking/Donation/Membership/Manual/Other=99) — extensible; no Payment code branches on a specific value.

**Value Objects** (`Payment.Domain/ValueObjects/`, all PaymentService-local — the shared `BuildingBlock.Domain.ValueObjects.Money` has no currency and was deliberately left untouched, following the same per-bounded-context precedent `Address` already set in Inventory/User): `Money` (Amount + `Currency`), `Currency` (ISO-4217 code), `BillingAddress`, `CardInformation` (masked-only).

## Ports & routing

Internal `8080` (REST) only, no gRPC. Gateway path prefix `/api/payment/` (`RequireAuth: true`), public debug port `5109` (`PAYMENT_PUBLIC_HTTP_PORT`).

## Routes (Carter endpoints, `Payment.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/payments` | `Payment/CreatePayment.cs` | Create a Payment against `ReferenceType`+`ReferenceId`; idempotent via the `Idempotency-Key` header — a retry with the same key returns the original Payment instead of duplicating it (RequireAuthenticated) |
| GET | `/payments/{paymentId}` | `Payment/GetPayment.cs` | Fetch a Payment with its Items/Attempts (RequireAuthenticated) |
| POST | `/payment-intents` | `PaymentIntent/CreatePaymentIntent.cs` | Create a PaymentIntent (RequireAuthenticated) |
| GET | `/payment-intents/{paymentIntentId}` | `PaymentIntent/GetPaymentIntent.cs` | Fetch a PaymentIntent (RequireAuthenticated) |
| POST | `/refunds` | `Refund/CreateRefund.cs` | Create a Refund against an existing Payment (RequireAuthenticated) |
| GET | `/refunds/{refundId}` | `Refund/GetRefund.cs` | Fetch a Refund (RequireAuthenticated) |

No business workflow behind these yet by design — `CreatePayment`/`CreatePaymentIntent`/`CreateRefund` persist the aggregate in `Pending`/`Created`/`Requested` status and return; there is no gateway call, no authorization/capture, no webhook processing. That is exactly the work later phases add.

## Messaging

No integration events published or consumed yet. `Payment.Infrastructure` wires `AddKafkaMessaging`/`AddInboxOutboxInfrastructure` (so `PaymentDbContext`'s Outbox/Inbox tables and the relay/retry hosted services are live), but no `IIntegrationEventConsumer` is registered and no event contract exists in `BuildingBlock.Contract/Events/Payment/` yet — PaymentService has nothing to consume from another service in this phase (it's only ever called into, never subscribes), and publishing `PaymentCreated`/`PaymentCaptured`/... events for Order (or any consumer) to react to is deferred to the phase that actually needs it.

## Payment-specific building blocks (not present in every service)

- **Currency-aware `Money`, `Currency`, `BillingAddress`, `CardInformation` Value Objects are PaymentService-local** — see "Value Objects" above.
- **Idempotency framework applied to every money-moving endpoint** — `.RequireIdempotency()` on `CreatePayment`/`CreatePaymentIntent`/`CreateRefund`, same as `Order.API`'s `CreateOrder.cs`. Requires `PAYMENT_REDIS_CONNECTION_STRING` (see `.env`).
- **Startup seeding, not EF `HasData`** — `Storage/Seeders/PaymentSeeder.cs` seeds `PaymentMethod`/`PaymentGateway` catalog rows with deterministic GUIDs, run from `ApplicationPipeline.cs`, matching Product/Auth/User/Inventory's own seeding convention (not the EF `HasData` migration-baked approach).
- **No Hangfire/background job scheduling wired yet** — `AddInboxOutboxCleanupJobs` registers the cleanup `IRecurringJob`s (harmless no-op without a scheduler), but `UseBackgroundJobsDashboard()`/`UseBackgroundJobsScheduling()` are intentionally not called — there's no `PAYMENT_HANGFIRE_DB_CONNECTION` yet. Wire this in when a real background job (webhook retry, scheduled payment dispatch, settlement polling) is implemented.
- **Exception codes 800-899 already reserved** for Payment Service in `docs/reference/exceptions.md`/`MessageCode.cs`, currently unused — this phase's handlers only throw generic `NotFoundException`/`ExceptionFactory.*` (free-text, no `MessageCode`). Add `MessageCode` entries in this range as real business exceptions are introduced.

## Persistence: Read/Write services

Per [conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md) — only the core lifecycle slice has a Read/Write service pair, since only that slice has CQRS:

- `Payment.Application/Abstractions/Persistence/{Payments,PaymentIntents,Refunds}/` hold the three aggregates' Read/Write service ports.
- `IPaymentRepository`/`IPaymentIntentRepository`/`IRefundRepository` are near-empty markers — `IPaymentRepository` adds one extra method (`GetByIdempotencyKeyAsync`) for the idempotent-create check; the other two add nothing beyond the generic `IRepository<T, TId>`.
- **Every other aggregate (27 of them) has a `DbSet<T>` and a complete `IEntityTypeConfiguration<T>` but no repository, no Read/Write service, no CQRS** — intentional, not an oversight (see "Scope decision" in the originating task plan). A repository is added alongside whichever aggregate's CQRS work starts next.

## Persistence notes

- Relational FKs throughout, never owned-type entities for anything with independent rows (`PaymentItem`/`PaymentAttempt`/`RefundAttempt`/etc. are all real tables with their own PK + FK back to the parent) — matches Order's post-refactor convention, not the older Owned-Types pattern.
- Multi-property Value Objects (`Money`, `BillingAddress`) **are** mapped via `OwnsOne` (a genuine EF-owned-type mapping for a VO with no identity of its own is a different thing from the owned-*entity* anti-pattern Order moved away from) — see `Configs/MoneyConfigurationExtensions.cs` for the shared `Amount`/`Currency` column-pair mapping reused by every Money-bearing entity.
- `payment_intents`/`payments`/`refunds`/`payment_methods`/`payment_gateways`/... table names are `snake_case`; every table has `CreatedAt`/`UpdatedAt` (`ConfigureCommonFields`/`ConfigureAuditFields`) and the indexes a lookup-by-reference or lookup-by-status query needs (`(ReferenceType, ReferenceId)`, `(Status, CreatedAt)`, etc.)
- `payment_db` added to `scripts/postgres/init.sql`; no `payment_hangfire_db` yet (see "No Hangfire" above).

## Naming note

No `Payment`-vs-`NovaCore.Payment` namespace collision workaround needed for most types — only the `Payment` entity itself collides with the `NovaCore.Payment` root namespace segment (same issue Order/Product/Inventory already document for their own root-namespaced entity). `Payment.Application`/`Payment.Persistence` both alias it:

```csharp
global using PaymentEntity = NovaCore.Payment.Domain.Entities.Payments.Payment;
```

`PaymentIntent`, `Refund`, `PaymentItem`, `PaymentAttempt`, `RefundAttempt`, and every other aggregate have no such collision and are used by their plain names.

## Planned phases (intentionally postponed)

This phase is architecture + domain model + migration-ready schema + a proof-of-concept CQRS/API slice only. Explicitly out of scope, to be scoped as their own dated tasks:

- **Phase 2 — Accounts, billing, invoicing CQRS**: `PaymentAccount`/`PaymentToken`/`BillingProfile`/`Invoice` Read/Write services + endpoints.
- **Phase 3 — Gateway integration**: real Stripe/PayPal/VNPay/MoMo calls from `PaymentAttempt`/`RefundAttempt`, `GatewayConfiguration` secret-store integration, `GatewayStatusMapping` actually driving status translation.
- **Phase 4 — Webhook processing**: `WebhookEvent` signature verification + dispatch, `WebhookDelivery` outgoing retries.
- **Phase 5 — Settlement & reconciliation**: `Settlement`/`Reconciliation`/`Payout`/`PaymentFee` real financial-ops workflows, likely Hangfire-scheduled.
- **Phase 6 — Recurring payments**: `ScheduledPayment` dispatch loop, `PaymentSession` redirect/checkout flow completion.
- **Phase 7 — Cross-service integration**: `Payment*IntegrationEvent` contracts in `BuildingBlock.Contract/Events/Payment/`, Order's `OrderPayment.RecordPayment` wired to consume them, equivalent wiring for Subscription/Wallet/Booking/... as those services come online.
- **Phase 8 — Notifications**: `PaymentNotification` actually dispatched via NotificationService.

## Known issues

- `IdempotencyRecord`/`PaymentAudit`/`PaymentEventLog` have no writer yet — the tables exist (migration-ready) but nothing populates them until the phase that needs them is implemented.
- No integration tests yet (`tests/integration/Payment.IntegrationTests` doesn't exist) — add alongside Phase 2+ business logic.
