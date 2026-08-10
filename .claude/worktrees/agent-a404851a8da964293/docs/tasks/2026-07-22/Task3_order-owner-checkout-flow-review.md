# Task 3: Backend readiness review for the redesigned buyer/order-owner checkout flow

**Scope:** Frontend wants to redesign checkout (NovaCoreUI `docs/tasks/2026-07-22/Task6_checkout-flow-redesign.md`) into: **Cart** (review + total) → **Order Owner** (name, address, phone — prefilled from the buyer's own profile but editable, and *approved even if it doesn't match the logged-in account*) → **Confirm** (maybe payment method) → *(maybe an external "perform transfer" step)* → **Complete** (Order ID + message). This task checks what the backend already supports vs. what's missing, endpoint by endpoint.

## 1. Order owner name/phone independent of the account — ALREADY SUPPORTED

`CreateOrderCommand` (`Order.Application/Features/Orders/Commands/CreateOrder/CreateOrderCommand.cs:7-8`) takes `CustomerName`/`CustomerPhone` as free-text fields; the caller's own session supplies only `CustomerId` (never client-provided — resolved server-side, confirmed by `CreateOrderHandler.cs:31` passing the session's `customerId` separately from `request.CustomerName`/`request.CustomerPhone`). These are validated only for non-emptiness (`CreateOrderValidator.cs:12-17`, `NotEmpty`), not compared against the account's own stored name/phone anywhere.

**Conclusion:** the buyer can already type an owner name/phone that differs from their own account's profile, and the backend accepts it as-is. No backend change needed for this part of the flow.

Confirmed persisted, not just accepted transiently: `Order.Domain/Entities/Order.cs:14-15,43-44` stores `CustomerName`/`CustomerPhone` directly on the `Order` aggregate (`Order.Persistence/Configs/OrderConfig.cs:12-13`, both `IsRequired()`), and a migration landed **today**, `20260722021852_AddOrderCustomerSnapshotAndDiscount`, confirming this is recent/current schema, not legacy cruft.

**Side effect worth flagging to the frontend:** `GetOrderResponse` (`Order.Application/Features/Orders/Queries/GetOrder/GetOrderQuery.cs:13-22`) already returns `CustomerName`, `CustomerPhone`, `CancellationReason`, and per-item `Discount`/`LineTotal` — none of which exist in the frontend's current `GetOrderResponse` TypeScript type (see Frontend Task 5). This is not a backend gap at all; it's a **frontend contract drift** — the frontend's types/OpenAPI client were generated before this migration and haven't been refreshed. `docs/backend/order/README.md` (the frontend repo's mirror of this contract) still says these DTOs are "unchanged," which is now stale and needs updating alongside the frontend's regen.

## 2. Address — IMPLEMENTED 2026-07-22

**Resolved.** User confirmed address collection is needed for this iteration. Added:

- `Order` entity: new `ShippingAddress` (string, required, max 500) — free-text snapshot, same convention as `CustomerName`/`CustomerPhone` (captured once at Create time, never resynced/normalized into structured street/city/zip fields).
- `CreateOrderCommand`/`CreateOrderRequest` (client path) and `AdminCreateOrderCommand`/`AdminCreateOrderRequest` (admin path): both now take `ShippingAddress` (required, non-empty, max 500 — `CreateOrderValidator`/`AdminCreateOrderValidator`).
- `GetOrderResponse`: now includes `ShippingAddress`.
- Migration `20260722103729_AddOrderShippingAddress` (adds `shipping_address` column, `NOT NULL`, default `''` for any pre-existing rows) — not yet applied to a live DB, same as the day's earlier `AddOrderCustomerSnapshotAndDiscount` migration.
- New `Order.API/OrderDbContextFactory.cs` (`IDesignTimeDbContextFactory<OrderDbContext>`) was added to unblock `dotnet ef migrations add` without booting the full app host (Kafka/Redis/APM) — Order didn't have one before, unlike User's `UserDbContextFactory`. Tooling-only, mirrors User's pattern exactly.

**Frontend action needed:** NovaCoreUI's checkout redesign (Task 6) can now collect and submit `shippingAddress` on both `POST /orders` and `POST /orders/admin`, and `GET /orders/{orderId}` returns it. See that task's own update.

## 3. Payment method selection + "transfer then auto-complete" — NOT SUPPORTED

- No payment/channel concept exists anywhere in the Order service's contract.
- `CompleteOrder` exists but is **Admin-only** (`RequireAdmin` policy, confirmed in `docs/services/order-service.md` and `reference/create-order-saga.md`) — it's a manual admin action today, not something a payment confirmation (webhook or user "I've transferred" click) could call.
- `reference/create-order-saga.md`'s own "Future extension points" section already anticipated this: *"Payment — a `ChargePaymentStep` would slot in after `ConfirmOrder`, or as its own saga triggered off `OrderConfirmedIntegrationEvent`, with `RefundStep` as its compensation."* — explicitly deferred, not built.

**This is the single largest gap between the desired flow and current backend capability.** Building it needs real scoping: a payment-method field on order creation, a way for a buyer (or a webhook) to signal "payment done" without full `Admin`/`Root` privileges, and either a new endpoint or a relaxed/rebuilt `CompleteOrder` path for this specific case. Recommend treating this as its own follow-up task rather than bundling it into the checkout-redesign work — the frontend can build everything up through the "Confirm" step against what already exists, and stop there until this is scoped.

## GetOrder ownership check — fixed 2026-07-22

Was flagged in passing: `GetOrder` only required `RequireAuthenticated`, with no ownership check against the requesting user's `CustomerId` — any authenticated user could fetch any order by ID if they knew/guessed it. **User decided to fix now.** `GetOrderHandler` now injects `ICurrentUserService` and throws `ForbiddenException` (403) unless the caller is Admin/Root or `order.CustomerId` matches the caller's own id — admin dashboard use case preserved (Admin/Root bypass), regular customers can now only fetch their own orders. No endpoint-level policy change (still `RequireAuthenticated`) since the owner-vs-admin distinction is data-level, not role-level.

## Payment method + auto-complete-on-transfer — deferred, by decision

**User decided to defer this as its own follow-up task**, per this task's own original recommendation. Not scoped or built in this session. Still needs: a payment-method field, a non-admin way to signal payment done (webhook or user action), and either a new endpoint or a relaxed/rebuilt `CompleteOrder` path. Raise as a fresh task when picked up.

## Status

**Resolved for this session's scope:**
- [x] Owner name/phone decoupled from account — already worked, no backend change needed.
- [x] Address field — implemented (see above).
- [x] GetOrder ownership check — implemented (see above).
- [ ] Payment method + auto-complete-on-transfer — deferred by decision, needs its own follow-up task when picked up; biggest remaining lift.
- [ ] Frontend contract drift on `GetOrderResponse` (customerName/Phone/cancellationReason/item discount+lineTotal/shippingAddress already exist server-side but aren't in the frontend's types yet) — flag to frontend, no backend action needed beyond keeping `docs/backend/order/README.md`'s mirror doc in mind next time it's touched.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-22/Task5_order-detail-missing-fields.md`, `docs/tasks/2026-07-22/Task6_checkout-flow-redesign.md`.
