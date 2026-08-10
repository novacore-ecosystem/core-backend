# Workflow: Add New Domain Entity

**Read first:** [02-architecture-rules.md](../02-architecture-rules.md#layer-responsibilities), [04-coding-rules.md](../04-coding-rules.md), [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md) (binding style rules — aggregate creation shape, no Spec objects, plain navigation collections, many-to-many mapping entities, reusable Value Object validation), [06-implementation-templates.md](../06-implementation-templates.md#domain-entity).

## Steps

1. **Decide entity vs value object vs aggregate root.** Has its own identity/lifecycle → `BaseEntity<TId>`. Owns a collection of child entities, or is a transaction/consistency boundary → `AggregateRoot<TId>` (a marker base type — it does not raise events itself, see below). No identity, structural equality → `ValueObject`. See `BuildingBlock.Domain/Abstractions/`.
2. **Create `{Service}.Domain/Entities/{Entity}.cs`** — private constructor (EF Core), static `Create(...)` factory method that validates invariants and throws via `ExceptionFactory` (never raw exceptions — see [reference/exceptions.md](../reference/exceptions.md#domain-exceptions)). If this is an aggregate root that owns a required collection (e.g. `Product`/`Variant`), `Create` takes the full initial collection directly and resolves every cross-item invariant internally — see [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md#1-aggregate-creation-takes-the-collection-directly-not-a-spec-wrapper), not the simple single-entity template.
3. **Add behavior methods** (not public setters) for every state change — this is how the aggregate protects its invariants (`AddVariation`, `RemoveVariation`, `SetDefaultVariation`, `AssignCategory`, `RemoveCategory`, ... — named after the business action, never a generic `Update`). Single-entity methods take flat parameters, not a Spec/DTO-like wrapper, however long the list gets — see [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md#2-no-specrequestcommanddto-like-objects-inside-domain--flat-parameters-instead). If another part of the system needs to react to this change, publish that reaction from the **Application-layer command handler** that calls the entity method — an Internal event for same-service reactions, an Integration event (`IOutboxStore.EnqueueAsync`) for cross-service ones. Do not look for a way to raise an event from inside the entity itself — there is no such mechanism (`AggregateRoot<TId>` has no `RaiseDomainEvent` method); see [reference/events.md](../reference/events.md).
4. **Owns a collection?** Use a normal EF navigation property with a private setter (`public ICollection<Child> Children { get; private set; } = [];`), not a private backing field + `IReadOnlyCollection` wrapper, and — if the collection represents a many-to-many relationship to another aggregate root — use an explicit mapping entity (`ProductCategoryMapping`-shaped), never a primitive `HashSet<Guid>`/`List<Guid>`. See [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md#3-aggregate-collections-are-normal-navigation-properties-not-backing-field-wrappers).
5. **Value Object with validation logic?** Expose the validation as `IsValid(...)`/`TryCreate(...)` in addition to `Create(...)`, backed by one shared private validation method, so FluentValidation can reuse the exact same rule instead of re-declaring it. See [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md#5-value-object-validation-is-reusable-outside-the-constructor).
6. **Add the EF configuration** — `{Service}.Persistence/Config/{Entity}Config.cs` implementing `IEntityTypeConfiguration<{Entity}>`.
7. **Register the `DbSet<{Entity}>`** on `{Service}DbContext`.
8. **Generate a migration** (`dotnet ef migrations add ...` in the Persistence project — see the service's existing `Migrations/` folder for naming convention).
9. **Add a `MessageCode` entry** if the entity needs entity-specific validation error codes — see `BuildingBlock.Domain/Enums/MessageCode.cs`, pick the correct per-service range (documented at the top of the enum).
10. **Add unit tests** in `{Service}.Domain.Tests` (create the project if this is the first Domain test for the service — see [testing/TestingArchitecture.md](../testing/TestingArchitecture.md)): every validation branch in `Create`/`TryCreate`, every behavior method's invariant (including its failure paths), and Value Object equality/normalization if applicable. See [testing/TestingGuidelines.md](../testing/TestingGuidelines.md).

## Checklist

- [ ] No public setters — all mutation through named behavior methods
- [ ] Invariants enforced in `Create(...)` and mutation methods, throwing `ExceptionFactory.*` results
- [ ] Single-entity Domain methods take flat parameters — no Spec/Request/Command/DTO-like parameter object
- [ ] Collections are `ICollection<T> { get; private set; }`, not a backing-field + `IReadOnlyCollection` wrapper; many-to-many uses a mapping entity, not a primitive id collection
- [ ] Value Object validation logic is reusable via `IsValid`/`TryCreate`, not only enforced inside `Create`
- [ ] Any resulting Internal/Integration event is published from the Application-layer handler, not from inside the entity
- [ ] EF configuration added, migration generated
- [ ] Did NOT put business logic in the EF configuration or the repository — only mapping there
- [ ] Unit tests added for every validation branch and behavior-method invariant
