# Architecture Map

**Scope:** the system-level picture — what services and BuildingBlocks exist, how they depend on each other, and how a request/event flows through them. Read this once for onboarding. For binding rules (what you must/must not do), see [02-architecture-rules.md](02-architecture-rules.md).

## Services

| Service | Projects | Status |
|---|---|---|
| **Auth** | `Auth.Domain`, `Auth.Application`, `Auth.Infrastructure`, `Auth.Persistence`, `Auth.API` | Implemented — reference implementation |
| **User** | `User.Domain`, `User.Application`, `User.Infrastructure`, `User.Persistence`, `User.API` | Implemented |
| **Product** | `Product.Domain`, `Product.Application`, `Product.Infrastructure`, `Product.Persistence`, `Product.API` | Implemented — see [services/product-service.md](services/product-service.md) |
| **Inventory** | `Inventory.Domain`, `Inventory.Application`, `Inventory.Infrastructure`, `Inventory.Persistence`, `Inventory.API` | Implemented — see [services/inventory-service.md](services/inventory-service.md) |
| **Order** | `Order.Domain`, `Order.Application`, `Order.Infrastructure`, `Order.Persistence`, `Order.API` | Implemented — see [services/order-service.md](services/order-service.md) |
| **Payment** | `Payment.Domain`, `Payment.Application`, `Payment.Infrastructure`, `Payment.Persistence`, `Payment.API` | Foundation only — see [services/payment-service.md](services/payment-service.md) |
| **Audit** | `Audit.Domain`, `Audit.Application`, `Audit.Infrastructure`, `Audit.Persistence`, `Audit.API` | Implemented — see [services/audit-service.md](services/audit-service.md). MongoDB-backed, not EF/Postgres |
| **YarpApiGateway** | single project | Implemented — the only service exposed to the host network |

Each service follows the same 5-project Clean Architecture split: `Domain` → `Application` → `Infrastructure` → `Persistence` → `API`. See [02-architecture-rules.md](02-architecture-rules.md) for the dependency direction rule and [services/auth-service.md](services/auth-service.md) for the concrete folder layout.

Payment concerns (gateways, accounts, methods, refunds, billing, ...) are exclusively owned by **Payment** — see [reference/payment-ownership-boundaries.md](reference/payment-ownership-boundaries.md) for the full responsibility matrix and why `Order`/`User` only ever hold a lightweight payment *reference*, never payment data itself.

## BuildingBlocks

Fourteen shared projects under `src/BuildingBlocks/`. Full reference: [03-building-blocks-reference.md](03-building-blocks-reference.md).

```
SharedKernel  (constants, POCOs, extensions — zero dependencies, root of the graph)
  ← Domain          (entities, aggregates, domain exceptions, MessageCode)
    ← Application   (CQRS contracts, ICacheService/IAppLogger/ICurrentUserService, app exceptions)
      ← Infrastructure  (Redis cache impl, event dispatch impl, exception→HTTP mapping, authorization, Scrutor scanning)
        ← Web           (ASP.NET Core host: current-user, global exception handler, JWT bearer, Swagger, CORS, Carter, health checks, refresh-token Redis lookup)
      ← Persistence   (contracts-only: IRepository<T>, IOutboxStore/IInboxStore primitives — zero ORM/DB package references)
        ← Persistence.Ef     (EF Core + Npgsql implementation: DbContext base, EfOutboxStore/EfInboxStore, EfUnitOfWork)
        ← Persistence.Mongo  (MongoDB.Driver implementation: MongoContextBase, MongoOutboxStore/MongoInboxStore — peer of
                               Persistence.Ef, used by Audit Service; neither provider is referenced by the other)
  ← Contract         (proto files, IIntegrationEvent DTOs — the only BuildingBlock meant for cross-service reference)
    ← Grpc            (client/server helpers, interceptors)
  + Contract → Messaging → Messaging.Kafka   (broker-agnostic pub/sub abstraction + KafkaFlow implementation)
  + Application → Saga    (in-process saga orchestrator with compensation; currently unused by any service)

Search  (Elasticsearch client + generic indexer; zero dependencies, peer of SharedKernel — see reference/search.md)
```

Every `Auth.*`/`User.*` project layers on top of the matching BuildingBlock (`Auth.Application` depends on `BuildingBlock.Application`, etc.) plus whichever cross-cutting BuildingBlocks it needs (`Grpc`, `Messaging.Kafka`, `Contract`, `Web`).

## Request flow (typical write, e.g. Register)

```
HTTP POST /api/auth/register
  → Gateway (YARP: JWT integrity check only, no role resolution; routes by path prefix)
    → Auth.API/Endpoints/Register.cs (Carter ICarterModule, binds request, builds RegisterCommand)
      → MediatR pipeline: ValidationBehavior<RegisterCommand,_> (FluentValidation) → RegisterHandler
        → Auth.Persistence/Services/AuthService (ASP.NET Identity UserManager)
          → Internal event: OnUserRegisteredEvent → OnUserRegisteredHandler → gRPC call to User.API (CreateUserProfile)
      → ApiResponse<T> returned, mapped to HTTP by BuildingBlock.Web/ExceptionHandling on failure
```

See [reference/events.md](reference/events.md) for the full two-tier event model and [services/auth-service.md](services/auth-service.md) for the complete traced example.

## Event flow (cross-service, e.g. account deletion)

```
Auth.API → command handler enqueues UserAccountDeletionIntegrationEvent (Contract DTO)
  directly via IOutboxStore.EnqueueAsync, committed in the same SaveChangesAsync as the account change
    → OutboxRelayHostedService relays it to Kafka → topic "useraccountdeletionintegrationevent"
      → User.Infrastructure/Messaging/Consumers/UserAccountDeletionIntegrationEventConsumer (IIntegrationEventConsumer)
        → Inbox dedup, then dispatches DeleteUserProfileCommand via MediatR (adapter only, no business logic in the consumer)
```

The Outbox/Inbox pattern is fully implemented and used by every service that publishes or consumes integration events — see [reference/inbox-outbox-runtime.md](reference/inbox-outbox-runtime.md) for the full runtime detail (retry, backoff, dead-letter) and [decisions/event-messaging-refactor.md](decisions/event-messaging-refactor.md) for how this replaced an earlier, incomplete mechanism.

## Networking

Only `yarp-api-gateway` is published to the host (port 5000). Every service listens on two internal ports: `8080` (REST) and `5002` (gRPC), reachable only inside the Docker network. Full detail: `docs/_archive/architecture/NETWORK.md` (content still accurate, kept as reference — not yet promoted into the new structure).

## Infrastructure dependencies (shared, one Redis/Postgres/Kafka/Seq per environment)

- **Postgres** — one shared container (`pg`) today; each service has its own `ConnectionStrings:DefaultConnection`. See [setup/database-split.md](setup/database-split.md) for splitting later.
- **Redis** — one shared container (`redis`), keys separated by service-specific prefixes/naming convention (see [reference/caching.md](reference/caching.md)). The Gateway also connects directly to Redis (bypassing `ICacheService`) for the refresh-token existence check.
- **Kafka** — one shared container (`kafka`), topics named `{eventtype}` (lowercased event type name only, no service prefix — see `KafkaFlowEventPublisher.GenerateTopicName`).
- **Seq** — shared structured-logging sink for all services.
- **Elasticsearch** — one shared container (`elasticsearch`), indexes named per-service (`product-search`, and future `customer-search`/`order-search`). A read model only, never a source of truth — see [reference/search.md](reference/search.md). **Kibana** (`kibana`) is also provisioned against the same container for dashboards.
