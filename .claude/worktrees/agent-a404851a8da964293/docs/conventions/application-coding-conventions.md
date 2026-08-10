# Application Coding Conventions

**Scope:** binding style rules for `*.Application` projects — the default for every future Command/Query/Event handler, gRPC service, integration consumer, and background job in this codebase, in every service. These are style/implementation conventions, not architecture: the Application layer's *responsibilities* (orchestration only, business rules stay in Domain, no Infrastructure/Web dependency) are already fixed by [02-architecture-rules.md](../02-architecture-rules.md#layer-responsibilities); this doc is about *how* an Application-layer class is shaped and organized once you're inside that boundary. For folder structure and CQRS shape as previously documented, see [04-coding-rules.md](../04-coding-rules.md) — this doc supersedes and extends that doc's folder-structure section with the fuller shape below; `04-coding-rules.md` now links here instead of repeating it. For Domain-layer conventions, see [conventions/domain-coding-conventions.md](domain-coding-conventions.md).

## Feature-First structure

```
{Service}.Application/
  Abstractions/{Auth,Repositories,Security,Services}/     interfaces only — no implementation
  Common/
    Extensions/       shared extension methods used across features
    Validations/       shared FluentValidation rules (e.g. a reusable RuleFor pattern), not feature-specific validators
    Constants/          Application-layer constants that don't belong in BuildingBlock.SharedKernel
    Regex/              shared GeneratedRegex definitions used by more than one feature
  Features/
    {FeatureArea}/
      Commands/{CommandName}/
        {CommandName}Command.cs
        {CommandName}Handler.cs
        {CommandName}Validator.cs        (if the command needs validation)
      Queries/{QueryName}/
        {QueryName}Query.cs
        {QueryName}Handler.cs
      Events/{EventName}/
        {EventName}Event.cs
        {EventName}Handler.cs
      DTOs/                 response/projection shapes specific to this feature area
      Mapping/
        MapsterConfig.cs     this feature's Mapster type-adapter configuration
      Utilities/             feature-local helpers — see "Utilities must remain dependency-free" below
  DependencyInjection.cs
  GlobalUsings.cs
```

`Abstractions/`, `Features/{Area}/Commands|Queries|Events`, `DependencyInjection.cs`, `GlobalUsings.cs` are already the established shape (see [04-coding-rules.md](../04-coding-rules.md#folder-structure-per-feature)). `Features/{Area}/DTOs/` and `Features/{Area}/Mapping/` already exist in the codebase (e.g. `Auth.Application/Features/Auth/DTOs`, `Auth.Application/Features/RefreshToken/Mapping`) — use them for any feature with more than a trivial response shape or any mapping worth configuring once. `Common/` and feature-level `Utilities/` are the target convention going forward for genuinely shared or feature-local helper code — introduce them the first time a feature actually needs one, don't pre-create empty folders.

**Utilities must remain dependency-free.** A `Utilities/` (or `Common/Extensions/`) class is a static, side-effect-free helper — string/collection/date shaping, not something that calls a repository or a cache. If a piece of shared logic needs dependency injection (a service, a repository, `ICurrentUserService`, ...), it is not a Utility — create a proper service abstraction in `Abstractions/Services/` with an implementation in Infrastructure/Persistence instead of smuggling a DI-dependent static helper into `Utilities/`.

## Handler philosophy

This rule applies to every orchestrator in the Application layer: Command Handlers, Query Handlers, Internal Event Handlers, Integration Consumers' target handlers, gRPC service implementations, Background Jobs.

Every orchestrator should read like a workflow, not hide one. Its purpose is to *coordinate* — the top-level method should present the overall processing flow so a developer understands what happens by reading only that method, without having to jump into five private methods to reconstruct the sequence.

```csharp
public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken ct = default)
{
    // Validate request
    await ValidateInputAsync(request, ct);

    // Execute business process
    var product = await CreateProductAsync(request, ct);

    // Publish integration events
    await PublishIntegrationEventsAsync(product, ct);

    // Refresh cache
    await RefreshCacheAsync(product, ct);

    return BuildResponse(product);
}
```

A developer should understand the workflow by reading only this method.

## Responsibility-based extraction, not mechanical extraction

**Do not extract methods mechanically or purely to reduce line count.** Extraction is justified when a chunk of code represents a distinct *responsibility* — Validation, Loading, Main processing, Persistence, Event publishing, Synchronization, Cache refresh, Response mapping are the typical responsibility boundaries in this codebase's handlers. A large responsibility becomes its own method; a tiny wrapper that only forwards to one other call does not.

**Bad** — a wrapper that adds a name but no responsibility boundary:

```csharp
Handle()
{
    ...
}

private Task SaveAsync()
{
    return repository.SaveAsync();
}
```

Don't create a method whose entire body is one call to something else — call the something else directly from `Handle`.

**Simple CRUD should remain simple.** Small, linear code is perfectly acceptable — a two-line `GetByIdHandler` that loads and maps does not need `LoadEntityAsync`/`MapToResult` extracted out of it. Extraction should improve maintainability (make the workflow easier to follow, isolate something genuinely reused), not navigation cost (forcing a reader to jump between methods to reconstruct three lines of logic).

## Private method organization

When extraction is appropriate (a handler with several real responsibilities — validation, main process, event publishing, cache refresh, etc.), group the resulting private methods by responsibility using `#region`:

```csharp
#region Validation
private async Task ValidateInputAsync(CreateProductCommand request, CancellationToken ct) { ... }
#endregion

#region Business
private async Task<ProductEntity> CreateProductAsync(CreateProductCommand request, CancellationToken ct) { ... }
#endregion

#region Events
private async Task PublishIntegrationEventsAsync(ProductEntity product, CancellationToken ct) { ... }
#endregion

#region Cache
private async Task RefreshCacheAsync(ProductEntity product, CancellationToken ct) { ... }
#endregion
```

Exact region names depend on the feature — the grouping principle (one region per responsibility, matching the top-level `Handle` method's own comments) is what's fixed, not the literal names above.

## Comments

Comments should briefly name a responsibility, not narrate implementation:

**Good:** `// Validate request`, `// Publish events`

**Avoid:** multi-line explanatory comments describing *what* the following code does step by step — well-named methods and variables already say that. A comment earns its place only when it explains something the code can't: a non-obvious constraint, a workaround, a reason a simpler approach doesn't work.

## Mapping

**Policy decision (2026-07-17), resolving the long-standing "Mapster is registered but unused" gap** (previously documented in [04-coding-rules.md](../04-coding-rules.md#mapping) and flagged in [07-solid-recommendations.md](../07-solid-recommendations.md#cross-cutting-observation-mapster-is-dead-code-not-a-pattern) as an inconsistency worth resolving one way or the other): **new features use Mapster for straightforward mapping.** Configure the mapping once per feature in `Features/{Area}/Mapping/MapsterConfig.cs`, then call `.Adapt<T>()` at the call site instead of hand-mapping field by field. Only fall back to manual mapping when the mapping itself carries business rules (conditional fields, computed values, cross-entity composition) that don't belong in a declarative type-adapter config.

Existing hand-mapped features (all of Auth/User's current handlers, and Product/Inventory/Order's first implementation pass) are **not** retrofitted as part of adopting this policy — that would be a pure-refactor change with no behavior difference, out of scope unless a feature is already being touched for another reason. If you're modifying a handler that already hand-maps and it's a small, low-risk change to introduce `.Adapt<T>()` at the same time, prefer doing so; don't go out of your way to convert unrelated code in the same change.

## Validation

- FluentValidation, one validator class per command/query, co-located in the same feature folder (unchanged from [04-coding-rules.md](../04-coding-rules.md#validation)).
- **Reuse Domain validation.** If the field being validated has a Domain-level rule (a Value Object, or an entity's `IsValid*` method — see [conventions/domain-coding-conventions.md](domain-coding-conventions.md#5-value-object-validation-is-reusable-outside-the-constructor)), call it (`Must(Sku.IsValid)`, `Must(Variant.IsValidPrice)`) instead of re-declaring the rule (a hand-rolled regex, a duplicated length check). The Domain stays the single source of truth; a validator only re-implements a rule when it's genuinely HTTP/input-shape-specific (e.g. a `MaximumLength` on a free-text field the Domain itself doesn't bound).
- Shared validation logic that isn't tied to one command (a rule reused across several validators, not owned by Domain) lives in `Common/Validations/`, not copy-pasted into each validator.

## Constants

Shared, cross-feature constants belong in `BuildingBlock.SharedKernel`/other `BuildingBlock.*` projects (already established — see [03-building-blocks-reference.md](../03-building-blocks-reference.md#sharedkernel)) if they're used across services, or `{Service}.Application/Common/Constants/` if they're shared across features within one service but not meant for `BuildingBlock.SharedKernel`. Feature-specific constants (a value only one feature cares about) stay inside that feature's own folder. Avoid scattered magic strings/numbers in handler bodies regardless of which of these three homes is correct.

## Regex

Prefer `[GeneratedRegex]` (source-generated, already the established pattern in Domain Value Objects — see `Sku.cs`'s `SkuFormat()`). A Regex pattern used by more than one class should be declared exactly once — in `Common/Regex/` if it's Application-layer and cross-feature, or left where it's declared if only one class uses it. Don't declare the same pattern independently in two places.
