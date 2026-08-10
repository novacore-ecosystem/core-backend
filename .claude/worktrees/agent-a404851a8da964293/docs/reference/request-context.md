# Request Context

**Scope:** the ambient, read-only snapshot of the current request's identity - who's calling, which
Tenant they belong to, every Scope they can act under, and the request's
correlation/idempotency/locale metadata. Lives in `BuildingBlock.SharedKernel` (the type itself -
zero dependencies, so every layer of the platform can see it), initialized and torn down
exclusively by `BuildingBlock.Web`'s `RequestContextMiddleware`.

(Named `RequestContext`, not `ExecutionContext` - the latter collides in spirit and in name with
`System.Threading.ExecutionContext`, a BCL type with a completely different job. This type is
about request *identity*, not thread/async flow mechanics.)

## Why it exists

Framework components that run underneath a request - EF Core interceptors and model-building
conventions, Mapster profiles, Outbox/Inbox, Audit - need to know the current user/tenant/scope,
but none of them can (or should) take a DI dependency on `HttpContext`. Before this existed, that
need leaked into places it didn't belong: `DbContextBase` resolved `ICurrentTenantService` via
`this.GetService<T>()` inside `OnModelCreating`, meaning a persistence component had an opinion
about *where* request identity came from. `RequestContext` replaces that with a single ambient
accessor any component can read without DI at all:

```csharp
NovaCore.BuildingBlock.SharedKernel.Context.RequestContext.Current.TenantId
```

## Why it is not a DbContext

`RequestContext` has nothing to do with EF Core. It doesn't track entities, doesn't open
connections, doesn't participate in `SaveChanges`. It is a plain, framework-agnostic snapshot of
identity - the persistence layer is just one of several consumers (see Future usage below).

## Why it is not a DI service, service locator, or general-purpose state container

It isn't registered in DI, isn't resolved with `GetService<T>()`/constructor injection, and has no
interface. A DI-resolved "current user service" implies the value can vary by registration (swap
the implementation, wrap it, mock it) - `RequestContext.Current` is deliberately none of that.
Every consumer, anywhere in the process, sees the exact same value for the current logical request,
because the storage underneath (`AsyncLocal<T>`) flows with the async call chain automatically. It
is also not a place to stash arbitrary request-scoped data - its shape is fixed to request
*identity* fields only; a component that needs to pass its own data through a request reaches for
`HttpContext.Items` or its own scoped service, not this type.

That said, application code that only needs *some* of what's on `RequestContext` and prefers an
injectable, mockable abstraction can still keep using `ICurrentUserService` (`BuildingBlock.Web`) -
the two aren't in conflict, they answer different questions for different consumers.

## Data

```csharp
public sealed class RequestContextData
{
    public Guid? UserId { get; init; }
    public Guid? TenantId { get; init; }
    public IReadOnlyCollection<Guid> ScopeIds { get; init; } = [];
    public required string CorrelationId { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? Locale { get; init; }
}
```

### Why `ScopeIds` is a collection, not a single `ScopeId`

A user can legitimately act across more than one Scope at once (e.g. a regional manager who can
see every Branch under their Region). Earlier this held a single `ScopeId`, which could only ever
express "acting as exactly one Scope" - wrong for that case. The JWT now carries every Scope the
user is allowed to see, **already expanded to include descendants** at token-issuance time:

```json
{ "scope_ids": ["branch-a-guid", "branch-b-guid", "region-x-guid"] }
```

Business services never compute the Scope hierarchy themselves - no recursive tree traversal, no
call back to Auth Service to expand "give me all children of Region X." They just consume the
already-flat list from `RequestContext.Current.ScopeIds`. This keeps every business service
Scope-hierarchy-agnostic: Auth Service is the only place that understands parent/child Scope
structure, at token-issuance time.

## Initialization and lifecycle

```text
Request
  │
  ▼
RequestContextMiddleware reads JWT claims + headers
  │
  ▼
RequestContext.Initialize(data)      ← exactly once, before the rest of the pipeline runs
  │
  ▼
Execute Pipeline (handlers, EF SaveChanges, ...)
  │
  ▼
RequestContext.Clear()               ← always, in a `finally` block
```

`RequestContextMiddleware` (`BuildingBlock.Web/Middleware/RequestContextMiddleware.cs`) is the
**only** component allowed to read request identity off `HttpContext` - JWT claims, the
`X-Correlation-Id`/`Idempotency-Key`/`Accept-Language` headers. It runs once per request, after
`UseAuthentication`/`UseAuthorization` (so `HttpContext.User`'s claims are populated) and before
every other custom middleware, and calls the framework-only `RequestContext.Initialize(...)`.
Every other component - including this same request's own handlers - only ever reads
`RequestContext.Current`. Centralizing the HttpContext read in one place means:

- **It parses HttpContext exactly once.** No other component re-parses claims/headers, so there's
  one place that defines what "the current tenant" or "the current correlation id" means.
- **Anonymous requests just work.** Login, Refresh Token, health checks, and public endpoints all
  flow through the same middleware; `UserId`/`TenantId` are simply `null` and `ScopeIds` is empty
  when the request carries no authenticated identity. The middleware never rejects a request for
  missing identity - that's an authorization concern, handled elsewhere in the pipeline.
- **CorrelationId is always present.** Generated with `Guid.NewGuid()` when the
  `X-Correlation-Id` header is absent, so every consumer can rely on it being non-empty.
- **IdempotencyKey stays optional.** `null` when the `Idempotency-Key` header is absent - callers
  decide what "missing" means for their own endpoint (see `BuildingBlock.Infrastructure`'s
  `IdempotencyMiddleware`, a separate, unrelated mechanism - see the note under Future usage).

The middleware wraps the rest of the pipeline in a `try`/`finally` and calls
`RequestContext.Clear()` unconditionally once it finishes - including on an unhandled exception.
`AsyncLocal<T>` already scopes per logical async call chain, so a later, unrelated request would
never actually observe an earlier request's data even without this - but the framework does not
rely on that alone. Explicit `Initialize` → use → `Clear` is defense-in-depth, not a workaround for
a known leak.

## Request Context is read-only

`RequestContextData` only has `init` setters, and `RequestContext.Initialize(...)`/`Clear()` are
documented (not enforced by the compiler - there's no `InternalsVisibleTo` gate) as
middleware-only. No handler, application service, or background job may call either or otherwise
mutate the current request's identity mid-request. If a value genuinely needs to change (e.g. a
saga acting on behalf of a different tenant), that is a new, explicit unit of work - never a
mutation of the ambient one.

## Why DbContext contains no request-related dependency injection

`DbContextBase` (`BuildingBlock.Persistence.Ef/DbContext/DbContextBase.cs`) - and `AuthDbContext`,
which can't inherit it - do not inject `ICurrentTenantService`, do not touch `HttpContext`, and
never call `this.GetService<T>()` for anything request-scoped. A DbContext's job is ORM
configuration; it has no business knowing *where* `TenantId`/`ScopeId`/`UserId` come from.
Everything that used to require that knowledge (the Entity Convention's query filters,
`TenantAssignmentInterceptor`'s automatic assignment) now reads `RequestContext.Current` directly -
a static, zero-dependency read, not a DI resolution. See `docs/reference/tenant-convention.md` for
how the Entity Convention consumes it, including why the Scope filter is a `Contains` (`IN`) check
against `ScopeIds` rather than an equality check.

## Future usage

`RequestContext.Current` is meant to be the one place every framework component reads identity
from going forward:

- EF interceptors and the Entity Convention (Tenant/Scope/SoftDelete) - already wired, see
  `docs/reference/tenant-convention.md`.
- Audit, Outbox, Inbox - not wired yet; they still use their own metadata providers
  (`IAuditMetadataProvider`, etc.), which remain valid DI-resolved abstractions for now. Wiring
  them onto `RequestContext` instead is later work, not part of this refactor.
- Mapster configuration - not wired yet.
- `BuildingBlock.Infrastructure`'s `IdempotencyMiddleware`/`IIdempotencyStore` (the request-level
  Idempotency-Key dedupe cache) is a separate, older mechanism and is unaffected by this work - it
  keeps reading the header itself for now. `RequestContext.Current.IdempotencyKey` exists so a
  future pass can consolidate onto it instead of a second independent header read.
