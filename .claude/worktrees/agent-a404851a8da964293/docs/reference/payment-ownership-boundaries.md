# Payment Ownership Boundaries

**Scope:** the responsibility split between PaymentService and every other service that needs to charge, refund, or store a payment method — a reference doc, not a how-to. Written 2026-08-06 alongside the OrderService/UserService architecture sync that made the two pre-existing payment placeholders (`OrderPayment`, `UserPaymentMethod`) match this boundary literally. See [services/payment-service.md](../services/payment-service.md) for PaymentService's own domain model and phased roadmap.

## Responsibility matrix

| Concern | Owner | Notes |
|---|---|---|
| Payment lifecycle (create/authorize/capture/fail/expire) | **PaymentService** | `Payment`, `PaymentIntent`, `PaymentAttempt` |
| Refunds | **PaymentService** | `Refund`, `RefundAttempt` |
| Payment methods catalog (Visa/MasterCard/VNPay/...) | **PaymentService** | `PaymentMethod` — reference data |
| Payment gateways (Stripe/PayPal/VNPay/MoMo/...) | **PaymentService** | `PaymentGateway`, `GatewayConfiguration`, `GatewayStatusMapping` |
| User-linked payment accounts (saved cards/banks/wallets) | **PaymentService** | `PaymentAccount`, `PaymentToken`, `CardInformation` — never a real PAN/CVV |
| Billing profiles & invoices | **PaymentService** | `BillingProfile`, `Invoice` |
| Settlement & reconciliation | **PaymentService** | `Settlement`, `Reconciliation`, `Payout`, `PaymentFee` |
| Webhooks (in/out) | **PaymentService** | `WebhookEvent`, `WebhookDelivery` |
| Payment notifications (dispatch tracking only) | **PaymentService** | `PaymentNotification` — actual send stays NotificationService's job |
| Order business (items, shipping, discounts, taxes, workflow) | **OrderService** | Order never gains a payment concept beyond the reference below |
| Order's own payment *status snapshot* | **OrderService** | `OrderPayment` — reference + cached snapshot, see below |
| Identity, auth, roles, permissions, profile, preferences | **UserService** | Unrelated to payments |
| User's own default-payment-method *reference* | **UserService** | `UserPaymentMethod` — reference only, see below |

No service other than PaymentService may duplicate the left-hand column's ownership. If a future service needs any of these concerns, it calls PaymentService — it does not build its own copy.

## `ReferenceType` / `ReferenceId` convention

PaymentService never depends on the assembly, database, or API of the service it's charging on behalf of. Every `Payment`/`PaymentIntent`/`Refund`/`Invoice`/`ScheduledPayment` links to its business context purely through two fields:

- `ReferenceType` (`NovaCore.Payment.Domain.Enums.ReferenceType`) — which business module this payment is for (`Order`, `Subscription`, `WalletTopup`, `Invoice`, `Booking`, `Donation`, `Membership`, `Manual`, `Other`).
- `ReferenceId` (`Guid`) — that module's own identifier for what's being paid for (e.g. an `Order.Id`). PaymentService never interprets this id beyond storing/indexing it — the owning module is the only party that knows what it means.

Any new consumer module (Wallet, Booking, Marketplace, ...) integrates the same way: add a `ReferenceType` value, call PaymentService's `POST /payments` with that type + its own id. No PaymentService code branches on a specific `ReferenceType` value.

## Service boundaries

**OrderService must never own:**
- A payment gateway, payment method catalog, or payment account.
- A payment token, redirect URL, webhook payload, retry count, or raw gateway response.
- Any card/bank/wallet detail (masked or otherwise).

**OrderService may only:**
- Request payment creation from PaymentService (not implemented yet — see "Payment integration strategy" below).
- Query payment status from PaymentService.
- React to PaymentService's integration events (not implemented yet).
- Keep `OrderPayment` as a local reference + display snapshot (see below) so order-status logic (e.g. `Order.Cancel()` checking whether a refund is required) doesn't need a synchronous call for every read.

**UserService must never own:**
- A payment token, external customer id, external payment method id, or card detail (`CardInformation`).
- Any provider/payment-type classification — that's PaymentService's `PaymentAccountType`/`GatewayType`.

**UserService may only:**
- Keep `UserPaymentMethod` as a local reference (`PaymentAccountId` + `DisplayName` + `IsDefault`) so a user's payment-method list/default can be shown without a synchronous call to PaymentService for every profile read.

## Current ownership after the 2026-08-06 slim-down

Both entities below are now the read-model half of a boundary PaymentService owns the write side of. Neither is wired to real data yet — both are populated only by a future consumer of PaymentService's integration events (Phase 7, not started).

- **`Order.Domain.Entities.Orders.OrderPayment`** (`src/Services/Order/Order.Domain/Entities/Orders/OrderPayment.cs`) — `PaymentId` (nullable `Guid`, PaymentService's `Payment.Id`), `PaymentStatus` (Order's own local snapshot enum — not the same type as PaymentService's `PaymentStatus`), `PaidAmount`, `CurrencyCode`, `PaidAt`. 1:1 with `Order`, shared PK. Updated wholesale via `internal RecordPayment(...)`.
- **`User.Domain.Entities.Users.UserPaymentMethod`** (`src/Services/User/User.Domain/Entities/Users/UserPaymentMethod.cs`) — `PaymentAccountId` (`Guid`, PaymentService's `PaymentAccount.Id`, unique per row), `DisplayName`, `IsDefault`. Owned child of `User`.

## Payment integration strategy (current state and next steps)

**Current state:** reference-only. Neither `OrderPayment.RecordPayment` nor any `UserPaymentMethod` creation path is wired to PaymentService — both tables exist and are migration-ready but nothing populates them yet. Order/User have zero API endpoints or CQRS commands that call PaymentService today (confirmed by full-codebase review, 2026-08-06).

**Planned (see [services/payment-service.md](../services/payment-service.md), Phase 7 — Cross-service integration):**
1. PaymentService publishes `Payment*IntegrationEvent`s (`PaymentCreated`/`PaymentCaptured`/`PaymentFailed`/`RefundCompleted`/...) via its Outbox, contracts added to `BuildingBlock.Contract/Events/Payment/`.
2. OrderService adds an `IIntegrationEventConsumer` that calls `OrderPayment.RecordPayment(...)` when a payment for `ReferenceType.Order` changes status.
3. OrderService adds a "request payment" API/command that calls PaymentService's `POST /payments`/`POST /payment-intents` with `ReferenceType.Order` + the order's id — this is the point where Order "requests payment creation" per its boundary above; not implemented yet.
4. UserService adds an endpoint/command to create a `UserPaymentMethod` reference when a user saves a `PaymentAccount` through PaymentService (either a direct call from the client to PaymentService followed by a User-side reference write, or a `PaymentAccount`-linked integration event — exact mechanism to be decided when this phase is scoped).

Until then, both `OrderPayment`/`UserPaymentMethod` remain schema-ready placeholders — this doc exists so their eventual wiring has an unambiguous contract to implement against.
