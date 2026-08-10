# Product Service

**Scope:** Product-specific facts and its documented divergences from the [User Service](user-service.md) reference implementation. General patterns live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md) — not repeated here. Product's Domain layer is also the reference implementation for [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md), the project's binding Domain-layer style rules — every future Domain entity/aggregate in any service follows those rules, not just Product.

## Projects

`Product.Domain`, `Product.Application`, `Product.Infrastructure`, `Product.Persistence`, `Product.API` — same 5-layer split as User. The Product-specific half of the Search read-model integration lives inside `Product.Persistence/Contexts/Products/Search/`, beside that context's `Read`/`Write`/`Repositories` — there is no separate `Product.Persistence.Elasticsearch` project. See [reference/search.md](../reference/search.md).

## Aggregate model (redesigned, then style-refactored)

Product went through a domain redesign (flat entity → aggregate) and then a style refactor (2026-07-17) to align with [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md), the project's binding Domain-layer conventions. **Product is the aggregate root; every SKU-level detail lives on `Variant`, an owned child entity.** A Product cannot exist without at least one variation, and exactly one variation is always the Default.

- **Product** (`Product.Domain/Entities/Product.cs`) — `Code` (`ProductCode` VO, unique, shared style/model code — distinct from a variation's own `Sku`), `Name`, `Description`, `Slug` (`Slug` VO, unique), `Metadata` (`ProductMetadata`), `CategoryMappings`/`TagMappings` (`ICollection<ProductCategoryMapping>`/`ICollection<ProductTagMapping>` — explicit mapping entities, not id collections; see "Many-to-many" below), `Variations` (`ICollection<Variant> { get; private set; } = [];` — a normal EF navigation property with a private setter, not a backing-field + `IReadOnlyCollection` wrapper). `Create(id, code, name, description, slug, IEnumerable<VariantCreateModel> variations, metadata?)` takes the complete initial variation collection directly and resolves every cross-item invariant internally — requires at least one variation, constructs each one, and resolves exactly one Default (the model flagged `IsDefault: true`, or the first one if none is flagged) — the caller never splits the collection into "first" + "rest" itself. `AddVariation(...)` (flat parameters, not a Spec object)/`RemoveVariation`/`SetDefaultVariation`/`AssignCategory`/`RemoveCategory`/`AssignTag`/`RemoveTag` enforce "≥1 variation" and "exactly one Default" — `RemoveVariation` refuses to remove the last variation and auto-promotes the lowest-`DisplayOrder` remaining variation to Default if the one removed was it. `IAuditable`, registered as its own Aggregate Root in `ConfigureAuditHierarchy`.
- **Variant** (`Product.Domain/Entities/Variant.cs`) — `Sku`/`Barcode` (VOs), `Price`, `Cost`, `Weight`, `Dimensions` (VO: Length/Width/Height), `Images` (`ICollection<string> { get; private set; } = [];`), `Status` (`VariantStatus`: Active/Inactive/Discontinued), `IsDefault`, `DisplayOrder`, `Metadata` (`VariantMetadata`). No independent identity outside its Product (no repository, no separate aggregate root) — `Create`/`MarkAsDefault`/`UnmarkAsDefault` are `internal`, reachable only through `Product`'s methods, and take flat parameters (`Guid id, Guid productId, Sku sku, decimal price, int displayOrder, Barcode? barcode = null, ...`), never a Spec/DTO wrapper; single-entity mutations that don't affect cross-variation invariants (pricing, images, physical attributes, status) are public directly on the entity. Exposes `IsValidPrice`/`IsValidCost`/`IsValidWeight` so FluentValidation can reuse the exact same rule Domain enforces.
- **VariantCreateModel** (`Product.Domain/Entities/VariantCreateModel.cs`) — the one intentional exception to "no DTO-like objects in Domain": the element type of the `IEnumerable<...>` `Product.Create`'s bulk factory accepts. A collection of N structured items has no flat-parameter equivalent; every single-item Domain method still takes flat parameters. See [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md#the-one-intentional-exception-collection-element-shapes-for-rule-1).
- **ProductCategoryMapping** / **ProductTagMapping** (`Product.Domain/Entities/`) — explicit many-to-many mapping entities (`BaseEntity<Guid>` with `ProductId`+`CategoryId`/`TagId`), constructed only via `internal static Create(...)`, reachable only through `Product.AssignCategory`/`AssignTag`. Not primitive id collections.
- **ProductCategory** (`Product.Domain/Entities/ProductCategory.cs`) — `Code` (`CategoryCode` VO, unique), `Name`, `Description`, `Status`, `ParentCategoryId` (self-referencing, nullable). Independent aggregate root, not owned by Product. No `Children` navigation — descendant traversal is a read-side query concern (`IProductCategoryReadService.GetChildIdsAsync`), not an in-memory Domain collection (a category tree can be arbitrarily deep; materializing it inside one aggregate instance would break the aggregate-per-transaction boundary). `ChangeParent` only guards direct self-parenting; deeper cycle detection (moving a category under its own descendant) is an Application-layer check (`UpdateProductCategoryHandler.EnsureNoCycleAsync`, walks the parent chain via repeated `ReadService` lookups) since it spans multiple aggregate instances.
- **ProductTag** (`Product.Domain/Entities/ProductTag.cs`) — `Code` (`TagCode` VO, unique), `Name`. Flat, no hierarchy. Independent aggregate root.
- **Value Objects** (`Product.Domain/ValueObjects/`) — `Sku`, `Barcode`, `Slug`, `ProductCode`, `CategoryCode`, `TagCode` (all derive from a shared `BuildingBlock.Domain.Abstractions.StringValueObject` base), `Dimensions` (compound VO). Each exposes `IsValid(...)`/`TryCreate(...)` in addition to `Create(...)`, backed by one shared private validation method — FluentValidation calls `Sku.IsValid(...)` etc. instead of re-declaring the rule. See [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md#5-value-object-validation-is-reusable-outside-the-constructor). Seeded with one "Uncategorized" `ProductCategory` (code `UNCATEGORIZED`) by `ProductSeeder`.
- **Metadata framework** (`BuildingBlock.Domain.Metadata`, reusable across services) — `MetadataBase` wraps a private `Dictionary<string, object?>`; derived classes (`ProductMetadata`, `VariantMetadata`) declare ordinary properties wrapping `Get<T>()`/`Set(value)`, keyed automatically via `[CallerMemberName]` unless `[Metadata("key")]` overrides it. Persisted as a `jsonb` column via `MetadataBase.ToJson()`/`FromJson<T>()`.

Categories/tags are many-to-many with Product, modeled as `ProductCategoryMapping`/`ProductTagMapping` entities (Domain Rule 4) and persisted as real join tables (`product_category_mappings`/`product_tag_mappings`, unique-indexed on `(ProductId, CategoryId)`/`(ProductId, TagId)`, cascade-deleted with the Product) — **not** a `jsonb` array of ids. This superseded an earlier `HashSet<Guid> CategoryIds`/`TagIds` + `jsonb`-array design; see the "Style refactor" note below.

## Ports & routing

Internal `8080` (REST) only. Gateway path prefix `/api/product/` (`RequireAuth: true`).

## Routes (Carter endpoints, `Product.API/Endpoints/`)

| Method | Route | Purpose |
|---|---|---|
| POST | `/products` | Create a product together with all initial variations in one request (RequireAdmin) |
| GET | `/products/{productId}` | Fetch a product with its variations — reads Postgres directly, unlike the list endpoint below (RequireAuthenticated) |
| GET | `/products` | Paginated/searchable/filterable list, served entirely from Elasticsearch (`search`, `categoryId`, `tagId`, `status`, `sortBy`, `sortDescending`, `page`, `pageSize`) — see [reference/search.md](../reference/search.md) (RequireAuthenticated) |
| POST | `/products/search/rebuild` | Drop + recreate the Elasticsearch index and repopulate it entirely from Postgres — see [reference/search.md](../reference/search.md) (RequireAdmin) |
| PUT | `/products/{productId}` | Update Name/Description/Slug only — never touches variations (RequireAdmin) |
| DELETE | `/products/{productId}` | Hard delete, cascades to owned variations (RequireAdmin) |
| POST | `/products/{productId}/variations` | Add a variation; `DisplayOrder` is server-assigned (RequireAdmin) |
| PUT | `/products/{productId}/variations/{variationId}` | Update a variation's Sku/Barcode/Price/Cost/Weight/Dimensions/Images/Status (never DisplayOrder/IsDefault) (RequireAdmin) |
| DELETE | `/products/{productId}/variations/{variationId}` | Remove a variation (last one refused) (RequireAdmin) |
| POST | `/products/{productId}/variations/{variationId}/default` | Change the Default variation (RequireAdmin) |
| POST | `/products/{productId}/variations/reorder` | Reassign DisplayOrder for every variation (RequireAdmin) |
| POST / DELETE | `/products/{productId}/categories/{categoryId}` | Assign/remove a category (RequireAdmin) |
| POST / DELETE | `/products/{productId}/tags/{tagId}` | Assign/remove a tag (RequireAdmin) |
| POST / GET / PUT / DELETE | `/categories`, `/categories/{categoryId}` | ProductCategory CRUD + flat list (RequireAdmin for writes, RequireAuthenticated for reads) |
| POST / GET / PUT / DELETE | `/tags`, `/tags/{tagId}` | ProductTag CRUD + flat list (RequireAdmin for writes, RequireAuthenticated for reads) |

Deleting a category/tag is refused (`ConflictException`) if it has children (category only) or is still assigned to any product.

## Documented divergence from User: no gRPC surface

Unlike Auth/User, Product does not bind a gRPC port — nothing currently calls into Product via gRPC.

## Messaging: Product → Inventory / Order, plus Product → itself (ten integration events)

`BuildingBlock.Contract/Events/Product/` — Product publishes each of these directly from the relevant command handler via `IOutboxStore.EnqueueAsync` in the same handler that saved the aggregate (see "Event publishing style" below), not through a Domain Event hop:

| Event | Fired by | Consumed by |
|---|---|---|
| `ProductCreatedIntegrationEvent` (ProductId, Code, Name, Slug) | `CreateProductHandler` | Order (nothing to do until a variation exists — Order doesn't build a catalog row from this alone) + Product itself (Search sync) |
| `ProductUpdatedIntegrationEvent` (ProductId, Name, Slug) | `UpdateProductHandler` | Order — refreshes `ProductName` on every `OrderProductCatalog` row for that product + Product itself (Search sync) |
| `ProductDeletedIntegrationEvent` (ProductId) | `DeleteProductHandler` | Inventory (deletes all Inventory rows for the product) + Order (deletes all catalog rows for the product) + Product itself (Search removal) |
| `VariantCreatedIntegrationEvent` (ProductId, VariantId, Sku, ProductName, Price) | `CreateProductHandler` (once per initial variation) / `AddVariationHandler` | Inventory (creates a zero-stock row against the `MAIN` warehouse) + Order (creates an `OrderProductCatalog` row keyed by variation) + Product itself (Search sync) |
| `VariantUpdatedIntegrationEvent` (ProductId, VariantId, Sku, Price) | `UpdateVariationHandler` | Order (updates Sku/Price on the catalog row) + Product itself (Search sync) |
| `VariantDeletedIntegrationEvent` (ProductId, VariantId) | `DeleteVariationHandler` | Inventory (deletes Inventory rows for that variation) + Order (deletes the catalog row) + Product itself (Search sync) |
| `ProductCategoryAssignedIntegrationEvent` (ProductId, CategoryId) | `AssignProductCategoryHandler` | Product itself (Search sync only) |
| `ProductCategoryRemovedIntegrationEvent` (ProductId, CategoryId) | `RemoveProductCategoryHandler` | Product itself (Search sync only) |
| `ProductTagAssignedIntegrationEvent` (ProductId, TagId) | `AssignProductTagHandler` | Product itself (Search sync only) |
| `ProductTagRemovedIntegrationEvent` (ProductId, TagId) | `RemoveProductTagHandler` | Product itself (Search sync only) |

Stock/catalog sync reacts to variation-level events, not `ProductCreatedIntegrationEvent` — a product is never stocked/orderable until it has a variation, which is guaranteed at creation time anyway.

**Product is now both publisher and consumer of its own events** (previously publish-only) — it self-consumes via its own Outbox → Kafka → its own Kafka consumer group to keep the Elasticsearch Search index in sync, exactly like any other cross-service consumer, just looping back to itself. The last four events above (Category/Tag assigned/removed) exist purely for this purpose — no other service consumes them. Full detail: [reference/search.md](../reference/search.md).

## Event publishing style: direct Outbox enqueue, not a Domain Event hop

Every service publishes integration events directly from the command handler via `IOutboxStore.EnqueueAsync`, in the same handler that saved the aggregate — there is no Domain Event hop (see [reference/events.md](../reference/events.md), corrected 2026-07-17 to match this). Product's command handlers (`CreateProductHandler`, `UpdateProductHandler`, `DeleteProductHandler`, `AddVariationHandler`, `UpdateVariationHandler`, `DeleteVariationHandler`) follow this same established pattern, same as every other service.

## Search (Elasticsearch read model)

Product Search (`GET /products`, `POST /products/search/rebuild`) is served entirely from Elasticsearch, never Postgres. Full architecture — reusable `BuildingBlock.Search` vs. Product-specific `Product.Persistence/Contexts/Products/Search`, sync flow, rebuild strategy, projection pattern: [reference/search.md](../reference/search.md).

Search registration (`AddProductSearchServices`) is just one more call inside `Product.Persistence`'s own `AddPersistence(configuration)`, alongside `AddRepositories`/`AddUnitOfWork`/`AddOutbox`/etc. — `Program.cs` calls only `.AddPersistence(...).AddApplication()...`, no separate composition-root step for search.

One documented deviation: after `dbContext.Database.MigrateAsync()`, `Program.cs` also calls `IProductSearchIndexer.EnsureIndexAsync()` — idempotent index/mapping bootstrap on every startup, the Elasticsearch equivalent of running EF migrations.

## Persistence: Read/Write services

Per [conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md) — Product was the hand-built reference slice this pattern was proven against (see the Course Correction in [the migration tracker](../refactoring/persistence-refactor-plan.md) for how the design settled). `Product.Application/Abstractions/Persistence/{Products,ProductCategories,ProductTags}/` hold the three aggregates' `I*ReadService`/`I*WriteService` ports. `ProductRepo`/`ProductCategoryRepo`/`ProductTagRepo` all implement the generic `IRepository<T>` in full; their own `I*Repository` interfaces are empty markers (no bulk/by-foreign-key need for any of the three) — registration is the Scrutor scan, not manual. Read Services (`ProductReadService`, etc.) query `ProductDbContext` directly and independently of the repo — `Search`/`Exists`/`GetChildIds`/`GetAll` all live there, never on the repo.

Every `ProductWriteService`/`ProductCategoryWriteService`/`ProductTagWriteService` method delegates to `repo.UpdateAsync(id, action, ct)` (or `AddAsync`/`DeleteAsync`) and returns — none of them call `SaveChangesAsync`/`ExecuteTransactionAsync` themselves. Every one of Product's mutation handlers (`CreateProductHandler`, `UpdateProductHandler`, `AddVariationHandler`, the other variation/category/tag handlers) owns `IUnitOfWork.ExecuteTransactionAsync` itself, wrapping the Write Service call(s) plus the Outbox enqueue in one transaction. `AddVariationHandler` is the one case worth calling out: its event needs the newly-generated variation's `Id`/`Sku`, which only exist *after* the mutation runs, so `AddVariationAsync` returns the created `Variant` and the handler builds the event from that return value inside the same `ExecuteTransactionAsync` — no separate method shape needed for this, since every Write Service method is already non-committing by default.

## Persistence notes

- **`Variant` is an EF owned collection** (`builder.OwnsMany(x => x.Variations, ...)`, table `variants`), same pattern as Order/OrderItem — owned rows are always loaded with the Product, no explicit `Include()`. `Variations`/`CategoryMappings`/`TagMappings` are plain `{ get; private set; }` auto-properties, so EF invokes the private setter directly — no `UsePropertyAccessMode(PropertyAccessMode.Field)` needed.
- **`CategoryMappings`/`TagMappings` are owned collections mapping to real join tables** (`product_category_mappings`/`product_tag_mappings`, unique-indexed on `(ProductId, CategoryId)`/`(ProductId, TagId)`, cascade-deleted with the Product) — `ProductConfig.ConfigureCategoryMappings`/`ConfigureTagMappings`. Membership lookups (`SearchAsync` by `categoryId`/`tagId`, `ExistsWithCategoryAsync`/`ExistsWithTagAsync`) are plain LINQ (`p.CategoryMappings.Any(m => m.CategoryId == categoryId)`) translating to a normal SQL `EXISTS` — no raw SQL needed. This replaced an earlier `jsonb`-array-of-ids + raw-SQL `jsonb @>` design during the 2026-07-17 Domain style refactor (see [conventions/domain-coding-conventions.md](../conventions/domain-coding-conventions.md#4-many-to-many-relationships-use-explicit-mapping-entities-not-primitive-id-collections)).
- **`Variant.Images` is an EF Core primitive collection** (`ICollection<string>` mapped with `.HasColumnType("jsonb")` only — no explicit `ValueConverter`/`ValueComparer`, EF Core's built-in primitive-collection support handles the `jsonb` round-trip automatically).
- **Value Objects use a two-lambda `HasConversion`** (`x => x.Value, x => Sku.Create(x)`), and `Dimensions` (a compound VO) is mapped via nested `OwnsOne` inside the `variants` owned-type builder — `ComplexProperty` isn't available from an `OwnedNavigationBuilder` in this EF Core version.
- **Sku uniqueness is global**, not per-product — enforced both by a unique index on `variants.sku` and an app-level `IProductReadService.SkuExistsAsync` check (`SelectMany` over every product's owned `Variations`).

## Naming note: the `Product` entity vs. the `Product` root namespace

Unchanged from before the redesign — every project in this service lives under the `Product.*` namespace root and the primary entity is also named `Product`, so `Product.Application`/`Product.Persistence` alias it:

```csharp
global using ProductEntity = Product.Domain.Entities.Product;
```

`Variant`, `ProductCategory`, `ProductTag` have no such collision and are used by their plain names. [Inventory Service](inventory-service.md) has the identical issue for its `Inventory` entity — same fix, same reasoning.

## Known issues

- Cross-aggregate cycle detection for `ProductCategory.ChangeParent` (moving a category under one of its own descendants) walks the parent chain via repeated repository round-trips in the Application handler, capped at 100 hops — fine for realistic category tree depths, not a formally bounded guarantee.
- No caching (`ICacheService`/`CacheKeys.Products`/`Categories`) wired up yet — same as before the redesign.
