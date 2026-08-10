# Workflow: Add New Repository (+ Read/Write Persistence Service)

**Read first:** [conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md) (the binding pattern this workflow implements), [06-implementation-templates.md](../06-implementation-templates.md#repository-interface--implementation).

Adding persistence for a new aggregate root means three things, not one: a repository (Persistence-internal), a Read Service (Application-owned port), a Write Service (Application-owned port). Application never sees the repository interface.

## Steps

1. **Decide the repository's shape.**
   - If the aggregate needs a genuine tracked-load-and-mutate workflow (create, update-in-place, delete by id), `{Aggregate}Repo` implements the generic `BuildingBlock.Persistence.Repository.IRepository<T>` in full. Its own `I{Aggregate}Repository` is an **empty marker** interface unless step 2 applies.
   - Mongo-backed aggregates skip `IRepository<T>` entirely (no `IQueryable`/change-tracker shape to satisfy it) — write a thin hand-rolled repo instead (see Audit/Notification).
2. **Add real methods to `I{Aggregate}Repository` only for bulk workflows keyed by something other than the primary key** — a delete/update that fans out to every row matching a foreign key (`DeleteByProductIdAsync`, `UpdateProductNameByProductIdAsync`), not a single tracked entity. Implement the query-then-bulk-mutate entirely inside that repo method.
3. **Implement the repo** in `{Service}.Persistence/{Aggregates}/Repositories/{Aggregate}Repo.cs`, implementing both `IRepository<{Entity}>` (if applicable) and `I{Aggregate}Repository`.
4. **Add the Read Service.** `I{Aggregate}ReadService` in `{Service}.Application/Abstractions/Persistence/{Aggregates}/`; `{Aggregate}ReadService` in `{Service}.Persistence/{Aggregates}/Read/`, injecting `{Service}DbContext`/`{Service}MongoContext` **directly** — never delegates to the repository, even when a query shape duplicates something the repo could theoretically answer. Every query is `AsNoTracking()` (EF) or a plain find (Mongo). This is where `Include`/projection/pagination/criteria search/existence checks live.
5. **Add the Write Service.** `I{Aggregate}WriteService` in `{Service}.Application/Abstractions/Persistence/{Aggregates}/` with **one intent-named method per mutation** (`UpdateTagNameAsync(id, tagName, ct)`, not `UpdateAsync(id, Func<T, Task> action, ct)`); `{Aggregate}WriteService` in `{Service}.Persistence/{Aggregates}/Write/`, injecting the repo (and `IUnitOfWork` only if it self-commits, see step 6).
6. **Decide who commits.**
   - If the caller (handler) commits via `unitOfWork.ExecuteTransactionAsync(...)` — the common case — the Write Service method just delegates to the repo and returns. **It must never call `ExecuteTransactionAsync` itself.**
   - If the caller only ever does a bare `unitOfWork.SaveChangesAsync()` (no explicit transaction needed), the Write Service method may inject `IUnitOfWork` and call that itself after the repo call.
   - Either way, the handler is what decides the transaction boundary — the Write Service never opens one.
7. **Register in DI**, `{Service}.Persistence/DependencyInjection.cs`:
   ```csharp
   services.AddScopedByInterface(typeof(IRepository<>), typeof({Service}DbContext));  // Scrutor - EF repos only
   services.AddScoped<I{Aggregate}ReadService, {Aggregate}ReadService>();
   services.AddScoped<I{Aggregate}WriteService, {Aggregate}WriteService>();
   ```
   The Scrutor scan picks up any `IRepository<T>` implementer (and its specific `I{Aggregate}Repository` marker, via `AsImplementedInterfaces()`) automatically — never manually register a repo that implements the generic interface. Mongo repos (no `IRepository<T>`) are registered manually, same line pattern as the two above.
8. **Update every consumer**, not just `*Handler.cs` files — grep the old repository interface's usages solution-wide before assuming you found them all. gRPC service implementations, background jobs, cache decorators, and other `*.Infrastructure` classes have all been real (easy-to-miss) consumers in past migrations.

## Checklist

- [ ] `{Aggregate}Repo` (implementation, abbreviated) / `I{Aggregate}Repository` (interface, full word) — matches the asymmetric naming convention
- [ ] `I{Aggregate}Repository` is either an empty marker, or has only bulk/by-foreign-key methods — not a dumping ground for query methods that belong on the Read Service
- [ ] `I{Aggregate}ReadService`/`I{Aggregate}WriteService` live in `{Service}.Application/Abstractions/Persistence/{Aggregates}/` — Application has zero references to any repository interface
- [ ] Read Service injects the DbContext/MongoContext directly, never the repo
- [ ] Write Service's public methods are intent-named — no `Func<TEntity, Task>` parameter crosses the Application/Persistence boundary
- [ ] Write Service never calls `unitOfWork.ExecuteTransactionAsync(...)` — that's the caller's job
- [ ] No `SaveChangesAsync`/`ExecuteTransactionAsync` call inside the repository itself
- [ ] Not manually added to any `AddScoped<IRepository<...>, ...>()` call for an EF repo — verify the Scrutor scan is picking it up instead
- [ ] Every consumer of the old repository interface (handlers *and* non-MediatR classes) updated to the new Read/Write services
