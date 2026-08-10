# Testing Guidelines

**Scope:** how to write an individual test in this repo — structure, naming, mocking rules, when to reach for `NovaCore.TestKit`. Read [TestingArchitecture.md](TestingArchitecture.md) first if you haven't seen the `/tests` layout yet.

## Structure — AAA, one behavior per test

```csharp
[Fact]
public void Create_NameExceedsMaxLength_ThrowsInvalidArgumentException()
{
    var tooLong = new string('a', 256);

    Action act = () => Product.Create(id, code, tooLong, ...);

    act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
}
```

Arrange / Act / Assert, blank line between each. No comments restating what the code does — the test name and the Shouldly assertion already say it. A `[Theory]`/`[MemberData]` parametrizing the *same* behavior over several inputs is still "one behavior," not several tests — use it instead of copy-pasting near-identical `[Fact]`s (see `ExceptionFactoryTests` for the pattern).

## Naming

`MethodOrScenario_Condition_ExpectedResult` — e.g. `Touch_UpdatesUpdatedAt_ButNotCreatedAt`, `Sku_TooLong_ThrowsInvalidArgumentException`. Test class name matches the production type it covers, suffixed `Tests` (`Sku` → `SkuTests`), in a matching folder path so the test tree mirrors the production tree.

## Mocking rules

Mock **only true external collaborators**: repositories, `IUnitOfWork`, `ICacheService`, HTTP clients, message publishers. Use NSubstitute (`substitute.Method(args).Returns(value)`).

**Never mock:**
- Value Objects, Entities, Aggregates — construct real ones (via their `Create` factory or a `TestDataBuilder`)
- Domain Services / pure functions — call them for real
- `NovaCore.TestKit` fakes' own callers — the fakes (`FakeCurrentUserService`, `FakeAppLogger<T>`) exist precisely so you don't need a mocking framework for these; use them directly, don't wrap them in a mock too.

If you find yourself mocking a Value Object or Entity to make a test pass, the test is verifying the mock's behavior, not the production code's — stop and construct the real thing instead.

## When to build a `TestDataBuilder`

Reach for a `TestDataBuilder<T>` subclass (see `NovaCore.TestKit.Builders`) when a type's `Create` factory takes more than ~4 arguments and most tests only care about overriding one or two of them (e.g. an aggregate like `Product` or `NotificationCampaign`). For a single-argument Value Object factory (`Sku.Create(value)`), just call it directly — a builder would add indirection with no readability win.

```csharp
public sealed class ProductBuilder : TestDataBuilder<Product>
{
    private string _name = "Test Product";
    private readonly List<VariantCreateModel> _variations = [DefaultVariation()];

    public ProductBuilder WithName(string name) { _name = name; return this; }

    public override Product Build() =>
        Product.Create(TestId.NewGuid(), ProductCode.Create($"P-{TestId.Suffix()}"), _name, "desc", Slug.Create(_name), _variations);
}
```

Arrange in a test then reads as `var product = new ProductBuilder().WithName("Widget").Build();` — 1 line instead of re-deriving every constructor argument per test.

## Exception assertions

Domain rule violations always throw a `DomainException` subtype carrying a `MessageCode` (see `docs/reference/exceptions.md`). Assert both with the TestKit helper rather than a bare `Should.Throw<T>`:

```csharp
Action act = () => Sku.Create("");
act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
```

## What NOT to test

- Framework/BCL behavior (don't test that `string.Join` joins strings)
- Auto-generated boilerplate (DI registration wiring itself — that's covered by the service actually starting, not a unit test)
- Private implementation details reachable only via reflection — test the public contract
- Getters/setters with no logic

Quality over coverage percentage. A test earns its place by protecting a business rule, a reusable abstraction's contract, or documenting a previously-fixed bug (see "Bug fixes" below).

## Bug fixes

Reproduce with a failing test *before* fixing the underlying code — the failing test is the proof the bug existed and stays in the suite as a regression guard afterward. Name it after the scenario, not the ticket/issue number (numbers rot; the scenario doesn't).

## When production code changes, update tests

- **New Value Object** → full unit test suite for `Create`/`TryCreate`: every validation branch, normalization behavior, equality.
- **New Entity/Aggregate method** → test every business rule/invariant it enforces, including the failure paths.
- **New Domain Service** → test all business rules, not just the happy path.
- **New Application handler** → one test per branch (success + each validation/business failure), mocking only the repository/`IUnitOfWork`/external services it depends on.
- **Modified aggregate invariant** → update every existing test that constructed the old (now-invalid) state.
- **New reusable `BuildingBlock.*` infrastructure** → unit tests belong in the matching `BuildingBlock.*.Tests` project, using `NovaCore.TestKit` where applicable.
- **New integration (Kafka producer/consumer, external HTTP call, cache decorator)** → decide if an integration test is warranted (Phase 5 territory); at minimum, unit-test the pure logic around the integration point with the collaborator mocked.

This list is also linked from the relevant `docs/workflows/*.md` checklists — see [05-context-loading-map.md](../05-context-loading-map.md).
