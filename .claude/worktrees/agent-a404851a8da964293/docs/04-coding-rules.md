# Coding Rules

**Scope:** conventions extracted from the actual codebase (Auth Service = reference). These are observed rules, not invented ones — every example below cites the file it came from. For layering/dependency rules, see [02-architecture-rules.md](02-architecture-rules.md). For ready-to-copy code shapes, see [06-implementation-templates.md](06-implementation-templates.md). For layer-specific style rules — how a class is shaped, not just where the file goes — see [conventions/domain-coding-conventions.md](conventions/domain-coding-conventions.md) (Domain: aggregate creation shape, no Spec objects, many-to-many mapping entities, reusable Value Object validation) and [conventions/application-coding-conventions.md](conventions/application-coding-conventions.md) (Application: full Feature-First folder shape, Handler Philosophy, responsibility-based extraction, Mapster policy). This doc covers naming, CQRS shape, endpoints, DI registration, and everything else not owned by those two.

## Folder structure (per feature)

Full shape, including `Common/`, `DTOs/`, `Mapping/`, `Utilities/` and the responsibility of each folder: [conventions/application-coding-conventions.md#feature-first-structure](conventions/application-coding-conventions.md#feature-first-structure). Minimal shape for reference:

```
{Service}.Application/
  Abstractions/{Auth,Persistence,Security,Services}/     interfaces only — Persistence/ holds I{Aggregate}ReadService/I{Aggregate}WriteService per aggregate, not repository interfaces (those live in Persistence now, see conventions/persistence-coding-conventions.md)
  Features/{FeatureArea}/
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
  DependencyInjection.cs
  GlobalUsings.cs
```

Example: `Auth.Application/Features/Auth/Commands/Register/{RegisterCommand,RegisterHandler,RegisterValidator}.cs`. One class per file, filename == class name (Carter endpoint files are the one exception — see below).

```
{Service}.Infrastructure/
  {Concern}/  e.g. Caching/, Security/{Jwt,RefreshTokens}/, BackgroundJobs/{Jobs/{JobName},Services}/, Messaging/Consumers/, GrpcClients/
  DependencyInjection.cs

{Service}.Persistence/
  Configs/            IEntityTypeConfiguration<T> per entity
  Migrations/
  Seeders/
  Services/            concrete app-facing services backed by EF/Identity
  UnitOfWork/
  {Aggregates}/         one folder per aggregate root (plural, matches the DbSet name)
    Read/{Aggregate}ReadService.cs
    Write/{Aggregate}WriteService.cs
    Repositories/{Aggregate}Repo.cs, I{Aggregate}Repository.cs
  {Service}DbContext.cs
  DependencyInjection.cs

{Service}.API/
  Endpoints/{FeatureVerb}.cs   one Carter module per endpoint (not per feature folder)
  DependencyInjection.cs        AddPresentation, calls AddBuildingBlockWeb
  ApplicationPipeline.cs        UseApplication, calls UseBuildingBlockWeb
  Program.cs
  GlobalUsings.cs
```

## Naming conventions

- Commands/Queries: `{Verb}Command` / `{Verb}Query`, handler `{Verb}Handler`, validator `{Verb}Validator`, result `{Verb}Result` (record).
- Events: `On{Trigger}Event` / `On{Trigger}Handler` (e.g. `OnUserRegisteredEvent`, `OnUserRegisteredHandler`).
- Repository **interfaces**: `I{Entity}Repository` (full word, Persistence-internal). Repository **implementations**: `{Entity}Repo` (abbreviated). This asymmetry is intentional house style — keep it. Read/Write persistence services (the Application-owned ports) use the full word both sides: `I{Entity}ReadService`/`{Entity}ReadService`, `I{Entity}WriteService`/`{Entity}WriteService` — see [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md).
- Namespaces mirror folder paths exactly (`Auth.Application.Features.Auth.Commands.Register`).
- `GlobalUsings.cs` per project centralizes cross-cutting `global using`s (e.g. CQRS abstractions, Carter, MediatR).
- `ct` is the standard `CancellationToken` parameter name, always trailing, **always defaulted** (`CancellationToken ct = default`) — including on handler/interface method signatures, not just leaf calls. This is atypical MediatR style but is the consistent house convention; follow it everywhere, including new interfaces.

## CQRS shape

```csharp
public record RegisterCommand(...) : ICommand<RegisterResult>;
public record RegisterResult(...);

public sealed class {Verb}Handler(I{Entity}WriteService {entity}WriteService, IUnitOfWork uow, ...) : ICommandHandler<{Verb}Command, {Verb}Result>
{
    public async Task<{Verb}Result> Handle({Verb}Command request, CancellationToken ct = default) { ... }
}
```

Primary-constructor DI. `ICommand`/`ICommandHandler`/`IQuery`/`IQueryHandler` come from `BuildingBlock.Application.Abstractions.CQRS` — always use these, never `MediatR.IRequest` directly, so the pipeline behaviors apply uniformly.

## Validation

- FluentValidation, one validator class per command/query, co-located in the same feature folder.
- Auto-registered via `services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)` — **do not manually register a validator**; if it's not picked up, the problem is namespace/assembly, not registration.
- Validation failures surface as `BuildingBlock.Application.Exceptions.ValidationException` via `ValidationBehavior<,>` — never catch/rethrow this yourself.

## Mapping

Mapster is registered (`TypeAdapterConfig.GlobalSettings` + `services.AddScoped<IMapper, ServiceMapper>()`) in every service's `Application/DependencyInjection.cs`. **New features use it** for straightforward mapping; existing hand-mapped handlers (Auth/User, and Product/Inventory/Order's first pass) are grandfathered, not retrofitted. Full policy and the reasoning behind it: [conventions/application-coding-conventions.md#mapping](conventions/application-coding-conventions.md#mapping).

## Repository & Read/Write Persistence Services

Application handlers never inject a repository interface directly — they inject `I{Aggregate}ReadService` (queries) or `I{Aggregate}WriteService` (mutations), both owned by Application and implemented in `{Service}.Persistence`. The repository interface (`I{Aggregate}Repository`) is Persistence-internal, injected only by that aggregate's own Write Service (and occasionally its Read Service, though the Read Service more commonly queries `{Service}DbContext` directly). Full rules, worked examples, and the transaction-ownership contract: [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md). Short version:

- `{Entity}Repo` implements the generic `IRepository<T>` (`BuildingBlock.Persistence.Repository.IRepository<T>`) in full wherever the aggregate has a genuine tracked-load-and-mutate need. Its own `I{Entity}Repository` is an empty marker unless it needs a bulk workflow keyed by something other than the primary key (e.g. delete-by-product-id) — reach for that only once a repository's real usage looks like that, not preemptively.
- Mongo-backed aggregates (Audit, Notification) skip `IRepository<T>` entirely — their repos are thin, hand-written, manually registered in DI.
- `{Entity}ReadService` injects `{Service}DbContext`/`{Service}MongoContext` directly and is independent of the repository — this is where `Include`/projection/pagination/search/exists-checks live.
- `{Entity}WriteService` injects the repo (and `IUnitOfWork` only for the bare-`SaveChangesAsync` self-commit case) and exposes one intent-named method per mutation — never a `Func<TEntity, Task>` parameter crossing into Application.
- **`{Entity}WriteService` never calls `unitOfWork.ExecuteTransactionAsync(...)` itself** — see Transaction Management below; that boundary always belongs to the caller.
- Registration: the Scrutor scan (`AddScopedByInterface(typeof(IRepository<>), typeof({Service}DbContext))`) picks up any EF repo implementing `IRepository<T>` automatically — **do not manually register one that does**. Read/Write services are registered explicitly, one per aggregate.

## Transaction Management

`IUnitOfWork` (`BuildingBlock.Application.Abstractions.Persistence.IUnitOfWork`) exposes exactly two members: `SaveChangesAsync(ct)` and `ExecuteTransactionAsync(Func<Task> action, Func<Task>? rollbackAction = null, CancellationToken ct = default)`. There is no `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync`/`WithBeginTransactionAsync` on the interface — if you've seen those names, they're from an outdated version of this doc; `ExecuteTransactionAsync` is the only transaction primitive.

**Do not rely on EF Core's implicit per-`SaveChanges()` transaction as your transaction boundary.** For any workflow with more than one logical write, or that must stay atomic with a non-EF side effect staged on the same `DbContext` (e.g. an Outbox row), wrap the whole thing in `ExecuteTransactionAsync`:

```csharp
public async Task<Result> Handle(SomeCommand request, CancellationToken ct = default)
{
    await unitOfWork.ExecuteTransactionAsync(async () =>
    {
        await entityWriteService.DoSomethingAsync(request.Id, ..., ct);  // never self-commits, see below

        await outboxStore.EnqueueAsync(new SomeIntegrationEvent(...), ct);
    }, ct: ct);

    return new Result(...);
}
```

The Write Service method itself (`entityWriteService.DoSomethingAsync`) only calls `repo.UpdateAsync(...)` and returns — it never calls `SaveChangesAsync` or `ExecuteTransactionAsync` on its own when the caller already owns a transaction. See [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md#write-service-responsibility) for the one exception (a Write Service method may self-commit via a bare `SaveChangesAsync` when its caller never wraps it in an explicit transaction at all).

`ExecuteTransactionAsync` opens the real database transaction (via `Database.BeginTransactionAsync`, wrapped in EF's execution strategy for retry-on-transient-failure), runs `action`, calls `SaveChangesAsync` once more itself after `action` returns, commits, and — on any exception — runs `rollbackAction` (if provided), rolls back, clears the change tracker (so entities touched inside `action` don't stay stuck `Added`/`Modified` after a rollback), and rethrows. **The transaction's lifetime is owned by `ExecuteTransactionAsync`, not by any `SaveChangesAsync` call inside it.**

**Multiple `SaveChangesAsync()` calls inside one `action` are fine** — they're just flushes against the same open transaction, not separate commits — as long as they all happen inside the same `ExecuteTransactionAsync` scope and the workflow commits exactly once, at the end, on success. What's not fine is calling `SaveChangesAsync()` **outside** `ExecuteTransactionAsync`, either instead of it or as a follow-up call after it returns — each bare `SaveChangesAsync()` gets its own implicit, separate transaction, so a crash between two such calls can commit one write and lose the other.

Reference for the correct shape: `StockInHandler`/`StockOutHandler`/`AdjustStockHandler` (`Inventory.Application/Features/Inventories/Commands/`) — the entire aggregate mutation + transaction-log write happens inside one `ExecuteTransactionAsync` call. `Product.Application`'s handlers (`CreateProductHandler`, `UpdateProductHandler`, the variation update/delete handlers) follow the same shape today — the aggregate mutation and the Outbox enqueue are wrapped in one `ExecuteTransactionAsync` the handler owns, per [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md). An earlier version of this doc flagged a double-`SaveChangesAsync`/non-atomic-Outbox gap in these handlers — that was fixed as part of the persistence-service migration, not left as a known issue.

## Exceptions

See the full rule in [02-architecture-rules.md](02-architecture-rules.md#exception-rule) and the catalogue in [reference/exceptions.md](reference/exceptions.md). Short version: Domain exceptions for business rules, Application exceptions for HTTP-aware failures, never raw BCL exceptions from a handler.

## Endpoints (Carter)

```csharp
public sealed class Register : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async ([FromBody] RegisterRequest request, [FromServices] ISender sender, CancellationToken ct = default) =>
        {
            var command = new RegisterCommand(...);   // hand-mapped from request
            var result = await sender.Send(command, ct);
            return ApiResponse<RegisterResult>.Ok(result);
        })
        .AllowAnonymous()                              // or rely on default auth policy
        .WithSummary("Auth_RegisterUser")
        .WithDescription(API_DESC.JoinToString("\n"))   // Markdown description array, see template
        .Produces<ApiResponse<RegisterResult>>();
    }
}
```

The request DTO (`RegisterRequest`) is a `record` declared in the same file, above the module class. One `ICarterModule` per endpoint file, named after the action (`Register.cs`, `Login.cs`, `GetUser.cs`), not per feature. Endpoints never contain business logic — bind, build command/query, `sender.Send`, return.

## DI registration

One public `Add{Layer}` extension per project (`AddApplication`, `AddInfrastructure`, `AddPersistence`, `AddPresentation`), composed of `private static` helper methods, all chained off `services`. Composition order is fixed — see [02-architecture-rules.md](02-architecture-rules.md#composition-root-convention-per-service). Any reflection-based bulk registration (repositories, background jobs) goes through `BuildingBlock.Infrastructure.Extensions.ServiceScanningExtensions` (Scrutor) — never hand-roll a loop over `Assembly.GetTypes()` elsewhere.

## Caching / decorator pattern

To add caching to an existing service (not a repository), wrap it: define the decorator implementing the same interface, resolve the concrete implementation manually in DI, and register the decorator as the interface. Canonical example: `Auth.Infrastructure/Caching/CachedAuthServiceDecorator.cs` + `AddRoleCaching()` in `Auth.Infrastructure/DependencyInjection.cs`. Full pattern: [reference/caching.md](reference/caching.md).

## Background jobs

Implement `IRecurringJob` (`BuildingBlock.Application.Abstractions.Jobs`), register via `AddScopedByInterfaceAndConcrete<IRecurringJob>`. See [workflows/add-background-job.md](workflows/add-background-job.md).

## Formatting

Baseline house style for whitespace/line-breaking, applies to all C# code unless a more specific rule above overrides it for that construct.

**Properties** — no blank lines between consecutive properties/fields in a class, even across attributes or short XML doc comments. Keeps entities scannable when they have many properties.

```csharp
public Guid Id { get; private set; }
/// <summary>Current stock quantity.</summary>
public int Quantity { get; private set; }
public DateTime CreatedAt { get; private set; }
public DateTime UpdatedAt { get; private set; }
```

Prefer a single-line `/// <summary>...</summary>` over a multi-line XML block unless the description genuinely needs more than one line.

**Overloads** — group overloaded methods back-to-back with no blank line between them; this is the first level of logical grouping, ahead of reaching for `#region`. Non-overload methods keep normal blank-line spacing between them. Reserve `#region` for files with enough distinct responsibilities that overload grouping alone doesn't organize them.

```csharp
public Task UpdateAsync(Guid id, Action<TEntity> action) { ... }
public Task UpdateAsync(Guid id, Func<TEntity, Task> action) { ... }
public Task UpdateAsync(
    Guid id,
    Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
    Action<TEntity> action) { ... }
```

**Parameter wrapping** — once a call/declaration needs to wrap, every parameter goes on its own line (no partial wrap to fit width), each indented one level deeper than the declaration. Nested callbacks add one further indent level per level of ownership, so depth is visible at a glance:

```csharp
Execute(
    request,
    options,
    item =>
    {
        Process(
            item,
            context =>
            {
                Save(context);
            });
    })
```

**Method chains** — break the fluent chain before breaking a method's own parameters; only break inside a `Select`/`Where`/etc. once that call's own arguments are the actual readability problem, not just because the chain is long.

```csharp
var result = users
    .Select(user =>
    {
        return new UserDto(
            user.Id,
            user.Name);
    })
    .ToArray();
```

Priority when a line is too long: (1) break the method chain, (2) break that method's parameters, (3) break nested callbacks/expressions — stop as soon as it reads cleanly, don't pre-emptively apply the next level.

**Closing parenthesis** — stays attached to the last argument's line, never on its own line. Applies to method calls, constructor calls, generic calls, and LINQ arguments alike.

```csharp
var result = service.Execute(
    request,
    options,
    cancellationToken);
```

**Ternary expressions** — once a ternary's single-line form runs long (roughly more than 3/4 of the editor's width), split it across three lines: condition, `? trueBranch`, `: falseBranch`, each indented one level under the assignment.

```csharp
return isActive
    ? AccountStatus.Active
    : AccountStatus.Inactive;
```

Short ternaries stay on one line — this is about readability once the expression is genuinely long, not a blanket ban on inline ternaries (`Status = Status == AccountStatus.Locked ? AccountStatus.Active : Status;` is fine as-is).

**Modern C# syntax** — prefer collection expressions over the classic constructor syntax wherever the target type is already known: `List<string> roles = [];` / `public ICollection<T> X { get; private set; } = [];`, not `new List<string>()` / `new()` for a collection type specifically. (Target-typed `new()` for a non-collection class — `public ProductMetadata Metadata { get; private set; } = new();` — is unrelated to this rule and stays as-is.) Reach for other .NET 10/C# 13 syntax the same way whenever it's a strict readability win over the older equivalent — this isn't an exhaustive list, just the pattern to default to when a newer and older spelling both work.

## Async

All I/O-bound methods are `async Task`/`async Task<T>`, `ct` threaded through every call down to the EF Core / Redis / HTTP call. No `.Result`/`.Wait()` in request-handling code paths (the two known exceptions — `SeedDatabase`/`InitializeRefreshTokenCache` in `Auth.API/ApplicationPipeline.cs` calling `.Wait()` — are startup-only, not request-time, and are an accepted exception to this rule, not a pattern to copy elsewhere).
