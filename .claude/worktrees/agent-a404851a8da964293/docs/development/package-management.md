# Centralized NuGet Package Management

NovaCore uses .NET's [Central Package Management (CPM)](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) to control shared NuGet package versions from a single file instead of repeating `Version="..."` in every `.csproj`.

## Why

Before CPM, each of the ~50 projects declared its own `PackageReference` versions. The same package (e.g. `Microsoft.EntityFrameworkCore`, `Serilog`, `MediatR`) was frequently pinned to slightly different versions in different services purely by accident, with no single place to see or change "what version of X are we actually on."

CPM fixes this by making the version declaration and the package reference two separate concerns:
- **`Directory.Packages.props`** declares *what version a package defaults to* solution-wide.
- Each project's `.csproj` declares *that it uses the package*, without a version.

This does not reduce flexibility — see [Overriding a version for one project](#overriding-a-version-for-one-project) below. It only removes the accidental drift.

## Folder structure

```
/Directory.Packages.props        # central versions for everything under src/ and ApiGateways/
/tests/Directory.Packages.props  # central versions for test-only packages (xunit, Shouldly, Testcontainers, ...)
/tests/Directory.Build.props     # sets ManagePackageVersionsCentrally=true + common test TFM/settings
```

Two `Directory.Packages.props` files exist by design, not by accident:

- The root file governs production code (`src/`, `ApiGateways/`). MSBuild auto-imports the **nearest** `Directory.Packages.props` walking up from a project's folder, so every project under `src/` and `ApiGateways/` picks up the root file.
- `tests/Directory.Packages.props` is closer to the test projects, so they pick up their own file instead, and only ever see test-scoped packages (`xunit`, `Shouldly`, `NSubstitute`, `Testcontainers.PostgreSql`, etc.). This keeps test tooling versions from leaking into production dependency graphs and vice versa.

Do not add a `<Import>` chain between the two files — they're intentionally isolated.

## How to add a new package

1. Add the package to the appropriate `Directory.Packages.props`:
   - `src/` or gateway code → root `Directory.Packages.props`
   - test-only tooling → `tests/Directory.Packages.props`

   ```xml
   <PackageVersion Include="Some.New.Package" Version="1.2.3" />
   ```

2. Reference it from the project **without a version**:

   ```xml
   <PackageReference Include="Some.New.Package" />
   ```

3. If the package is only ever going to be used by one project (a service-specific SDK, a one-off integration, an experimental library), it's fine to skip step 1 and keep the version local to that project:

   ```xml
   <PackageReference Include="Some.Service.Specific.Sdk" Version="1.0.0" />
   ```

   CPM does not require every package to be centralized — only packages shared across multiple projects benefit from it.

## How to override a version for one project

Central versions are defaults, not a floor or ceiling. Any project can pin its own version with `VersionOverride`:

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" VersionOverride="9.0.0" />
```

This is the mechanism for staged rollouts, compatibility holdouts, or a service that genuinely can't upgrade yet. Always add a one-line comment above the override explaining *why* it differs from the central version, so the next person doesn't "fix" it by accident. Two existing examples in this repo:

- `BuildingBlock.Web.csproj` overrides `Microsoft.AspNetCore.OpenApi` / `Microsoft.OpenApi` / `Swashbuckle.AspNetCore` to older versions than the rest of the solution — not yet verified against the newer stack used by `YarpApiGateway`.
- `Auth.Application.csproj` overrides `MediatR` to `14.2.0` (the rest of the solution is on `12.5.0`) — this service has not been migrated to the newer MediatR API.

## Recommended upgrade workflow

Do not bump a central version directly for a "quick" upgrade. Central versions affect every project that doesn't already have an override, so an untested bump can break many services at once. Instead:

1. Choose a package to upgrade.
2. Add a `VersionOverride` in one or two target services first.
3. Build and test only those services.
4. Deploy to development/staging and verify runtime behavior.
5. Once confirmed, move the new version into `Directory.Packages.props` and delete the now-redundant `VersionOverride`.
6. Rebuild the full solution to confirm nothing else broke.

This keeps upgrades reversible and scoped, instead of an all-or-nothing solution-wide bump.

## Common mistakes to avoid

- **Adding `Version="..."` back onto a `PackageReference`.** Under CPM this is either ignored or causes an `NU1008` restore error ("Projects using Central Package Management must define a Version value on a PackageVersion item"). Use `VersionOverride` instead.
- **Bumping the central version to "fix" a downgrade error (`NU1109`) without checking what actually needs the higher version.** Read the dependency chain in the error message — often only one project's transitive graph needs the bump, and a `VersionOverride` on that project is safer than moving the whole solution.
- **Forgetting a project references a package transitively.** If a project doesn't declare a package directly but a `ProjectReference` it depends on overrides that package to a higher version, you may need to also override it in the downstream project's own `PackageReference` (if the package appears there too) to avoid a version-downgrade conflict at restore time.
- **Adding test packages to the root `Directory.Packages.props`, or vice versa.** Keep the two files scoped to their respective directory trees.
- **Enabling `CentralPackageTransitivePinningEnabled` without checking the whole graph.** This repo has it **disabled** at the root (it's on for `tests/`, where the dependency graph is much shallower). With it enabled, CPM injects a version floor for every package transitively touched anywhere in the solution — including packages a project never references directly — which turned ordinary intentional overrides (like the `MediatR` one above) into hard `NU1109` restore failures for unrelated downstream projects. If you re-enable it, expect to add matching overrides everywhere a `VersionOverride`'d package is consumed transitively.
