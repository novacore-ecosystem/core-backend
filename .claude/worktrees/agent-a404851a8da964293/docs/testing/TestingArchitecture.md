# Testing Architecture

**Scope:** the shape of `/tests` — project layout, central package management, the shared `NovaCore.TestKit` library, and library choices. Read this once to understand how the test suite is put together; read [TestingGuidelines.md](TestingGuidelines.md) for how to write an individual test.

## Layout

```
tests/
  Directory.Build.props            net10.0, Nullable, ImplicitUsings, CPM — inherited by every project below
  Directory.Packages.props         pinned versions for every test package (see "Package management")

  Common/
    NovaCore.TestKit/            shared test infrastructure — Builders, Fakes, Random, ShouldlyExtensions

  BuildingBlock.SharedKernel.Tests/   Phase 1 — SharedKernel extensions
  BuildingBlock.Domain.Tests/         Phase 2 — ValueObject/StringValueObject/ExceptionFactory/BaseEntity
  BuildingBlock.Persistence.Ef.Tests/ pre-existing — EF interceptor + audit graph builder (EF InMemory)

  {Service}.Domain.Tests/          Phase 3 (added incrementally, see TestingRoadmap.md)
  {Service}.Application.Tests/     Phase 4 (added incrementally)
```

One test project per production project (`Foo` → `Foo.Tests`), matching the repo's existing `BuildingBlock.Persistence.Ef` → `BuildingBlock.Persistence.Ef.Tests` naming — not the `*.UnitTests` suffix style, to stay consistent with what was already there.

Every new test project is added to `NovaCore.sln` under the `tests` solution folder (`NovaCore.TestKit` goes under `tests/Common`) via `dotnet sln add <path> -s tests[/Common]`.

## Central package management — scoped to `/tests` only

`tests/Directory.Build.props` and `tests/Directory.Packages.props` exist so every test project pins the same package versions once, instead of drifting across dozens of `.csproj` files as the suite grows.

**This does not affect `src/**`.** MSBuild's `Directory.Build.props`/`Directory.Packages.props` auto-import walks *up* from each project toward the drive root and stops at the first file it finds. Since the repo root has no such files, `src/**` projects (which walk `src/Services/X/X.Domain` → ... → repo root) pick up nothing; only projects physically under `tests/` see these files. Confirmed by building the whole solution after introducing them — `src/**` `.csproj` files are untouched.

A test `.csproj` looks like this — no `<TargetFramework>`, no `<PackageReference Version="...">`, just the package name:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Shouldly" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/.../Foo.csproj" />
  </ItemGroup>
</Project>
```

## Library choices

| Concern | Library | Why |
|---|---|---|
| Test framework | xUnit | Already the repo's only precedent (`BuildingBlock.Persistence.Ef.Tests`) |
| Assertions | **Shouldly** | Free/MIT, no licensing ambiguity (unlike FluentAssertions v8+, which requires a paid commercial license for for-profit use). Fluent, readable failure messages. |
| Mocking | **NSubstitute** | Free/MIT, small clean API. Chosen over Moq to avoid Moq's 2023 SponsorLink telemetry incident — reverted since, but not worth the residual trust question. |
| Test data | Hand-written **Test Data Builders** | The domain layer already exposes `Create`/`TryCreate` static factories with real validation (see `Sku.cs`); builders call those factories so tests never construct invalid domain state. AutoFixture/Bogus are not used — revisit only if manual builders become a bottleneck. |
| Persistence component tests | `Microsoft.EntityFrameworkCore.InMemory` | Already used by `BuildingBlock.Persistence.Ef.Tests` for interceptor/graph-builder tests that don't need a real database |
| Integration tests (future) | Testcontainers (not yet introduced) | For Phase 5 — real Postgres/MongoDB/Redis/Kafka/Elasticsearch, matching `docker-compose.yml` service names |

## `NovaCore.TestKit`

Shared library referenced by every unit test project. Grown by rule-of-three — add a helper here only once a second test project needs it, not speculatively.

- **`Builders/TestDataBuilder<T>`** — abstract base for fluent Test Data Builders (`With*()` + `Build()`). Concrete builders call the production `Create` factory; they never bypass it.
- **`Fakes/FakeCurrentUserService`** — settable-property fake of `ICurrentUserService`, for Application-handler tests once Phase 4 starts.
- **`Fakes/FakeAppLogger<T>`** — records every log call to a list instead of writing anywhere, so a test can assert "an error was logged."
- **`Random/TestId`** — collision-free `Guid`/short-suffix generation so parallel tests never collide on a magic literal.
- **`ShouldlyExtensions/DomainExceptionShouldlyExtensions.ShouldThrowDomainException<TException>(MessageCode)`** — asserts exception type *and* `MessageCode` in one call, since a domain rule violation's real contract is both.

`BuildingBlock.Application`'s existing `NullAuditMetadataProvider` is reused directly (it's already a no-op fake) rather than duplicated into the TestKit.

## Where integration tests will live (not yet built)

Phase 5 (Infrastructure) will introduce `{Service}.IntegrationTests` projects using Testcontainers, spinning up the same images referenced in `docker-compose.yml` (`pg`, `mongo`, `redis`, `kafka`, `elasticsearch`). These are deliberately out of scope until the Domain/Application layers are well covered — see [TestingRoadmap.md](TestingRoadmap.md).
