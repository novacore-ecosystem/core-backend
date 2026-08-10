# Pattern Library

**Scope:** the philosophy layer — *why* NovaCore shapes a construct the way it does. Distinct from `template-library.md` (the literal file shape) and from `docs/conventions/*.md` (the full rule text this library indexes, not restates). Load a pattern entry before writing any code of that kind; load the Template Library entry it points to for the literal starting shape.

Each entry's Reading Contract format follows `reading-contracts.md`. "Real example" cites the same reference files `docs/06-implementation-templates.md` already names as ground truth — open the actual file, not just the template prose, when in doubt.

---

### Entity
**Intent:** a mutable, identity-bearing object whose state changes only through behavior methods, never public setters — keeps invariants enforceable at the point of change.
**Reading Contract:** Required: `docs/02-architecture-rules.md`, `docs/conventions/domain-coding-conventions.md` · Optional: `docs/workflows/add-new-domain-entity.md` (if this is a brand-new entity, not an edit to one) · Forbidden: Persistence/API/Infrastructure source
**Template:** `docs/06-implementation-templates.md` — "Domain entity" section
**Real example:** the entity cited alongside that section's EF config note

### Aggregate
**Intent:** a transaction/consistency boundary — `AggregateRoot<TId>` marks the entity other code is allowed to load and mutate directly; owned children are never reached around it.
**Reading Contract:** Required: `docs/02-architecture-rules.md`, `docs/conventions/domain-coding-conventions.md` (collection-owning aggregate shape) · Optional: — · Forbidden: Persistence/API/Infrastructure source
**Template:** `docs/06-implementation-templates.md` — "Domain entity" section is the base shape; for a collection-owning aggregate, `docs/conventions/domain-coding-conventions.md`'s aggregate section overrides it (`Create(..., IEnumerable<{Child}CreateModel> children)`, `ICollection<T>` navigation, mapping entities for many-to-many)
**Real example:** see the aggregate example cited in `docs/conventions/domain-coding-conventions.md`

### Value Object
**Intent:** an immutable, structurally-equal type for validated primitives (money, email, etc.) — validation lives once, in the Value Object, not re-implemented at every call site.
**Reading Contract:** Required: `docs/conventions/domain-coding-conventions.md` (reusable Value Object validation section) · Optional: — · Forbidden: Persistence/API/Infrastructure source
**Template:** gap — see `template-library.md`
**Real example:** cited in `docs/conventions/domain-coding-conventions.md`

### Repository
**Intent:** a thin EF-backed accessor, kept as an empty marker interface unless a bulk/by-foreign-key method is genuinely needed — Application never depends on it directly, only on the Read/Write service interfaces built on top.
**Reading Contract:** Required: `docs/conventions/persistence-coding-conventions.md`, `docs/04-coding-rules.md` (Repository & Read/Write Persistence Services section) · Optional: `docs/workflows/add-new-repository.md` (if new) · Forbidden: Domain business-rule internals beyond public surface, API/UI
**Template:** `docs/06-implementation-templates.md` — "Repository + Read/Write persistence service" section
**Real example:** cited in that section

### Persistence Service (Read/Write split)
**Intent:** separates queries (`I{Entity}ReadService`, `AsNoTracking`, no transaction) from mutations (`I{Entity}WriteService`, intent-named methods, transaction owned by the caller) — this is the actual abstraction Application code depends on, not the repository.
**Reading Contract:** Required: `docs/conventions/persistence-coding-conventions.md`, `docs/04-coding-rules.md` (Transaction Management section) · Optional: `docs/reference/inbox-outbox-runtime.md` (if the write also enqueues Outbox) · Forbidden: Domain business-rule internals beyond public surface, API/UI
**Template:** `docs/06-implementation-templates.md` — "Repository + Read/Write persistence service" section
**Real example:** cited in that section

### CQRS (Command / Query + Handler)
**Intent:** every write is a `Command`, every read a `Query`, each with exactly one `Handler` via MediatR-style dispatch — keeps Application-layer intent explicit and testable in isolation.
**Reading Contract:** Required: `docs/conventions/application-coding-conventions.md` (Handler Philosophy), `docs/04-coding-rules.md` (CQRS shape section) · Optional: — · Forbidden: other Features' internals unless explicitly composing them, other services
**Template:** `docs/06-implementation-templates.md` — "Command + Handler + Validator" and "Query + Handler" sections
**Real example:** `Auth.Application/Features/Auth/Commands/Register/*`

### Endpoint (Carter)
**Intent:** a thin HTTP adapter that maps a request DTO to a Command/Query and dispatches via `ISender` — no business logic in the endpoint itself.
**Reading Contract:** Required: `docs/04-coding-rules.md` (Endpoints section), target service's `docs/services/*.md` · Optional: `docs/workflows/add-new-api.md` (if new) · Forbidden: other services' endpoints, unrelated Features
**Template:** `docs/06-implementation-templates.md` — "Carter Endpoint" section
**Real example:** `Auth.API/Endpoints/Register.cs`, `User.API/Endpoints/CreateUser.cs`

### Mapping
**Intent:** fields are mapped by hand at the Handler/Endpoint boundary — Mapster is registered but deliberately unused, to keep mapping logic visible and diffable rather than convention-magic.
**Reading Contract:** Required: `docs/04-coding-rules.md` (Mapping section) · Optional: — · Forbidden: —
**Template:** gap — see `template-library.md`
**Real example:** any Query Handler in `docs/06-implementation-templates.md`'s "Query + Handler" section shows the hand-mapping shape inline

### Validator
**Intent:** `AbstractValidator<TCommand>` (FluentValidation), added only when a command's input is actually worth validating — not boilerplate for every command regardless of need.
**Reading Contract:** Required: `docs/conventions/application-coding-conventions.md` (validation placement) · Optional: — · Forbidden: —
**Template:** `docs/06-implementation-templates.md` — embedded in the "Command + Handler + Validator" section
**Real example:** `Auth.Application/Features/Auth/Commands/Register/RegisterValidator.cs`

### Background Job
**Intent:** `IRecurringJob` implementations registered with Hangfire, one class per job, cron-scheduled — currently only used by Auth.
**Reading Contract:** Required: `docs/workflows/add-background-job.md` · Optional: `docs/services/auth-service.md` (only for the Hangfire dashboard/queue example) · Forbidden: unrelated services
**Template:** `docs/06-implementation-templates.md` — "Background job" section
**Real example:** cited in that section

### Consumer (integration event, consume side)
**Intent:** a translation-only adapter — deserializes the event and dispatches a Command, never contains business logic itself.
**Reading Contract:** Required: `docs/reference/events.md`, `docs/workflows/add-integration-event.md` · Optional: — · Forbidden: implementing business rules directly in the consumer
**Template:** `docs/06-implementation-templates.md` — "Integration event (consume side)" section
**Real example:** cited in that section

### Saga
**Intent:** orchestration for a genuine multi-step, compensable workflow — deliberately not the default choice; use only when a real compensation requirement exists.
**Reading Contract:** Required: `docs/reference/saga.md` · Optional: `docs/reference/create-order-saga.md` (the one real usage, as a worked example) · Forbidden: introducing a saga for a workflow that doesn't need compensation — see the "whether to use" guidance in `docs/reference/saga.md`
**Template:** gap — see `template-library.md`
**Real example:** the CreateOrder saga, per `docs/reference/create-order-saga.md`

### Integration Event (publish side)
**Intent:** published only via the Outbox (`outboxStore.EnqueueAsync` inside the same transaction as the state change) — never a direct `IEventPublisher.PublishAsync` from feature code, which would break atomicity.
**Reading Contract:** Required: `docs/reference/events.md` · Optional: — · Forbidden: `docs/reference/saga.md`, `docs/reference/grpc.md` (unrelated to plain publish)
**Template:** `docs/06-implementation-templates.md` — "Integration event (publish side)" section
**Real example:** cited in that section

### Caching
**Intent:** decorator-based, opt-in per service/entity via `ICacheService` — caching is layered on top of a Read Service, not baked into it.
**Reading Contract:** Required: `docs/reference/caching.md`, `docs/04-coding-rules.md` (Caching / decorator pattern section) · Optional: — · Forbidden: —
**Template:** gap — see `template-library.md`
**Real example:** cited in `docs/reference/caching.md`

### Outbox
**Intent:** guarantees an aggregate change and its resulting integration event commit atomically in one transaction; a background relay publishes the row afterward.
**Reading Contract:** Required: `docs/reference/events.md`, `docs/reference/inbox-outbox-runtime.md` · Optional: — · Forbidden: —
**Template:** see Integration Event (publish side) above — Outbox is that pattern's mechanism, not a separate file shape
**Real example:** cited in `docs/reference/inbox-outbox-runtime.md`

### Inbox
**Intent:** dedup + retry + dead-letter for consumed integration events, so a redelivered message is a no-op rather than a duplicate side effect.
**Reading Contract:** Required: `docs/reference/inbox-outbox-runtime.md` · Optional: — · Forbidden: —
**Template:** see Consumer above — Inbox is the runtime wrapper around consumption, not a separate file shape
**Real example:** cited in `docs/reference/inbox-outbox-runtime.md`

### Retry
**Intent:** currently no standardized retry policy exists in this repo — do not invent a per-feature retry mechanism; this is a known, tracked gap.
**Reading Contract:** Required: `docs/tasks/2026-07-27/Task8_no-standardized-retry-policy.md` (the tracked gap itself) · Optional: — · Forbidden: introducing a bespoke retry mechanism in feature code before the standardized policy lands
**Template:** gap — no standard exists yet
**Real example:** none yet — this pattern entry exists to stop the AI from inventing five different retry approaches across services before the real one is designed

### Domain Service
**Intent:** for a business operation that doesn't naturally belong to one aggregate's identity (e.g. cross-aggregate calculation), a stateless Domain-layer service — this is a thin, rarely-needed pattern in this codebase's pragmatic-DDD style; most logic belongs on the aggregate itself first.
**Reading Contract:** Required: `docs/conventions/domain-coding-conventions.md`, `docs/decisions/` (check for an ADR on "pragmatic DDD" scope before introducing one) · Optional: — · Forbidden: Persistence/API/Infrastructure source
**Template:** gap — no dedicated example yet in this repo; before implementing one, confirm the logic genuinely can't live on an existing aggregate/entity method first (that's the default per this project's DDD conventions, not a Domain Service)
**Real example:** none cited yet — treat a request for a Domain Service as a signal to double-check whether an aggregate method would do instead, not as an automatic go-ahead

### Specification
**Intent:** N/A — **this project deliberately does not use the Specification pattern.** `docs/conventions/domain-coding-conventions.md` explicitly rules out Spec objects in favor of plain navigation collections and direct query logic in Read Services.
**Reading Contract:** Required: `docs/conventions/domain-coding-conventions.md` (the "no Spec objects" rule) · Optional: — · Forbidden: implementing a Specification/Spec-object type anywhere in this codebase
**Template:** none — not applicable
**Real example:** none — if a task asks for a Specification, express the filtering logic as a method on the relevant `I{Entity}ReadService` (see Persistence Service pattern above) instead, and treat the original ask as a stop condition to flag, not fulfill literally

### Configuration (EF entity mapping)
**Intent:** `IEntityTypeConfiguration<TEntity>` per entity, kept in `{Service}.Persistence/Config/`, separate from the entity itself — mapping concerns never leak into the Domain layer.
**Reading Contract:** Required: `docs/conventions/persistence-coding-conventions.md` · Optional: — · Forbidden: Domain-layer changes to express a mapping concern (mappings belong in Persistence, not on the entity)
**Template:** gap — see `template-library.md`; open a real `*Config.cs` file for the target service before writing a new one
**Real example:** any existing `{Entity}Config.cs` in the target service's `Persistence/Config/` folder

### Search
**Intent:** Elasticsearch-backed search lives inside the owning service's Persistence layer, scoped per aggregate — never a standalone `*.Persistence.Elasticsearch` project.
**Reading Contract:** Required: `docs/reference/search.md` · Optional: — · Forbidden: creating a new `*.Persistence.Elasticsearch`-shaped project; cross-service search index sharing
**Template:** gap — see `template-library.md`
**Real example:** cited in `docs/reference/search.md`
