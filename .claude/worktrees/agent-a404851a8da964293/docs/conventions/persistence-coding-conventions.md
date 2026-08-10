# Persistence Coding Conventions

**Scope:** binding style rules for `*.Persistence` projects and the `Abstractions/Persistence/` folder inside every `*.Application` project — the Read/Write persistence-service pattern all 7 services (User, Audit, Auth, Inventory, Product, Notification, Order) now follow. This is the standard for service #8: read this doc, not [refactoring/persistence-refactor-plan.md](../refactoring/persistence-refactor-plan.md) (the migration's historical tracker — it records *how* the pattern was arrived at, including a mid-migration course correction, but this doc is the settled shape to copy). For layer *responsibilities* (Application vs. Persistence boundaries), see [02-architecture-rules.md](../02-architecture-rules.md#layer-responsibilities); this doc is about *how* a Persistence-layer class is shaped once you're inside that boundary, the same relationship [conventions/application-coding-conventions.md](application-coding-conventions.md) has to [04-coding-rules.md](../04-coding-rules.md).

## Why a Read/Write split

Before this pattern, Application handlers injected repository interfaces directly, and a handful of them leaked an EF `Func<IQueryable<T>, IQueryable<T>> includes` parameter across the Application/Persistence boundary so the handler could shape its own `Include`/`ThenInclude` chain. That's a real EF Core dependency living inside Application code. Splitting into a Read side (queries, projections, `Include` composition — all EF/Mongo-specific, all internal to Persistence) and a Write side (load-modify-save workflows) removes that leak entirely and gives each aggregate root a single, intent-named surface Application actually calls.

## Folder structure

Per aggregate root, inside `<Service>.Persistence/`:

```
<Service>.Persistence/
├── <Aggregates>/                      (plural — see Naming below)
│   ├── Read/<Aggregate>ReadService.cs      — implements I<Aggregate>ReadService
│   ├── Write/<Aggregate>WriteService.cs    — implements I<Aggregate>WriteService
│   └── Repositories/
│       ├── I<Aggregate>Repository.cs       — Persistence-internal, may be an empty marker
│       └── <Aggregate>Repo.cs
├── Configs/, Outbox/, Inbox/, Migrations/, Seeders/, Saga/, UnitOfWork/   — stay flat, cross-cutting
├── <Service>DbContext.cs / <Service>MongoContext.cs                      — project root
└── DependencyInjection.cs
```

Single-aggregate services (User's `UserProfile`, Audit's `AuditLogEntry`) collapse to one aggregate folder — don't pre-build structure for aggregates that don't exist. Product's `Product.Persistence` additionally groups everything under `Engine/` (DbContext + UnitOfWork), `Reliability/` (Outbox/Inbox), `Storage/` (Migrations/Seeders) and `Contexts/<Aggregate>/` — that regrouping is Product-specific (it was the hand-built reference slice this whole pattern was proven against) and is **not** required for other services; the flat layout above (aggregate folders directly under the project root, cross-cutting concerns staying where they already were) is equally valid and is what User/Audit/Auth/Inventory/Notification/Order all use. Don't physically reorganize an existing service's unrelated folders (`Configs/`, `Migrations/`, ...) just to match Product — that was a one-time artifact of the migration's reference implementation, not a rule.

### Search belongs beside Read/Write/Repositories, not in its own project

A bounded context that adopts a search read-model (Elasticsearch or otherwise) owns that implementation itself — `Product.Persistence/Contexts/Products/Search/` (`ProductSearchIndexNames.cs`, `Mapping/`, `Indexers/`, `Repositories/`) sits beside `Read/`, `Write/`, and `Repositories/` under the same `Products` aggregate folder, exactly like this doc's flat layout would put it beside a single-aggregate service's other folders. There is **no** `*.Persistence.Elasticsearch` (or `*.Persistence.<Technology>`) peer project for any service: the reusable, technology-specific 20% (client registration, generic indexer, options) lives in `BuildingBlock.Search`; everything document/mapping/query-shaped is Product-specific and stays inside `Product.Persistence`. Registration is one more private method (`AddProductSearchServices(configuration)`) called from the same `AddPersistence(configuration)` composition root every other persistence capability registers from — never a standalone `Add{Technology}Persistence()` step in `Program.cs`. Future services repeat this as `Contexts/<Aggregate>/Search/` inside their own `<Service>.Persistence`, not as a new project. See [reference/search.md](../reference/search.md) for the full Product Search architecture.

## Interface ownership

`I<Aggregate>ReadService`/`I<Aggregate>WriteService` live in `<Service>.Application/Abstractions/Persistence/<Aggregate>/` — Application owns the port, the same pattern `IUnitOfWork` already follows. `I<Aggregate>Repository` lives entirely inside `<Service>.Persistence/<Aggregate>/Repositories/` — **Application never references a repository interface**. A MediatR handler (or gRPC service, background job, cache — anything outside the MediatR pipeline that used to touch a repository directly) injects `I<Aggregate>ReadService` for queries and `I<Aggregate>WriteService` for mutations.

## Mandatory: no transport objects declared beside a Service (binding solution-wide, added 2026-08-04)

A Service file (`I<Aggregate>ReadService.cs`/`I<Aggregate>WriteService.cs` and their Persistence implementations, but this also extends to any Application-layer service interface like `I<Aggregate>PreparationService`) declares **only** the service contract/implementation — never a `record`/`class`/`struct` request, response, or result type, no matter how small or single-use it looks. Every transport object has exactly one correct home, decided by why it exists:

- **A multi-parameter object a Domain factory/method needs** (an aggregate constructor or `Create` taking 5+ related values) → `<Service>.Domain/Entities/<Aggregate>/Data/` (e.g. `CreateOrderData`, consumed by `Order.Create`). Update operations should expose explicit intent-named methods instead (see Write Service responsibility below), not a matching "UpdateXData" object — only creation workflows typically need this.
- **A DTO that exists purely for Application ↔ Persistence (or Application ↔ any other layer) communication** → `<Service>.Application/Features/<Aggregate>/DTOs/`, alongside that aggregate's Commands/Queries/Events/Mapping. This is where `I<Aggregate>WriteService`'s `Create*Request` records belong (e.g. `CreateProductRequest`, `CreateOrderRequest`, `CreateWarehouseRequest`), and where a Persistence `ReadService`'s flattened projection record belongs too (e.g. `UserReadModel`). The interface file imports it with a `using`, same as any other type — it does not declare it.
- **A result/request shape reused across multiple features, not owned by one aggregate** (`ValidationResult`, a shared `OperationResult`) → `<Service>.Application/Common/` (or `Shared/`, matching whatever the service already uses).

Concretely: `IProductWriteService.cs` contains only the interface; `CreateProductRequest` lives in `Product.Application/Features/Products/DTOs/CreateProductRequest.cs` and is `using`-imported. Same shape for `IOrderWriteService`/`CreateOrderRequest`, `IWarehouseWriteService`/`CreateWarehouseRequest`, `IInventoryWriteService`/`CreateInventoryRequest`+`InventoryAdjustmentResult`, `IInventoryLotWriteService`/`CreateInventoryLotRequest`, `IInventoryReservationWriteService`/`CreateInventoryReservationRequest`, `IInventorySerialWriteService`/`CreateInventorySerialRequest`, `IUserReadService`+`IUserWriteService`/`UserReadModel`+`CreateUserRequest`+`SyncUserRequest`, `IOrderItemPreparationService`/`PreparedOrderItem` — all retrofitted 2026-08-04. Don't invent a new location for a "just this one file needs it" object; scattering transport contracts beside whichever Service happened to need one first makes them undiscoverable and hard to reuse from a second caller.

## Repository responsibility

Repositories are the reusable persistence-primitive layer — implement the shared generic `BuildingBlock.Persistence.Repository.IRepository<T>` **in full** wherever the aggregate has a genuine tracked-load-and-mutate need:

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>> includes, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    Task UpdateAsync<TId>(TId id, Func<T, Task> updateAction, CancellationToken ct = default);
    Task UpdateAsync<TId>(TId id, Func<IQueryable<T>, IQueryable<T>> includes, Func<T, Task> updateAction, CancellationToken ct = default);
    Task DeleteAsync<TId>(TId id, CancellationToken ct = default);
    Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default);
}
```

- `<Aggregate>Repo` implements both `IRepository<TEntity>` and its own `I<Aggregate>Repository`. When there's nothing beyond generic CRUD, `I<Aggregate>Repository` is an **empty marker** — `// Leave empty for now... Reserved for future scaling` — kept only so Scrutor's `AsImplementedInterfaces()` has a stable specific-interface slot to register against; nothing outside Persistence ever sees it (`ProductRepo`/`IProductRepository`, `OrderRepo`/`IOrderRepository` are both this shape today).
- Give `I<Aggregate>Repository` real methods only for **bulk workflows keyed by something other than the primary key** — a delete/update that fans out to every row matching a foreign key, not a single tracked entity. `IInventoryRepository.DeleteByProductIdAsync`/`DeleteByVariationIdAsync` and `IOrderProductCatalogRepository.UpdateProductNameByProductIdAsync`/`DeleteByProductIdAsync` are this shape: the repo method does the query-then-bulk-mutate internally (`ToListAsync` + `RemoveRange`, or a loop calling a domain method on each tracked row), and the Write Service just delegates to it with no further workflow of its own.
- Plain read-only queries (existence checks, id-list lookups, criteria search) do **not** belong on the repository at all — they belong on the Read Service, querying `TDbContext` directly. `IOrderProductCatalogReadService.GetByVariantionIdsAsync`/`ExistsAsync` are read-only and were moved off the repo for exactly this reason: nothing ever mutates through them, so routing them through a repo method would just be an unnecessary hop.
- Mongo-backed services (Audit, Notification) have **no generic `IRepository<T>`** — MongoDB.Driver has no `IQueryable`/change-tracker shape for the generic contract to describe. Their repos are thin, hand-written classes (`AddAsync` and not much else for append-only aggregates like `AuditLogEntry`), registered manually in DI rather than Scrutor-scanned. Don't force a Mongo repo to pretend to implement `IRepository<T>`.

## Read Service responsibility

`<Aggregate>ReadService` injects `TDbContext`/`TMongoContext` directly and is **completely independent of the repository** — it never delegates to it, even when a query shape happens to duplicate something the repo could theoretically answer. Every query is `AsNoTracking()` (EF) or a plain find (Mongo), owns its own `Include`/projection/pagination, and this is where a criteria-based `SearchAsync` or a paginated `GetAllAsync` lives.

```csharp
public sealed class ProductTagReadService(ProductDbContext dbContext) : IProductTagReadService
{
    public async Task<IReadOnlyList<ProductTag>> GetAllAsync(CancellationToken ct = default) =>
        await dbContext.ProductTags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);

    public async Task<bool> IsExistAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.ProductTags.AsNoTracking().AnyAsync(pt => pt.Id == id, ct);
}
```

An existence check that a handler only used to null-check (`var existing = await repo.GetByIdAsync(id, ct); if (existing is not null) ...`) should become a named `ExistsAsync`/`IsExistAsync` on the Read Service returning `bool`, not a full entity fetch kept only for its null-ness.

## Write Service responsibility

**A `WriteService` method never owns transaction lifetime.** Concretely:

- It must never call `unitOfWork.ExecuteTransactionAsync(...)` itself.
- If the *caller* commits via `ExecuteTransactionAsync` (the common case — this is what translates `DbUpdateConcurrencyException`/Postgres `23505` into `ConflictException`, see `EfUnitOfWork.ExecuteTransactionAsync`), the `WriteService` method just delegates to `repo.UpdateAsync(id, action, ct)` / `AddAsync` / `DeleteAsync` and returns — no commit call of its own. EF automatically enlists in whatever transaction the caller already opened.
- If the caller only ever commits via a **bare** `unitOfWork.SaveChangesAsync()` (no explicit transaction — a single `SaveChangesAsync` is already atomic on its own), the `WriteService` method *may* call that itself. This is the one case where injecting `IUnitOfWork` into a Write Service alongside its repo is correct (`WarehouseWriteService`, `ProductTagWriteService.CreateAsync`/`DeleteAsync`) — it is not "owning a transaction," just the default single-commit case.

Every mutation method is intent-named — never a generic `Func<TEntity, Task>` delegate crossing the Application/Persistence boundary:

```csharp
// Application-owned interface
public interface IProductTagWriteService
{
    Task CreateAsync(ProductTag tag, CancellationToken ct = default);      // self-commits (bare SaveChangesAsync)
    Task UpdateTagNameAsync(Guid id, string tagName, CancellationToken ct = default);  // caller owns the transaction
    Task DeleteAsync(Guid id, CancellationToken ct = default);             // self-commits (bare SaveChangesAsync)
}

// Persistence implementation
public sealed class ProductTagWriteService(IRepository<ProductTag> repo, IUnitOfWork unitOfWork) : IProductTagWriteService
{
    public async Task CreateAsync(ProductTag tag, CancellationToken ct = default)
    {
        await repo.AddAsync(tag, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateTagNameAsync(Guid id, string tagName, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async tag =>
        {
            tag.Rename(tagName);
            await Task.CompletedTask;
        }, ct);
        // no commit here - UpdateProductTagHandler wraps this call in its own ExecuteTransactionAsync
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
```

For an aggregate with several distinct mutations (Product has ~12: create, update details, add/update/delete/reorder/set-default variation, assign/remove category, assign/remove tag), that means **one named method per operation** on the interface — not a single generic pass-through — each internally calling `repo.UpdateAsync(id, <the exact domain-mutation lambda>, ct)`. The lambda still lives inside the Write Service method body; only the *public signature* Application calls is primitive/VO-typed instead of delegate-typed.

**Guards that used to live in the handler can live inside the Write Service's mutation lambda** when the guard is really part of that one operation, not a cross-cutting concern — `ProductWriteService.ReorderVariationsAsync` throws `BadRequestException` from inside its `repo.UpdateAsync` callback if the requested id set doesn't exactly match the existing variations; `OrderWriteService.CancelAsync`/`CompleteAsync` do the same for their status guards. This keeps the handler thin and the guard co-located with the one mutation it protects, rather than duplicated across every caller of that mutation.

**Output-dependent side effects don't need a special method shape.** Earlier drafts of this pattern had a `StageUpdateAsync` (non-committing) alongside a self-committing `UpdateAsync` for cases where an outbox event needed the mutation's own output (e.g. a newly generated child entity's id) or where multiple repositories had to commit atomically together. Since every `WriteService` method is non-committing by default now (the caller always owns the commit unless it's the simple bare-`SaveChangesAsync` case above), that distinction disappeared — a method that returns `Task<TResult>` instead of `Task` is enough:

```csharp
public async Task<(Guid CustomerId, decimal TotalAmount)> UpdateItemsAsync(
    Guid orderId, IReadOnlyCollection<OrderItemCreateModel> items, CancellationToken ct = default)
{
    var customerId = Guid.Empty;
    var totalAmount = 0m;
    await repo.UpdateAsync(orderId, async order =>
    {
        order.UpdateItems(items);
        customerId = order.CustomerId;
        totalAmount = order.TotalAmount;
        await Task.CompletedTask;
    }, ct);
    return (customerId, totalAmount);
}
```

`UpdateOrderHandler` wraps this call plus its outbox enqueue in one `ExecuteTransactionAsync` it owns, using the returned tuple to build the integration event — no nesting, no separate "stage" naming needed.

**Cross-aggregate and batched-same-aggregate transactions work the same way, just with more than one `WriteService` call inside the caller's `ExecuteTransactionAsync`.** Inventory's stock-mutation handlers (`AdjustStock`/`DeductStock`/`RestockStock`/`StockIn`/`StockOut`) inject `IUnitOfWork` plus `IInventoryWriteService`/`IInventoryTransactionWriteService`/(`IStockDeductionWriteService` where relevant) directly, and wrap multiple non-committing calls in one transaction they own. There is no special interface shape for this on the Write Service side — every method is already the right (non-committing) shape by default; only the caller's code composes them.

## Naming

- Class/interface pairs keep the codebase's existing asymmetric convention: `<Aggregate>Repo` (abbreviated) / `I<Aggregate>Repository` (full word). Read/Write services use the full word both sides: `<Aggregate>ReadService`/`I<Aggregate>ReadService`, `<Aggregate>WriteService`/`I<Aggregate>WriteService` — no established abbreviation for this newer concept.
- **Per-aggregate folder/namespace segments use the plural form** (`UserProfiles/`, `Products/`, `Inventories/`, `OrderProductCatalogs/`, ...), matching the service's own `DbSet<T>`/collection property name. A singular segment (`namespace User.Persistence.UserProfile`) collides with the `UserProfile` entity type itself — C# resolves the nested namespace before the `using`-imported type, producing `CS0118` the moment the type is referenced unqualified inside its own namespace.
- Don't rename anything that isn't moving as part of adding this pattern to a new service (existing typos, existing folder-name quirks like Auth's `Config/` vs. everyone else's `Configs/`, stay as they are unless you're touching them for an unrelated reason).

## DI registration

```csharp
private static IServiceCollection AddRepositories(this IServiceCollection services)
{
    // Scrutor: registers every concrete repo implementing IRepository<T> against both the
    // generic interface and its own specific marker/extra-methods interface in one scan.
    services.AddScopedByInterface(typeof(IRepository<>), typeof({Service}DbContext));

    // Read/Write services are one-per-aggregate - registered explicitly, not scanned.
    services.AddScoped<I{Aggregate}ReadService, {Aggregate}ReadService>();
    services.AddScoped<I{Aggregate}WriteService, {Aggregate}WriteService>();

    return services;
}
```

Mongo services skip the Scrutor line entirely (no `IRepository<T>` implementers exist) and register all three (repo + read + write) explicitly per aggregate, same as Audit/Notification do today.

## What NOT to do

- Don't inject a repository interface into an Application handler, gRPC service, background job, or cache decorator — go through the Read/Write service even for a single `GetByIdAsync` call.
- Don't add a `Func<TEntity, Task>` parameter to a Write Service's *public* interface — the lambda stays inside the implementation.
- Don't call `unitOfWork.ExecuteTransactionAsync(...)` from inside a Write Service. If you find yourself wanting to, the transaction boundary belongs one level up, in the caller.
- Don't let a Read Service delegate to the repository "to avoid duplicating a query" — the two sides are deliberately independent, even at the cost of a duplicated `Where` clause.
- Don't physically move a service's unrelated folders (`Configs/`, `Migrations/`, `Saga/`, ...) into a new grouping just to mirror Product's `Engine/Reliability/Storage/Contexts` layout — that grouping is Product-specific, not a requirement.
