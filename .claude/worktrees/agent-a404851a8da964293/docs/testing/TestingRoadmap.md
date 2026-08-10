# Testing Roadmap

**Scope:** the 6-phase priority order for building out test coverage, mapped onto NovaCore's actual project list. For "what's done so far," see [TestingProgress.md](TestingProgress.md) — this doc is the stable long-term plan; that one is the living checkpoint. Re-read this before picking the next batch of work in a new session.

Priority always runs simple/deterministic → complex/infrastructure-heavy, per the standing testing initiative. Don't skip ahead to Application/Infrastructure tests while a service's Domain layer is still uncovered unless there's a specific bug-fix reason to.

## Phase 1 — SharedKernel ✅ started

`BuildingBlock.SharedKernel` — `ArrayExtension`, `StringExtension`. Done (see Progress doc). Nothing else in SharedKernel has logic to test (`Constants/*`, `JwtSettings`, `JsonSerializerConfiguration` are data-only).

## Phase 2 — BuildingBlocks

| Project | Status | Notes |
|---|---|---|
| `BuildingBlock.Domain` | ✅ started | `ValueObject`/`StringValueObject` equality, `ExceptionFactory`, `BaseEntity`/`AggregateRoot`, `MessageCodeExtension` |
| `BuildingBlock.Application` | Not started | `ValidationBehavior<,>` pipeline behavior (mock `IValidator<T>`), `PaginatedResult`, `ApiResponse` — pure logic only, no DI container needed |
| `BuildingBlock.Persistence.Ef` | ✅ pre-existing | `AuditGraphBuilder`, `AuditInterceptor` (EF InMemory) |
| `BuildingBlock.Infrastructure` | Deferred | Mostly infra glue (Redis, Hangfire, JWT) — candidate for Phase 5 integration tests, not unit tests |
| `BuildingBlock.Grpc`, `BuildingBlock.Saga`, `BuildingBlock.Messaging*`, `BuildingBlock.Search`, `BuildingBlock.Persistence.Mongo`, `BuildingBlock.Web`, `BuildingBlock.Contract` | Not started | Mostly infrastructure adapters or plain contracts — low priority until a service that uses them reaches Phase 5 |

## Phase 3 — Domain (per service)

Priority order by business-rule density, richest first:

1. **Product.Domain** — ✅ Value Objects done (`Sku`, `ProductCode`, `Barcode`, `CategoryCode`, `Slug`, `TagCode`, `Dimensions`); `Product`/`Variant` entity invariants (variation collection rules, default-variation logic) still pending — current priority, see [TestingProgress.md](TestingProgress.md)
2. **Notification.Domain** — richest entity set (9 entities, 6 Value Objects) — `AudienceSelector`, `ChannelConfiguration`, `NotificationSchedule`, `TemplateContent`, etc.
3. **Inventory.Domain** — stock movement invariants
4. **Auth.Domain** — smallest (5 files), but security-sensitive
5. **User.Domain** — smallest (3 files)
6. **Order.Domain** — no Value Objects, simplest entity shape (`Order`, `OrderItem`, `OrderStatus`)
7. **Audit.Domain** — smallest overall, Mongo-backed, lowest business-rule density

## Phase 4 — Application (per service)

Same service order as Phase 3, once that service's Domain layer is covered. Target one test class per Handler (`{Verb}Handler` → `{Verb}HandlerTests`), covering the success path plus every validation/business-exception branch. Mock only `IRepository<T>`/specific repository interfaces, `IUnitOfWork`, and any `Abstractions/Services` interface the handler depends on — never the Value Objects/Entities it constructs.

## Phase 5 — Infrastructure / Integration

Introduce Testcontainers-backed `{Service}.IntegrationTests` projects once a service's Domain+Application layers are solid. Candidates, in order of how much business value the integration itself carries:

1. Outbox/Inbox relay (`BuildingBlock.Persistence.Ef` hosted services) against real Postgres
2. Repository implementations against real Postgres (replace/extend the current EF-InMemory-only coverage)
3. Product search indexing against real Elasticsearch
4. Redis-backed `ICacheService`/role-caching decorator
5. Kafka producer/consumer roundtrip via `BuildingBlock.Messaging.Kafka`
6. Audit service against real MongoDB

## Phase 6 — API

Carter endpoint smoke tests (`WebApplicationFactory`-style) — only after Phase 3-5 are mature for the services being tested. Lowest priority; most of an endpoint's logic already lives in, and is tested by, its handler.

## Explicitly out of scope for now

- `YarpApiGateway` — pure routing configuration, no business logic
- `BuildingBlock.Saga` — documented as currently unused by any service (`docs/reference/saga.md`)
- Any `Migrations/` folder — generated code
