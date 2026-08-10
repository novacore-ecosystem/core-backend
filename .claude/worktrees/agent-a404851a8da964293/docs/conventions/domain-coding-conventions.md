# Domain Coding Conventions

**Scope:** binding style rules for `*.Domain` projects — the default for every future Domain entity/aggregate in this codebase, in every service, not just Product. Product's Domain layer (`Product.Domain/Entities/`) is the reference implementation these rules were extracted from during a style refactor (2026-07-17); see [services/product-service.md](../services/product-service.md#aggregate-model-redesigned-then-style-refactored) for how they read in a real, complete aggregate. These are style/implementation conventions, not architecture — they don't change layer responsibilities or dependency direction (see [02-architecture-rules.md](../02-architecture-rules.md) for those). For Application-layer conventions, see [conventions/application-coding-conventions.md](application-coding-conventions.md).

## 0. Aggregates protect invariants through methods, not through exposed state

An aggregate root owns its collections and everything reachable through them. Every state change — adding/removing a child, changing which one is "the" default, attaching/detaching a many-to-many relationship — goes through a named method on the aggregate (`AddVariation`, `RemoveVariation`, `SetDefaultVariation`, `AssignCategory`, `RemoveCategory`, `AssignTag`, `RemoveTag`), never through a public setter or a caller mutating a collection directly. A method's name says what business action happened, not `Update{Field}`. This is the umbrella rule the other six rules below all serve — flat parameters, plain navigation collections, and mapping entities all exist so that the invariant-protecting *methods* stay the only reachable way to mutate the aggregate, without Domain having to lean on DTO objects or hidden collection types to enforce it.

## 1. Aggregate creation takes the collection directly, not a Spec wrapper

When an aggregate root naturally owns a collection and requires at least one element to exist meaningfully (e.g. `Product` requires ≥1 `Variant`), its `Create` factory accepts the collection directly:

```csharp
public static Product Create(
    Guid id,
    ProductCode code,
    string name,
    string description,
    Slug slug,
    IEnumerable<VariantCreateModel> variations,
    ProductMetadata? metadata = null)
```

The factory is responsible for the entire invariant, end to end:
- requiring at least one element (throw `EmptyCollectionException` via `ExceptionFactory.EmptyCollection`, not a caller-side check)
- initializing every child from the collection
- resolving whichever single-entity invariant applies across the collection (e.g. "exactly one Default variation") **internally** — the caller does not pick an index or split the collection into "the first one" vs "the rest"

Callers never construct a temporary "first item" plus "remaining items" split — see `Product.Create` (`Product.Domain/Entities/Product.cs`) for the reference implementation, and compare against the pre-refactor version where `CreateProductHandler` used to compute `defaultIndex` and call `Product.Create(initialVariation)` + a loop of `AddVariation(remaining)` — that logic now lives entirely in `Product.Create`.

## 2. No Spec/Request/Command/DTO-like objects inside Domain — flat parameters instead

Domain methods that operate on **one** entity take flat parameters, however long the list gets:

```csharp
// Variant.Create (internal - only Product may call it)
internal static Variant Create(
    Guid id,
    Guid productId,
    Sku sku,
    decimal price,
    int displayOrder,
    Barcode? barcode = null,
    decimal? cost = null,
    decimal? weight = null,
    Dimensions? dimensions = null,
    IEnumerable<string>? images = null,
    VariantStatus status = VariantStatus.Active,
    VariantMetadata? metadata = null)
```

Rationale: call sites stay explicit at the call site (you can see every argument being passed without opening a second file), adding/removing a parameter only touches the methods that actually use it instead of every place that constructs a wrapper object, and no DTO-shaped type leaks into the Domain layer.

**This rule is scoped to Domain.** Application-layer `{Verb}Command`/`{Verb}Query` records (see [04-coding-rules.md](../04-coding-rules.md#cqrs-shape)) are unaffected — CQRS command/query objects are the established, correct shape for that layer and are not "Spec objects reducing parameter count," they're the MediatR message contract.

### The one intentional exception: collection-element shapes for Rule 1

`VariantCreateModel` (`Product.Domain/Entities/VariantCreateModel.cs`) is a record, used only as the element type of the `IEnumerable<...>` a bulk-`Create` factory accepts (Rule 1). This is **not** a violation of Rule 2 — Rule 2 targets methods that construct/mutate a single entity and could trivially take flat parameters instead; a *collection* of N structured items has no flat-parameter equivalent (you cannot flatten "N variations" into positional arguments). Every single-item Domain method (`Variant.Create`, `Product.AddVariation`, `UpdatePricing`, `UpdateIdentifiers`, ...) still takes flat parameters per Rule 2 — only the bulk-`Create` entry point takes the collection-of-create-models shape, and only because Rule 1 explicitly asks for it.

## 3. Aggregate collections are normal navigation properties, not backing-field wrappers

```csharp
public ICollection<Variant> Variations { get; private set; } = [];
```

— not:

```csharp
private readonly List<Variant> _variations = [];
public IReadOnlyCollection<Variant> Variations => _variations.AsReadOnly();
```

Consistency is protected through named methods on the aggregate (`AddVariation`, `RemoveVariation`, `SetDefaultVariation`, `AssignCategory`, `RemoveCategory`, `AssignTag`, `RemoveTag`, ...), not by hiding the collection behind a read-only wrapper — callers *can* technically reach `product.Variations.Add(...)` directly, but the private setter plus the fact that `Variant.Create`/`MarkAsDefault`/`UnmarkAsDefault` are `internal` (assembly-scoped) means a caller outside `Product.Domain` cannot construct a valid variation to add in the first place. The invariant is protected by what can be *constructed*, not by what can be *seen*.

This also simplifies EF Core mapping — a plain `{ get; private set; }` auto-property is usable via normal property access (EF invokes the private setter directly), so owned-collection configs no longer need `builder.Navigation(x => x.Variations).UsePropertyAccessMode(PropertyAccessMode.Field)`.

## 4. Many-to-many relationships use explicit mapping entities, not primitive id collections

Two independent aggregate roots that reference each other many-to-many (e.g. `Product`↔`ProductCategory`, `Product`↔`ProductTag`) are linked through an explicit mapping entity:

```csharp
public sealed class ProductCategoryMapping : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
    // internal Create(...) - only Product may construct one
}
```

not `HashSet<Guid> CategoryIds`/`List<Guid> CategoryIds`. The owning aggregate exposes the mapping collection (`ICollection<ProductCategoryMapping> CategoryMappings`) and mediates every change through `AssignCategory`/`RemoveCategory`-shaped methods (Rule 3 applies here too — same pattern, same private-setter + internal-construction protection).

**No surrogate `Id` on a pure mapping entity.** `ProductCategoryMapping`/`ProductTagMapping`/`ProductCollectionMapping` extend the non-generic `BaseEntity` (no `Id` property at all, only `CreatedAt`/`UpdatedAt`) — the composite key is the two foreign keys themselves, configured in Persistence via `builder.HasKey(x => new { x.ProductId, x.CategoryId })` (see `ProductCategoryMappingConfig`). Only reach for a surrogate `Id` (`BaseEntity<Guid>`) when the mapping entity has its own independent lifecycle or business identity beyond the pairing itself — `UserRoleAssignment` (`User.Domain/Entities/Users/`) is that case: it tracks grant history (`AssignedAt`/`ExpiredAt`/`Status`) rather than just recording that a pairing exists, so a User can accumulate multiple historical rows for the same `(UserId, RoleId)` pair and genuinely needs its own `Id`. A plain existence-mapping (`UserTagMapping`) does not, and stays `BaseEntity` with no `Id`, same as the Product examples above.

Rationale: a mapping entity is a real row with timestamps like every other entity in this codebase, maps to a genuine relational join table instead of a JSON/array column, and — concretely — lets Persistence-layer queries use plain LINQ (`p.CategoryMappings.Any(m => m.CategoryId == categoryId)`) instead of a raw-SQL workaround for a LINQ-untranslatable converted-property predicate. The prior JSONB-array-of-ids approach for `Product.CategoryIds`/`TagIds` (see `docs/services/product-service.md`'s revision history) is superseded by this rule — do not reintroduce it.

## 5. One-to-one relationships reuse the parent's primary key — no surrogate `Id`

When an entity is a strict 1:1 extension of another aggregate (a "detail table" — e.g. `OrderOwner` for `Order`, or `UserProfile`/`UserAvatar`/`UserSetting`/`UserSecuritySetting`/`UserPrivacySetting`/`UserNotificationSetting`/`UserPreference`/`UserActivitySummary`/`UserPermissionSnapshot` for `User`), the child's own primary key **is** the parent's id — never a separately generated `Guid.CreateVersion7()` alongside a redundant `ParentId` column that always holds the same value as some other row's key:

```csharp
public sealed class OrderOwner : BaseEntity
{
    public Guid OrderId { get; private set; }   // the primary key - shared with Order, not a surrogate
    public Guid OwnerId { get; private set; }
    // ...
}
```

```csharp
public static OrderOwner Create(Guid orderId, ...) => new OrderOwner { OrderId = orderId, ... };
```

Persistence configures this as a shared-PK 1:1 association: `builder.HasKey(x => x.OrderId)` plus `builder.HasOne<Order>().WithOne(o => o.Owner).HasForeignKey<OrderOwner>(x => x.OrderId)` (see `OrderOwnerConfig`) — no separate `Id` column, no separate unique index needed to fake uniqueness. The parent never needs a redundant shadow property either (`Order` does not also carry an `OwnerId` — the `Owner` navigation is enough); if the parent needs a quick existence check without loading the child, add a method to the child's Read Service instead of duplicating the key back onto the parent as an extra column (this is what the `User`/`UserAvatar` fix removed — `User.AvatarId` was a redundant shadow copy of `UserAvatar.UserId`, which is already the `Avatar` navigation's key).

Translation entities (see [conventions/persistence-coding-conventions.md](persistence-coding-conventions.md) for the EF side) are the one variant of this rule: since a parent can have *many* translations (one per language), the translation's `Id` still reuses the parent's id, but the primary key is the composite `(Id, LanguageCode)`, not `Id` alone — see `RoleTranslation`/`ProductTranslation`/`UserRoleTranslation`'s `Create(parentId, languageCode, ...)` factories, which do `Id = parentId` and never generate a fresh `Guid`.

## 6. Value Object validation is reusable outside the constructor

Every Value Object with validation logic exposes that logic through a shared, single source of truth so upper layers (FluentValidation) call into the Domain's own rule instead of re-declaring it:

```csharp
public sealed partial class Sku : StringValueObject
{
    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out Sku? sku) { ... }

    public static Sku Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null) throw error;
        return new Sku(Normalize(value));
    }

    private static InvalidArgumentException? GetValidationError(string? value) { ... }
}
```

`GetValidationError` is the single place the actual rule lives; `Create` throws it (construction still enforces correctness - this never becomes optional), `IsValid`/`TryCreate` just check whether it's `null`. FluentValidation validators call `Sku.IsValid(...)`/`ProductCode.IsValid(...)`/etc. — never `.Length(1, 50)` or a hand-rolled regex that could silently drift from the Domain's own rule.

This pattern extends past formal Value Objects to any Domain-level validation an upper layer needs to check ahead of construction — e.g. `Variant.IsValidPrice(decimal)`/`IsValidCost`/`IsValidWeight` and `Product.IsValidName(string?)`/`ProductCategory.IsValidName`/`ProductTag.IsValidName`, even though `Price`/`Name` aren't wrapped Value Objects. The goal (one rule, reused everywhere, no divergence between API-level and Domain-level validation) applies regardless of whether the validated thing happens to be a formal VO.

Each VO stays self-contained (its own `GetValidationError`/`Normalize`/regex) rather than sharing a generic template-method base — the six string VOs in `Product.Domain/ValueObjects/` have different normalization (uppercase codes vs. lowercase slug) and different format rules, so a shared abstraction would mostly exist to save a few lines of structurally-similar-but-not-identical code. Consistent with this codebase's general aversion to introducing abstraction ahead of a second concrete need for it.

## Summary: what changed vs. the first Product implementation pass

| Before | After |
|---|---|
| `Product.Create(..., VariantSpec initialVariation)` + caller-side default-index splitting | `Product.Create(..., IEnumerable<VariantCreateModel> variations)` — Domain resolves the Default internally |
| `VariantSpec` record passed to `Variant.Create`/`Product.AddVariation` | Flat parameters on both |
| `private readonly List<T> _x = []; IReadOnlyCollection<T> X => _x.AsReadOnly();` | `ICollection<T> X { get; private set; } = [];` |
| `Product.CategoryIds`/`TagIds` as `HashSet<Guid>`, persisted as a `jsonb` array, membership queries via raw SQL `jsonb @>` | `ProductCategoryMapping`/`ProductTagMapping` entities, `ICollection<TMapping>` navigations, plain LINQ `.Any(...)` |
| VO `Create` only; validation logic inline, re-declared by FluentValidation | VO `Create`/`TryCreate`/`IsValid`, single shared `GetValidationError`; FluentValidation calls `IsValid` |
