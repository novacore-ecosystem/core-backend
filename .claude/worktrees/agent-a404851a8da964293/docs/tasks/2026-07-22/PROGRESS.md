# Progress — 2026-07-22

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

- [x] **Task 1 — Verify AddVariation contract vs. Frontend Task 2.** Resolved: global SKU uniqueness confirmed intentional (user decision); real bug was a TOCTOU race in the pre-check-vs-insert, now fixed (`EfUnitOfWork` translates Postgres unique-violation to 409) and tested; 409 message now names the conflicting product.
- [x] **Task 2 — Notification list null category/type/title.** Fixed: `BsonImmutableValueObjectRegistrar` registers explicit BsonClassMaps (reflection-bound private constructors) for all 8 `Notification.Domain.ValueObjects` types, from Persistence only (Domain stays MongoDB-free). 9 regression tests. Historical pre-fix documents left as-is (dev data, by user decision).
- [x] **Task 3 — Order-owner/checkout flow backend readiness.** Address field implemented (`ShippingAddress` on `Order`/`CreateOrderCommand`/`AdminCreateOrderCommand`/`GetOrderResponse` + migration, not yet applied to a live DB). `GetOrder` ownership check added (403 unless Admin/Root or own order) — was flagged as a gap, user decided to fix now. Payment method + auto-complete-on-transfer deferred as its own follow-up task, by decision.
- [x] **Task 4 — `POST /users/search` already exists; frontend Task 8's "no list endpoint" premise is stale.** Corrected the record, then closed the real gap: user decided on denormalizing Roles (option 2). `UserProfile.Roles` (`string[]`, GIN-indexed) is now a write-once creation-time snapshot; `SearchUsersItemResponse` has a `Roles` field and `UserCriteriaDefinition` has a `role` filter (new `StringCollectionContainsStrategy` in `BuildingBlock.Criteria`). No new cross-service event needed — both profile-creation paths already knew their roles at creation time.
- [x] **Task 5 — Warehouse/Inventory/stock-transaction list/search endpoints missing.** Built: `SearchAsync` on all three Read-service interfaces, one `CriteriaDefinition` per aggregate, three Query/Handler/Validator triads, three `RequireAdmin` Carter endpoints (`/warehouses/search`, `/inventories/search`, `/inventory-transactions/search`), plus supporting indexes. Also added `InventoryDbContextFactory` (Inventory was missing the design-time DbContext factory Order/User already had, needed to generate the migration without booting the full app host).

## Cross-repo dependencies

- Frontend Task 2 (add-variation) — unblocked, Task 1 resolved. No frontend fix needed; the 409 was correct behavior, now with a clearer message.
- Frontend Task 5 (order detail fields) needs no backend change — `GetOrderResponse` already has the fields (now including `shippingAddress` too), frontend just needs to catch up.
- Frontend Task 6 (checkout redesign) — Phase B's address blocker is now resolved; payment/auto-complete remains blocked pending a fresh, separately-scoped backend task.
- Frontend Task 8 (users role tabs) — unblocked. `Roles` now on `/users/search` results, plus a `role` filter for server-side tab pagination.
- Frontend Task 7 item 2 (warehouse/inventory/stock-transaction lists) — unblocked. All three search endpoints now exist (Task 5); frontend can drop the `localStorage` id-tracking workaround.
