# Tasks — Overall Progress

One line per still-open task, most recent date first. Per-task detail lives in each date folder — see [README.md](./README.md) for the convention.

## 2026-08-06

Source: explicit task — "Establish the production-ready foundation of a new PaymentService for the NovaCore ecosystem" (architecture + domain model only, no business workflows). See [2026-08-06/Task1_paymentservice-foundation.md](./2026-08-06/Task1_paymentservice-foundation.md) for full detail.

- [x] Task 1 — PaymentService production-ready foundation (`2026-08-06/Task1_paymentservice-foundation.md`) — done. 30 aggregates, full EF schema, CQRS+API slice for Payment/PaymentIntent/Refund. Phases 2-8 (accounts/billing CQRS, gateway integration, webhooks, settlement, recurring payments, cross-service integration, notifications) intentionally not started.
- [x] Task 2 — Sync OrderService/UserService with the PaymentService boundary (`2026-08-06/Task2_order-user-payment-architecture-sync.md`) — done. Slimmed `OrderPayment`/`UserPaymentMethod` to lightweight references, removed duplicated payment fields/enums/VOs, new `docs/reference/payment-ownership-boundaries.md`. No PaymentService change; live cross-service integration still not started (Phase 7).

## 2026-07-28

Source: planning-only request — "User Service Search, Elasticsearch, Localization & Cache Layer" (MiddleName name-model refactor, locale-aware DisplayName, Elasticsearch-backed search mirroring Product's architecture, User Detail cache layer, gRPC single/batch retrieval). Full read-only impact analysis across 6 research areas (Product's ES implementation, User's current state, cache infra, gRPC consumers, frontend screens, locale/pipeline conventions). See [2026-07-28/00-architecture-and-plan.md](./2026-07-28/00-architecture-and-plan.md) for architecture notes, dependency graph, implementation order, and risks. Tasks 1-16 and 18 implemented same day (name model, locale/DisplayName, Elasticsearch search, cache, gRPC incl. Audit as first consumer, migration review, docs); Task 17 (testing) remains not started. Paired with 7 frontend tasks in `NovaCoreUI/docs/tasks/2026-07-28/` (still blocked on these backend tasks reaching a stable contract for frontend consumption).

- [x] Task 1 — Add MiddleName: Domain + Persistence (`2026-07-28/Task1_middlename-domain-and-persistence.md`) — done.
- [x] Task 2 — Add MiddleName: Application layer (`2026-07-28/Task2_middlename-application-layer.md`) — done.
- [x] Task 3 — Propagate MiddleName: cross-service contracts (`2026-07-28/Task3_middlename-cross-service-contracts.md`) — done.
- [x] Task 4 — Build `ICurrentLocaleService` (`2026-07-28/Task4_current-locale-service.md`) — done.
- [x] Task 5 — Build locale-aware DisplayName formatter (`2026-07-28/Task5_displayname-formatter.md`) — done.
- [x] Task 6 — Scaffold User Elasticsearch search (`2026-07-28/Task6_elasticsearch-scaffolding.md`) — done.
- [x] Task 7 — Design UserSearchDocument + accent-insensitive mapping (`2026-07-28/Task7_search-document-and-accent-insensitive-mapping.md`) — done; live-ES diacritic verification still open.
- [x] Task 8 — ProjectionBuilder + sync events (`2026-07-28/Task8_projection-builder-and-sync-events.md`) — done.
- [x] Task 9 — RebuildUserSearchIndex command + ES config (`2026-07-28/Task9_rebuild-command-and-es-config.md`) — done.
- [x] Task 10 — Cut SearchUsers over to Elasticsearch (`2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md`) — done, full cutover.
- [x] Task 11 — User Detail cache scaffold (`2026-07-28/Task11_user-detail-cache-scaffold.md`) — done (DTO-based reader, not a literal decorator, due to UserProfile's private setters).
- [x] Task 12 — Wire cache invalidation into Create/Update/Delete (`2026-07-28/Task12_cache-invalidation-wiring.md`) — done.
- [x] Task 13 — Extend `user.proto` with GetUser/GetUsers (`2026-07-28/Task13_grpc-proto-getuser-getusers.md`) — done.
- [x] Task 14 — Implement server-side GetUser/GetUsers (`2026-07-28/Task14_grpc-server-implementation.md`) — done.
- [x] Task 15 — First real gRPC consumer (`2026-07-28/Task15_first-grpc-consumer.md`) — done: Audit, chosen autonomously — **flag for team confirmation**.
- [x] Task 16 — Migration/reindex review (`2026-07-28/Task16_migration-and-reindex-review.md`) — done; top risk: run `RebuildUserSearchIndex` before first real `SearchUsers` call post-deploy.
- [ ] Task 17 — Testing (`2026-07-28/Task17_testing.md`)
- [x] Task 18 — Documentation updates (`2026-07-28/Task18_documentation-updates.md`) — done.

## 2026-07-27

Source: full-system business-requirements audit (Product, Customer, Order+concurrency, Audit Log, Inventory) — read-only, no fixes applied yet. 12 tasks opened; Task 9 resolved same day. A second audit pass this date (SmartCommerce V3 Search/Variation/Cart-Stock checklist) opened Tasks 13-22 — all 10 resolved same day. See [2026-07-27/PROGRESS.md](./2026-07-27/PROGRESS.md) for detail.

- [ ] Task 1 — Customer (User) Delete endpoint does not exist (`2026-07-27/Task1_customer-delete-endpoint-missing.md`).
- [ ] Task 2 — Order concurrency token never round-trips through the API contract; real cross-session conflicts unprotected (`2026-07-27/Task2_order-concurrency-token-not-in-contract.md`).
- [ ] Task 3 — Inventory REST stock-mutation endpoints have no idempotency protection (`2026-07-27/Task3_inventory-rest-endpoints-missing-idempotency.md`).
- [ ] Task 4 — Order aggregate/DTOs missing `TotalQuantity` (`2026-07-27/Task4_order-missing-total-quantity.md`).
- [ ] Task 5 — OrderItem has no distinct Variant field (`2026-07-27/Task5_order-item-missing-variant-field.md`).
- [ ] Task 6 — Audit log query API has no server-side entity/actor filter (`2026-07-27/Task6_audit-log-missing-entity-filter.md`).
- [ ] Task 7 — Inventory publishes no domain-specific stock events over Kafka (`2026-07-27/Task7_inventory-no-domain-stock-events-on-kafka.md`).
- [ ] Task 8 — No standardized retry policy (Polly) anywhere (`2026-07-27/Task8_no-standardized-retry-policy.md`).
- [x] Task 9 — Dead-letter handling is a DB status flag only, not a real Kafka DLQ (`2026-07-27/Task9_dead-letter-handling-db-flag-only.md`) — resolved: periodic count-and-log dead-letter monitor job added; full replay-from-topic DLQ left out of scope.
- [ ] Task 10 — Category/Tag delete has no usage-count precheck (`2026-07-27/Task10_category-tag-delete-no-usage-precheck.md`).
- [ ] Task 11 — `RebuildProductSearchIndex` has no documented auth requirement (`2026-07-27/Task11_rebuild-search-index-auth-undocumented.md`).
- [ ] Task 12 — Inventory reservation model decision needed: deduct-immediately + saga-compensate vs. true reserve/commit/release (`2026-07-27/Task12_inventory-reservation-model-decision.md`).
- [x] Task 13 — Keyword/filter search has no case-insensitive option, shared infra + User + Order (`2026-07-27/Task13_case-insensitive-search-missing.md`) — resolved.
- [x] Task 14 — Order search has no Order ID field (`2026-07-27/Task14_order-id-search-filter-missing.md`) — resolved.
- [x] Task 15 — Product Search doesn't index/search Variation Name (`2026-07-27/Task15_product-search-missing-variation-name.md`) — resolved; live reindex not run this session.
- [x] Task 16 — `ProductVariation.Name` is a dead property, never set or returned (`2026-07-27/Task16_variation-name-not-wired.md`) — resolved: full Create/Add/Update/Read wiring + migration.
- [x] Task 17 — No shared stock-availability service across Cart/Order (`2026-07-27/Task17_no-shared-stock-validation-service.md`) — resolved: `IStockAvailabilityService` extracted.
- [x] Task 18 — Add Cart API never checks real stock, only a status flag (`2026-07-27/Task18_add-cart-no-realtime-stock-check.md`) — resolved.
- [x] Task 19 — Get Cart returns no live stock/availability per item (`2026-07-27/Task19_get-cart-no-live-stock-check.md`) — resolved.
- [x] Task 20 — Create Order's insufficient-stock error has no structured per-item detail (`2026-07-27/Task20_create-order-insufficient-stock-error-not-structured.md`) — resolved as part of Task 17's change.
- [x] Task 21 — Product Search's `isInStock` reflects only the default variation (`2026-07-27/Task21_product-list-add-cart-checks-default-variation-only.md`) — resolved via direct gRPC (user-confirmed approach).
- [x] Task 22 — GetProduct response has no per-variation stock data (`2026-07-27/Task22_get-product-no-per-variation-stock.md`) — resolved.
- [ ] Task 23 — UpdateOrder fails unconditionally, not just under concurrency (`2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md`) — discovered while building `Order.IntegrationTests`; new OrderItems get misclassified Modified instead of Added, causing every UpdateOrder call to 409. Not fixed (out of scope for a test-writing task).

## 2026-07-22

- [x] Task 1 — Verify AddVariation contract vs. Frontend Task 2 (`2026-07-22/Task1_verify-add-variation-contract.md`) — resolved: global SKU uniqueness intentional, TOCTOU race fixed, error message improved.
- [x] Task 2 — Notification list null category/type/title (`2026-07-22/Task2_notification-list-null-fields.md`) — fixed: BsonClassMap registration for all 8 affected value objects, tested.
- [x] Task 3 — Order-owner/checkout flow backend readiness (`2026-07-22/Task3_order-owner-checkout-flow-review.md`) — address field + GetOrder ownership check implemented; payment method deferred as its own follow-up task.

All three closed for this date. See [2026-07-22/PROGRESS.md](./2026-07-22/PROGRESS.md) for detail. Remaining open item: payment method + auto-complete-on-transfer needs a fresh backend task when scoped (see Task 3).
