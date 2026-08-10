# Implementation Templates

**Scope:** copy-paste starting points matching the conventions in [04-coding-rules.md](04-coding-rules.md). Replace `{Service}`, `{Feature}`, `{Entity}`, `{Verb}` placeholders. These mirror Auth Service's actual code — when in doubt, open the cited real file instead of guessing from the template.

## Command + Handler + Validator

`{Service}.Application/Features/{Feature}/Commands/{Verb}/{Verb}Command.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Commands.{Verb};

public record {Verb}Command(/* inputs */) : ICommand<{Verb}Result>;

public record {Verb}Result(/* outputs */);
```

`{Verb}Handler.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Commands.{Verb};

public sealed class {Verb}Handler(
    I{Entity}WriteService {entity}WriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<{Verb}Command, {Verb}Result>
{
    public async Task<{Verb}Result> Handle({Verb}Command request, CancellationToken ct = default)
    {
        // 1. Load/validate against current state (throw Application/Domain exceptions on failure)
        // 2. Wrap the mutation (+ any outbox enqueue) in unitOfWork.ExecuteTransactionAsync -
        //    the Write Service method itself never opens a transaction, see
        //    conventions/persistence-coding-conventions.md
        // 3. Return result
        throw new NotImplementedException();
    }
}
```

`{Verb}Validator.cs` (only if the command has input worth validating)
```csharp
namespace {Service}.Application.Features.{Feature}.Commands.{Verb};

public sealed class {Verb}Validator : AbstractValidator<{Verb}Command>
{
    public {Verb}Validator()
    {
        RuleFor(x => x.SomeField).NotEmpty().WithMessage("SomeField is required");
    }
}
```
Reference: `Auth.Application/Features/Auth/Commands/Register/{RegisterCommand,RegisterHandler,RegisterValidator}.cs`.

## Query + Handler

`{Service}.Application/Features/{Feature}/Queries/{Verb}/{Verb}Query.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Queries.{Verb};

public record {Verb}Query(/* inputs, e.g. Guid Id */) : IQuery<{Verb}Result>;

public record {Verb}Result(/* outputs */);
```

`{Verb}Handler.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Queries.{Verb};

public sealed class {Verb}Handler(I{Entity}ReadService {entity}ReadService) : IQueryHandler<{Verb}Query, {Verb}Result>
{
    public async Task<{Verb}Result> Handle({Verb}Query request, CancellationToken ct = default)
    {
        var entity = await {entity}ReadService.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("{Entity}", request.Id);

        return new {Verb}Result(/* map fields by hand — Mapster is registered but unused, see coding-rules */);
    }
}
```

## Carter Endpoint

`{Service}.API/Endpoints/{Verb}{Entity}.cs`
```csharp
namespace {Service}.API.Endpoints;

public sealed class {Verb}{Entity} : ICarterModule
{
    private readonly string[] API_DESC = [
        "## {Verb} {Entity}",
        "",
        "Describe what this does.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{route}", async (
            [FromBody] {Verb}Request request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new {Verb}Command(request.Field1, request.Field2);
            var result = await sender.Send(command, ct);
            return ApiResponse<{Verb}Result>.Ok(result);
        })
        .WithSummary("{Service}_{Verb}{Entity}")
        .WithDisplayName("{Verb} {Entity} API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<{Verb}Result>>();
        // .AllowAnonymous() only if this endpoint must bypass auth
    }
}

public record {Verb}Request(/* HTTP-facing shape, may differ from the command */);
```
Reference: `Auth.API/Endpoints/Register.cs`, `User.API/Endpoints/CreateUser.cs`. Full endpoint-adding checklist: [workflows/add-new-api.md](workflows/add-new-api.md).

## Repository + Read/Write persistence service

Full rationale and rules: [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md). Application never references the repository interface directly — only `I{Entity}ReadService`/`I{Entity}WriteService`.

`{Service}.Application/Abstractions/Persistence/{Entities}/I{Entity}ReadService.cs`
```csharp
namespace {Service}.Application.Abstractions.Persistence.{Entities};

public interface I{Entity}ReadService
{
    Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<{Entity}?> GetBySomeFieldAsync(string value, CancellationToken ct = default);
}
```

`{Service}.Application/Abstractions/Persistence/{Entities}/I{Entity}WriteService.cs`
```csharp
namespace {Service}.Application.Abstractions.Persistence.{Entities};

public interface I{Entity}WriteService
{
    Task CreateAsync({Entity} entity, CancellationToken ct = default);
    Task UpdateSomeFieldAsync(Guid id, string value, CancellationToken ct = default);   // intent-named, not a Func<T, Task> delegate
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

`{Service}.Persistence/{Entities}/Repositories/I{Entity}Repository.cs` (empty marker unless you need a bulk-by-foreign-key method — see the workflow doc)
```csharp
namespace {Service}.Persistence.{Entities}.Repositories;

public interface I{Entity}Repository
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
```

`{Service}.Persistence/{Entities}/Repositories/{Entity}Repo.cs`
```csharp
namespace {Service}.Persistence.{Entities}.Repositories;

public sealed class {Entity}Repo({Service}DbContext dbContext) : I{Entity}Repository, IRepository<{Entity}>
{
    public async Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.{Entities}.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    // GetByIdAsync(includes) / AddAsync / AddRangeAsync / UpdateAsync ±includes / DeleteAsync<TId> / DeleteRangeAsync<TId>
    // — see BuildingBlock.Persistence.Repository.IRepository<T>
}
```

`{Service}.Persistence/{Entities}/Read/{Entity}ReadService.cs` — injects `{Service}DbContext` directly, independent of the repo
```csharp
namespace {Service}.Persistence.{Entities}.Read;

public sealed class {Entity}ReadService({Service}DbContext dbContext) : I{Entity}ReadService
{
    public async Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.{Entities}.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<{Entity}?> GetBySomeFieldAsync(string value, CancellationToken ct = default)
        => await dbContext.{Entities}.AsNoTracking().FirstOrDefaultAsync(x => x.SomeField == value, ct);
}
```

`{Service}.Persistence/{Entities}/Write/{Entity}WriteService.cs` — never calls `unitOfWork.ExecuteTransactionAsync` itself
```csharp
namespace {Service}.Persistence.{Entities}.Write;

public sealed class {Entity}WriteService(IRepository<{Entity}> repo) : I{Entity}WriteService
{
    public async Task CreateAsync({Entity} entity, CancellationToken ct = default)
        => await repo.AddAsync(entity, ct);

    public async Task UpdateSomeFieldAsync(Guid id, string value, CancellationToken ct = default)
        => await repo.UpdateAsync(id, async entity =>
        {
            entity.UpdateSomeField(value);
            await Task.CompletedTask;
        }, ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteAsync(id, ct);
}
```

No manual DI registration needed for the repo — `AddScopedByInterface(typeof(IRepository<>), typeof({Service}DbContext))` in `{Service}.Persistence/DependencyInjection.cs` picks it up by Scrutor scan; the Read/Write services are registered explicitly (`services.AddScoped<I{Entity}ReadService, {Entity}ReadService>()`, same for Write). Full checklist: [workflows/add-new-repository.md](workflows/add-new-repository.md).

## Domain entity

This is the minimal shape — a flat entity with no owned collection. If your aggregate owns a required collection (needs ≥1 child, or a many-to-many relationship to another aggregate root), don't start from this template — see [conventions/domain-coding-conventions.md](conventions/domain-coding-conventions.md) for the collection-owning aggregate shape (`Create(..., IEnumerable<{Child}CreateModel> children)`, `ICollection<T> { get; private set; }` navigation, mapping entities for many-to-many) instead.

`{Service}.Domain/Entities/{Entity}.cs`
```csharp
namespace {Service}.Domain.Entities;

public sealed class {Entity} : BaseEntity<Guid>   // or AggregateRoot<Guid> if it's a transaction/consistency boundary
{
    public string SomeField { get; private set; } = string.Empty;

    private {Entity}() { }   // EF Core

    public static {Entity} Create(string someField)
    {
        if (string.IsNullOrWhiteSpace(someField))
            throw ExceptionFactory.RequiredField(nameof(someField));

        return new {Entity} { Id = Guid.NewGuid(), SomeField = someField };
    }

    // Behavior methods, not public setters, for every state change (see conventions/domain-coding-conventions.md#0).
    // AggregateRoot<TId> does not raise events itself (it's a plain marker base type) — if another part of the
    // system needs to react to a change here, publish that reaction from the Application-layer command handler
    // that calls this method, not from inside the entity. See reference/events.md.
}
```
EF config: `{Service}.Persistence/Config/{Entity}Config.cs` implementing `IEntityTypeConfiguration<{Entity}>`. Full checklist: [workflows/add-new-domain-entity.md](workflows/add-new-domain-entity.md).

## Integration event (publish side)

`BuildingBlock.Contract/Events/{Name}IntegrationEvent.cs`
```csharp
namespace BuildingBlock.Contract.Events;

public sealed class {Name}IntegrationEvent : IIntegrationEvent
{
    public Guid CorrelationId { get; }
    public string EventType => nameof({Name}IntegrationEvent);
    public DateTime PublishedAt { get; }
    // + payload fields

    public {Name}IntegrationEvent(/* payload */)
    {
        CorrelationId = Guid.NewGuid();
        PublishedAt = DateTime.UtcNow;
    }
}
```
Publish from the same command handler that made the change, via the Outbox — not a direct publish:
```csharp
await outboxStore.EnqueueAsync(new {Name}IntegrationEvent(/* payload */), ct);
await unitOfWork.SaveChangesAsync(ct);   // aggregate change + OutboxMessage row commit together
```
Never call `IEventPublisher.PublishAsync` directly from feature code — that's the lower-level primitive the Outbox relay itself is built on, and bypasses the Outbox's atomicity guarantee. See [reference/events.md](reference/events.md).

## Integration event (consume side)

`{Service}.Infrastructure/Messaging/Consumers/{Name}Consumer.cs`
```csharp
namespace {Service}.Infrastructure.Messaging.Consumers;

public sealed class {Name}Consumer(ISender sender) : IIntegrationEventConsumer
{
    public IReadOnlyList<string> Topics => ["{publishing-service}.{eventtypelowercased}"];

    public async Task HandleAsync(string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var evt = JsonSerializer.Deserialize<{Name}IntegrationEvent>(message, JsonSerializerConfiguration.Default)!;
        await sender.Send(new {SomeCommand}(evt./* fields */), ct);
        // Adapter only — no business logic here, translate to a command and dispatch.
    }
}
```
Register: `services.AddScoped<IIntegrationEventConsumer, {Name}Consumer>()` in `{Service}.Infrastructure/DependencyInjection.cs`, **before** `AddKafkaMessaging(...)` (topic discovery is eager). Full checklist: [workflows/add-integration-event.md](workflows/add-integration-event.md).

## Background job

`{Service}.Infrastructure/BackgroundJobs/Jobs/{JobName}/{JobName}Service.cs`
```csharp
namespace {Service}.Infrastructure.BackgroundJobs.Jobs.{JobName};

public sealed class {JobName}Service(/* deps */) : IRecurringJob
{
    public string JobId => "{service}-{jobname}";
    public string CronExpression => "*/5 * * * *";
    public string Queue => JobQueue.DEFAULT;
    public bool IsInit => false;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        // do the work
    }
}
```
Register: `services.AddScopedByInterfaceAndConcrete<IRecurringJob>(typeof(DependencyInjection))` — already wired at the `AddBackgroundJobs()` level in services that use Hangfire (currently only Auth). Full checklist: [workflows/add-background-job.md](workflows/add-background-job.md).
