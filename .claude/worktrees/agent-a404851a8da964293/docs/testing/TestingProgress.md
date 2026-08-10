# Testing Progress

**Scope:** the living checkpoint for the automated-testing initiative. Read this first when resuming the work in a new session — it tells you exactly what's done, what's next, and why anything was skipped, without re-deriving the analysis. Update it every time a meaningful batch of work lands. See [TestingRoadmap.md](TestingRoadmap.md) for the stable long-term plan this progress is tracked against, and [TestingArchitecture.md](TestingArchitecture.md)/[TestingGuidelines.md](TestingGuidelines.md) for the how.

## Current milestone

**Milestone 1 (Foundation) — complete. Milestone 2 (Product.Domain Value Objects) — complete.** Next up: Milestone 3 (Product.Domain entities — `Product`/`Variant` aggregate invariants), see "Current priority".

## Overall progress snapshot

- 5 test projects exist beyond the original one: `NovaCore.TestKit` (shared infra), `BuildingBlock.SharedKernel.Tests`, `BuildingBlock.Domain.Tests`, `Product.Domain.Tests`, `Order.Application.Tests`.
- 186 tests passing across all 6 test projects (25 + 14 + 35 + 97 + 8 + 7).
- 1 of 7 services (Product) has partial Domain-layer coverage — all 7 Value Objects done, entities (`Product`, `Variant`) not yet started.
- `Order.Application.Tests` (`CancelOrderHandlerTests`, `DeleteOrderHandlerTests`) exists out of Roadmap order — see "Bug-fix exceptions" below. No other Application/Infrastructure/API tests exist yet.
- **Phase 5 kicked off out of order (2026-07-27):** `tests/integration/Order.IntegrationTests` — the first Testcontainers-backed integration test project — was created ad hoc per a direct user request for a race-condition diagnostic, not because Order's Domain/Application layers were otherwise "solid" per the Roadmap's stated Phase 5 entry condition. See "Out-of-order work" below.

## Bug-fix exceptions to Roadmap order

Per Roadmap's "unless there's a specific bug-fix reason to" clause and Guidelines' "Bug fixes" section — a regression test earns its place immediately alongside a fix, even before that service's Domain layer (Phase 3) is otherwise covered.

- **`Order.Application.Tests`** (Order.Domain Phase 3 not yet started) — added while fixing a stock-leak bug: `CancelOrderHandler`/`DeleteOrderHandler` let a `Confirmed`/`Pending` order (stock already deducted by `CreateOrderSaga`) transition to `Cancelled`/deleted without ever calling `IInventoryClientService.RestockAsync`. 7 tests cover both handlers' restock-call behavior (including call ordering via `Received.InOrder`) and the non-invocation guard on invalid status transitions. Also added `BuildingBlock.Persistence.Ef.Tests/EfUnitOfWorkTests.cs` (2 tests) for a related TOCTOU race: `AddVariationHandler`'s `SkuExistsAsync` pre-check and the DB unique index aren't atomic, so a concurrent duplicate-SKU insert previously surfaced as an unhandled 500 instead of `ConflictException` (409) — `EfUnitOfWork.ExecuteTransactionAsync` now also translates a Postgres unique-violation (`23505`), not just `DbUpdateConcurrencyException`.

## Out-of-order work

Direct user requests for a specific diagnostic/regression test take priority over Roadmap sequencing, same principle as "Bug-fix exceptions" above but for a different trigger (an explicit ask, not a fix-in-progress).

- **`tests/integration/Order.IntegrationTests`** (2026-07-27) — first Phase 5 project, requested directly: a dedicated integration test reliably reproducing a race condition when two requests update the same Order simultaneously (`Concurrency/UpdateOrderRaceConditionTests.cs`), to later verify optimistic concurrency/distributed locking actually fixes it. Uses real Postgres via Testcontainers, wired through the actual `Order.Persistence.AddPersistence`/`Order.Application.AddApplication` DI extensions (not a simplified stand-in) - deliberately not a `WebApplicationFactory`/real-HTTP host, since `Order.Infrastructure.AddInfrastructure` eagerly wires Kafka/Redis/gRPC-to-Inventory, none of which matter to an EF-Core-vs-Postgres concurrency question. `IInventoryClientService`/`ICurrentUserService` substituted (ample-stock fake / `NovaCore.TestKit`'s `FakeCurrentUserService`); every other moving part is production code.
  - **Significant finding, NOT fixed here:** building this test surfaced that `UpdateOrder` fails unconditionally today - even with zero concurrent requests - because new `OrderItem` entities added via collection-navigation mutation (not an explicit `context.Add()`) get misclassified `Modified` instead of `Added` by EF Core once their client-generated Guid key is non-default, producing a 0-row `UPDATE` and a misleading `DbUpdateConcurrencyException`/409. See `docs/tasks/2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md`. The race test itself still runs correctly (100 iterations, ~6s, passing) - it just can't yet observe the "one 200, one 409" steady state it was built to distinguish from real corruption, since neither request currently succeeds. Re-run once Task 23 lands.
  - The anticipated shared `NovaCore.TestKit.Integration` fixture project (see "Future improvements," now below) wasn't created — with exactly one integration test project so far, an `OrderIntegrationTestBase` living directly in `Order.IntegrationTests/Infrastructure/` is sufficient; extract to a shared project only once a second `{Service}.IntegrationTests` project needs the same Testcontainers-bootstrap pattern (rule of three, same principle Milestone 2 already established for `UppercaseCodeValueObjectTests<T>`).

## Completed work

### Milestone 1 — Foundation
- `tests/Directory.Build.props` + `tests/Directory.Packages.props` — central package management scoped to `/tests` only (verified `src/**` is unaffected — see TestingArchitecture.md for why this is safe).
- Migrated `BuildingBlock.Persistence.Ef.Tests.csproj` to the new central package management (was: inline versions xUnit 2.9.2 / Test.Sdk 17.12.0 / EFCore.InMemory 10.0.1 → now inherits pinned versions; all 5 pre-existing tests still pass, confirmed by `dotnet test`).
- `tests/Common/NovaCore.TestKit` created: `Builders/TestDataBuilder<T>`, `Fakes/FakeCurrentUserService`, `Fakes/FakeAppLogger<T>`, `Random/TestId`, `ShouldlyExtensions/DomainExceptionShouldlyExtensions`.
- `tests/BuildingBlock.SharedKernel.Tests` created — 17 tests covering `ArrayExtension` and `StringExtension` (the only logic-bearing files in SharedKernel).
- `tests/BuildingBlock.Domain.Tests` created — 35 tests covering `ValueObject`/`StringValueObject` equality contract, `ExceptionFactory` (all 15 factory methods), `BaseEntity.Tourch()` (see Known Limitations — yes, "Tourch" is a typo in production), `MessageCodeExtension`.
- All 3 new projects wired into `NovaCore.sln` under the `tests`/`tests/Common` solution folders via `dotnet sln add`.
- `dotnet build NovaCore.sln` succeeds; `dotnet test NovaCore.sln` runs all 4 test projects, 57/57 passing.
- `/docs/testing/` doc set created (this file + Architecture + Guidelines + Roadmap).
- `docs/05-context-loading-map.md`, `docs/README.md`, and 3 workflow docs (`add-new-domain-entity.md`, `add-new-api.md`, `fix-bug.md`) updated with testing pointers — the latter two also had their stale "no automated test suite exists yet" claims corrected.

### Milestone 2 — Product.Domain Value Objects
- `tests/Product.Domain.Tests` created, referencing `Product.Domain` + `NovaCore.TestKit`.
- `ValueObjects/UppercaseCodeValueObjectTests<T>` — a shared abstract generic test base covering the validation contract common to `Sku`, `ProductCode`, `CategoryCode`, `TagCode` (all four: required, max 50 chars, `^[A-Z0-9-]+$`, `Trim().ToUpperInvariant()` normalization). Each concrete subclass (`SkuTests`, `ProductCodeTests`, `CategoryCodeTests`, `TagCodeTests`) is 4 lines wiring up the 3 static factory calls. Scoped to `Product.Domain.Tests` rather than `NovaCore.TestKit` since the exact shape (regex, max length) is Product-specific — promote to the TestKit only if a second service needs the identical shape.
- `SlugTests` (different casing direction, max length 200, kebab-case format), `BarcodeTests` (numeric-only, no max-length branch), `DimensionsTests` (multi-field numeric `ValueObject`, not `StringValueObject`) — each has its own test class since their validation shape genuinely differs from the shared base.
- 97 tests total, covering every validation branch (null/empty/whitespace, too-long, wrong-format, boundary-at-max-length), normalization, `TryCreate`/`IsValid` parity with `Create`, and equality-after-normalization for all 7 Value Objects.
- Found and worked around one incorrect test assumption during implementation: `Slug` lowercases *before* the format regex runs, so mixed-case input like `"Not-Lowercase"` is valid (normalizes then passes) rather than rejected — caught by a failing test run, fixed by removing that case from the "invalid format" theory and relying on the existing `Create_MixedCaseInput_NormalizesToLowercase` test to cover the normalization behavior instead.
- Project wired into `NovaCore.sln` under `tests`. Full solution: `dotnet build` succeeds, `dotnet test` passes 154/154 across 5 projects.

## Remaining work

See [TestingRoadmap.md](TestingRoadmap.md) for the full phase breakdown. Immediate next batches, in order:

1. Product.Domain entities (`Product`, `Variant`) — aggregate invariants, variation-collection rules, default-variation logic. **Current priority.**
2. Notification.Domain (richest remaining service).
3. `BuildingBlock.Application` pure-logic pieces (`ValidationBehavior<,>`, `PaginatedResult`, `ApiResponse`).
4. Remaining services' Domain layers (Inventory → Auth → User → Order → Audit), per Roadmap order.

## Current priority

Product.Domain entities — `Product`/`Variant` aggregate invariants (Phase 3, item 1 above).

## Files/folders already covered

- `src/BuildingBlocks/BuildingBlock.SharedKernel/Extensions/ArrayExtension.cs`
- `src/BuildingBlocks/BuildingBlock.SharedKernel/Extensions/StringExtension.cs`
- `src/BuildingBlocks/BuildingBlock.Domain/Abstractions/ValueObject.cs`
- `src/BuildingBlocks/BuildingBlock.Domain/Abstractions/StringValueObject.cs`
- `src/BuildingBlocks/BuildingBlock.Domain/Abstractions/BaseEntity.cs` (`Tourch()` only)
- `src/BuildingBlocks/BuildingBlock.Domain/Exceptions/ExceptionFactory.cs` (all 15 factory methods)
- `src/BuildingBlocks/BuildingBlock.Domain/Extensions/MessageCodeExtension.cs`
- `src/BuildingBlocks/BuildingBlock.Persistence.Ef/*` — pre-existing, see `AuditGraphBuilderTests.cs`/`AuditInterceptorTests.cs`
- `src/Services/Product/Product.Domain/ValueObjects/*.cs` — all 7 files (`Sku`, `ProductCode`, `Barcode`, `CategoryCode`, `Slug`, `TagCode`, `Dimensions`)

## Files skipped intentionally

- `BuildingBlock.SharedKernel/Constants/*.cs` (`AppRole`, `CacheKeys`, `HeaderKeys`, `JobQueue`) — string/int constants, no logic.
- `BuildingBlock.SharedKernel/Security/JwtSettings.cs` — plain options-binding POCO, no logic.
- `BuildingBlock.SharedKernel/Serialization/JsonSerializerConfiguration.cs` — static configuration, no branching logic to assert on beyond "does it construct" (framework behavior, per TestingGuidelines "what not to test").
- `BuildingBlock.Domain/Abstractions/AggregateRoot.cs` — empty marker subclasses of `BaseEntity`, no behavior beyond what `BaseEntityTests` already covers.
- `BuildingBlock.Domain/Metadata/*`, `Seeders/SeedAuthData.cs`, `Attributes/AuditIgnoreAttribute.cs` — reflection/attribute plumbing exercised indirectly by `AuditGraphBuilderTests`; revisit only if a bug surfaces here.

## Technical debt

- **`BaseEntity<T>.Tourch()` is a misspelling of `Touch()`.** Tests match the actual production method name (documented inline in `BaseEntityTests.cs`) rather than "fixing" it, per the rule that production code changes require an unambiguous testability/architecture reason. Renaming is a trivial, low-risk fix but touches every call site across every entity in every service — flagged here for a deliberate decision, not done silently as a side effect of writing tests.
- **`ExceptionFactory.InsufficientBalance`/`InsufficientQuota` both surface `MessageCode.InsufficientStock`**, not distinct codes — `ExceptionFactoryTests` documents this as current behavior. Only worth fixing if a real caller needs to distinguish the three cases by code.
- **No `IClock`/`IDateTimeProvider` abstraction** — `BaseEntity<T>` calls `DateTime.UtcNow` directly, which is why `BaseEntityTests.Tourch_UpdatesUpdatedAt_ButNotCreatedAt` needs a real `Thread.Sleep(5)` instead of a controllable fake clock. Low priority: introducing one is a production-code change affecting every entity's construction, with payoff limited to slightly faster/cleaner time-based tests.

## Future improvements

- `UppercaseCodeValueObjectTests<T>` (in `Product.Domain.Tests`) already generalizes the 4-way-duplicated Sku/ProductCode/CategoryCode/TagCode shape. If a second *service* (not just a second VO in Product) turns out to have the identical required/max-length/regex/normalize contract, promote it into `NovaCore.TestKit` — not before (rule of three across services, not just within one).
- Consider AutoFixture/Bogus for Application-layer DTO-heavy tests if hand-written builders prove too verbose once Phase 4 starts — revisit, don't pre-adopt.
- Testcontainers module registration (Phase 5) will need its own shared fixture project, likely `tests/Common/NovaCore.TestKit.Integration` or similar, kept separate from the unit-test `NovaCore.TestKit` so unit test projects never pull in a Testcontainers dependency.

## Known limitations

- Coverage tooling (`coverlet.collector`) is pinned in `Directory.Packages.props` but no coverage-reporting workflow/CI step exists yet — running `dotnet test --collect:"XPlat Code Coverage"` works per-project but nothing aggregates it yet.
- No CI pipeline currently runs `dotnet test` automatically (no `.github/workflows` or similar found in the repo) — this initiative produces the tests; wiring them into CI is a separate, not-yet-scoped task.
