# Architecture Rules

**Scope:** binding rules extracted from the actual codebase (Auth + User services, all BuildingBlocks). These are not aspirational — every rule below is currently followed by the reference implementation. If you're about to write code, read [04-coding-rules.md](04-coding-rules.md) too; that doc covers naming/shape conventions, this one covers layering and dependency direction.

## Layer responsibilities

| Layer | Owns | Must NOT contain |
|---|---|---|
| **Domain** | Entities, aggregates, value objects, domain exceptions, enums | MediatR, EF Core, ASP.NET Core, HTTP concerns, any `BuildingBlock.Infrastructure`/`Web` reference |
| **Application** | Commands/Queries/Handlers, validators, application events, Read/Write persistence-service *interfaces* (`I{Aggregate}ReadService`/`I{Aggregate}WriteService`), DTOs | EF Core `DbContext`, ASP.NET Core types (except the `Microsoft.AspNetCore.Http.Abstractions` package used only for header-forwarding style contracts — not controllers/endpoints), repository *interfaces* (these live in Persistence now — see [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md)) |
| **Infrastructure** | UnitOfWork *implementations*, external service clients (gRPC, Redis, Kafka), background jobs, caching decorators | Endpoint/route definitions, Swagger, CORS, Carter |
| **Persistence** | `DbContext`, EF configurations, migrations, repository interfaces+implementations, and the `{Aggregate}ReadService`/`{Aggregate}WriteService` implementations of Application's Read/Write ports — see [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md) | Business logic beyond simple query composition/load-modify-save workflows |
| **`BuildingBlock.Persistence`** (contracts) | Repository/UnitOfWork abstractions, `IOutboxStore`/`IInboxStore`, primitive read-model DTOs (e.g. `OutboxMessageSnapshot`) | Any reference to an ORM/database package (EF Core, MongoDB Driver, Dapper, Npgsql, ...) — must stay framework-agnostic |
| **`BuildingBlock.Persistence.Ef`** (EF provider) | The *only* place Outbox/Inbox EF entities (`OutboxMessage`, `InboxMessage`) and their `DbSet<T>` marker interfaces live; generic `EfOutboxStore<TContext>`/`EfInboxStore<TContext>` | Application/Domain business rules, other ORMs. `BuildingBlock.Persistence.Mongo` (used by Audit Service) owns its own `OutboxDocument`/`InboxDocument` independently — Inbox/Outbox entities are never shared across providers |
| **API** | Carter endpoint modules, `Program.cs`, `DependencyInjection.cs` (composition root), `ApplicationPipeline.cs` | Business logic — an endpoint only binds a request, builds a command/query, sends it via `ISender`, and returns the result |

## Dependency direction (must never be violated)

```
SharedKernel ← Domain ← Application ← Infrastructure ← Web ← API (per service, composition root)
```

- **Domain never depends on Application, Infrastructure, or Web.** Domain has no event-raising mechanism at all — `AggregateRoot<TId>` is a plain marker base class (see [reference/events.md](reference/events.md)), so there's no `IDomainEvent`/MediatR-coupling question to guard against in the first place.
- **Application never depends on Infrastructure or Web.** Application owns the Read/Write persistence-service *interfaces* (`I{Aggregate}ReadService`/`I{Aggregate}WriteService`, mirroring how it already owns `IUnitOfWork`); repository interfaces and every implementation live in Persistence — see [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md).
- **A per-service `*.Infrastructure` project may depend on `BuildingBlock.Web`** only for the pieces it legitimately needs to compose at the API layer (this is new since the `BuildingBlock.Web` extraction — see [decisions/buildingblock-web-extraction.md](decisions/buildingblock-web-extraction.md)). Prefer wiring `BuildingBlock.Web` extensions from the API project's `DependencyInjection.cs`/`Program.cs`, not from `*.Infrastructure`.
- **Gateway is intentionally minimal.** It depends only on `BuildingBlock.SharedKernel` + `BuildingBlock.Web` — never `BuildingBlock.Application`/`BuildingBlock.Infrastructure` directly. It performs JWT *integrity* validation only (signature/expiry/format) — no role/permission resolution, no DB/user loading. See [services/gateway.md](services/gateway.md).

## Composition root convention (per service)

Every service wires its layers in `Program.cs` in this exact order:

```csharp
builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);
```

`AddPersistence` must run before `AddInfrastructure` — infrastructure decorators (e.g. `CachedAuthServiceDecorator`) resolve the concrete service Persistence registered, and decorate it. Each layer exposes exactly one public `Add{Layer}` extension method; everything else is a `private static` helper chained off it. See [04-coding-rules.md](04-coding-rules.md#di-registration) for the naming convention and [03-building-blocks-reference.md](03-building-blocks-reference.md) for what each `BuildingBlock.*` DI extension registers.

## The two-tier event system

1. **Internal events** — same-service, in-process, MediatR, published explicitly via `IInternalEventDispatcher` (`IInternalEvent`/`IInternalEventHandler<T>`). Use for same-service orchestration that shouldn't be tangled into the primary handler (e.g. triggering a gRPC call after registration).
2. **Integration events** — cross-service, Kafka, plain DTOs in `BuildingBlock.Contract`, published via the transactional Outbox (`IOutboxStore.EnqueueAsync`, in the same handler and the same `SaveChangesAsync` as the aggregate mutation). Use only when another *service* needs to know something happened.

There is no third, "Domain event" tier — `AggregateRoot<TId>` is a plain marker base class with no event-raising capability; it exists to mark transaction-boundary/aggregate-root entities for tooling (e.g. the audit hierarchy, see [reference/audit-trail.md](reference/audit-trail.md)), not to raise events. A command handler that needs to publish an integration event enqueues it onto the Outbox directly — there is no domain-event hop in between. Do not reach for MediatR notifications as a substitute for integration events, and do not call another service's API directly from a domain entity. Full detail: [reference/events.md](reference/events.md).

## Exception rule

- Domain code throws `BuildingBlock.Domain.Exceptions.*` (via `ExceptionFactory`) for business-rule violations — no HTTP awareness.
- Application code throws `BuildingBlock.Application.Exceptions.*` (`NotFoundException`, `ConflictException`, `UnauthorizedException`, `ForbiddenException`, `BadRequestException`, `ValidationException`) for HTTP-aware failures.
- **Never throw raw BCL exceptions (`InvalidOperationException`, `ArgumentException`, etc.) from a handler.** They fall through `ExceptionHandlerHelper`'s switch as "unexpected" and surface as a masked 500 instead of the correct status code. (User Service currently violates this in two handlers — flagged as a bug, see [services/user-service.md](services/user-service.md#known-issues).)
- One central mapping point: `BuildingBlock.Infrastructure/ExceptionHandling/ExceptionHandlerHelper.cs`. One central HTTP wiring point: `BuildingBlock.Web/ExceptionHandling/GlobalExceptionHandler.cs`. Never write a per-service exception handler.

## What "reference implementation" means

Auth Service is the canonical example. When Auth and User disagree on a convention (see [services/user-service.md](services/user-service.md) divergences), **follow Auth** unless a workflow doc says otherwise. Known, accepted divergences are documented explicitly in each service doc — anything not documented as an accepted divergence is drift and should be fixed to match Auth, not copied.
