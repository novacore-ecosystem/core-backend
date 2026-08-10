# BuildingBlocks Reference

**Scope:** what each `src/BuildingBlocks/BuildingBlock.*` project is for, its key types, and its DI extension methods. This is a lookup table, not a tutorial — for usage patterns and gotchas, follow the links into `reference/`. Dependency graph: [01-architecture-map.md](01-architecture-map.md#buildingblocks).

## SharedKernel
Zero-dependency constants/POCOs/extensions. Root of the dependency graph — transport- and framework-agnostic, referenced by every layer including Infrastructure implementations that must not depend on `BuildingBlock.Web`.
- `Constants/CacheKeyConstant.cs` — centralized Redis key builders
- `Constants/AppRoleConstant.cs` — Root/Admin/User role strings, used for role-to-permission seeding and `IsInRole` checks
- `Constants/Permissions.cs` — the permission key catalog, grouped by business module, each with a `Full` aggregate; see [reference/authorization.md](reference/authorization.md)
- `Constants/AppClaimTypes.cs` — custom claim type key constants only (e.g. `Permission`) — no logic, no extension methods
- `Extensions/ClaimsPrincipalExtension.cs` — reads raw claim values off `ClaimsPrincipal` (e.g. `GetPermissions()`) — no authorization decisions; those live in `BuildingBlock.Web.Authorization`
- `Security/JwtSettings.cs` — shared JWT config POCO (SecretKey/Issuer/Audience/expirations)
- `Serialization/JsonSerializerConfiguration.cs` — `JsonSerializerOptions.Default` used everywhere JSON is (de)serialized
- Must never contain: authorization/permission-evaluation logic, HTTP-specific behavior, or ASP.NET infrastructure — those belong in `BuildingBlock.Web`.
- No DI, no interfaces.

## Domain
DDD tactical types + domain exception hierarchy. No infra/framework dependencies beyond SharedKernel.
- `Abstractions/AggregateRoot.cs` (plain marker base class — no event-raising capability), `BaseEntity.cs`, `IEntity.cs`, `ValueObject.cs`, `StringValueObject.cs`, `IAuditable.cs` (marker used by the audit-graph pipeline, see [reference/audit-trail.md](reference/audit-trail.md))
- `Exceptions/DomainException.cs` (abstract, carries `MessageCode`) + 7 concrete types + `ExceptionFactory` (static factory, see [reference/exceptions.md](reference/exceptions.md))
- `Enums/MessageCode.cs` — ~800-entry centralized error-code enum, ranges: System 001-099, Validation 100-199, Client 200-299, then per-service blocks (Auth/Product/Inventory/Order/User/Payment)
- No DI.

## Application
Framework-agnostic application-layer contracts. Depended on by every service's Application layer.
- **CQRS**: `Abstractions/CQRS/ICommand[.cs]`, `ICommandHandler.cs`, `Iquery.cs` (note lowercase filename), `IQueryHandler.cs` — thin wrappers over MediatR
- **Cross-cutting service contracts** (`Abstractions/Services/`): `ICacheService`, `IAppLogger<T>`, `ICurrentUserService`, `IAppService` (marker)
- **Persistence contracts** (`Abstractions/Persistence/`): `IUnitOfWork`. `IRepository<T>` (the generic repository contract) lives in `BuildingBlock.Persistence`, not here — see [Persistence, Persistence.Ef, Persistence.Mongo](#persistence-persistenceef-persistencemongo) below and [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md) for the full Read/Write persistence-service pattern built on top of it.
- **Events** (`Abstractions/Events/`): `IInternalEvent : INotification`, `IInternalEventHandler<T>`, `IInternalEventDispatcher` — same-service, MediatR-based (see [reference/events.md](reference/events.md#the-two-tiers) for the naming history)
- **Jobs** (`Abstractions/Jobs/`): `IRecurringJob`, `IScheduledJob`, `IJobOptions`, `IScheduledJobScheduler`
- `Behaviors/ValidationBehavior.cs` — MediatR pipeline behavior running FluentValidation
- `Exceptions/ApplicationException.cs` (abstract, has `StatusCode`) + 6 concrete (see [reference/exceptions.md](reference/exceptions.md))
- **DI**: `AddApplicationBehaviors()` registers `ValidationBehavior<,>` as open-generic `IPipelineBehavior<,>`. Does not register MediatR itself — each service does that.

## Infrastructure
Concrete implementations of the Application contracts above, plus DI-scanning and other non-Web infrastructure helpers. Does **not** contain ASP.NET exception handling or authorization infrastructure — those live in `BuildingBlock.Web` (see below and [reference/authorization.md](reference/authorization.md)), since Infrastructure must stay usable by non-Web consumers (e.g. `Auth.Infrastructure`'s JWT token generation).
- `Caching/RedisCacheService.cs` (`ICacheService` impl) + `CacheOptions.cs` (bound from config section `"Cache"`)
- `Events/ApplicationEventDispatcher.cs` — thin `IMediator.Publish` wrapper implementing `IInternalEventDispatcher` (registers as `InternalEventDispatcher`; the file/DI-method name `AddApplicationEventDispatcher()` predates a rename and is legacy naming, not a second mechanism — see [reference/events.md](reference/events.md))
- `Logging/AppLogger.cs` — `IAppLogger<T>` wrapping `ILogger<T>`
- `Extensions/ServiceScanningExtensions.cs` — Scrutor-based marker-interface assembly scanning (`AddScopedByInterface<T>`, `AddSingletonByInterface<T>`, `*AndConcrete` variants) — the mechanism repositories, background jobs, etc. auto-register through
- `BackgroundJobs/HangfireSchedulingExtensions.cs` — shared Hangfire bootstrap (storage/server setup, `RecurringJobRegistry` job discovery, `ScheduledJobScheduler`), reused by every service that runs recurring jobs (currently Auth and User). `BackgroundJobs/Cleanup/` — the Inbox/Outbox cleanup jobs (`OutboxCleanupJob`/`InboxCleanupJob`), opt-in per service via `AddInboxOutboxCleanupJobs(configuration)`, independent of `AddHangfireScheduling`'s own job-assembly markers. See [reference/inbox-outbox-runtime.md](reference/inbox-outbox-runtime.md#cleanup) and [workflows/add-background-job.md](workflows/add-background-job.md).
- **DI**: `AddRedisCache(...)`, `AddApplicationEventDispatcher()` (registers `IInternalEventDispatcher`), `AddAppLogger()`, `AddHangfireScheduling(configuration, jobAssemblyMarkers)`, `AddInboxOutboxCleanupJobs(configuration)`

## Persistence, Persistence.Ef, Persistence.Mongo
Framework-agnostic persistence contracts, split from their provider implementations so multiple providers can plug in without touching either the contracts or each other. Two providers exist today: `Persistence.Ef` (EF Core + Npgsql, used by every service except Audit) and `Persistence.Mongo` (MongoDB.Driver, used by Audit Service) — true peers, neither referenced by the other.
- **`BuildingBlock.Persistence` (contracts only — no ORM/database package reference, ever):**
  - `Repository/IRepository.cs` — generic repository abstraction: `GetByIdAsync` (±`Func<IQueryable<T>, IQueryable<T>> includes` overload), `AddAsync`, `AddRangeAsync`, `UpdateAsync<TId>` (±includes overload, delegate-based tracked-load-and-mutate), `DeleteAsync<TId>`, `DeleteRangeAsync<TId>`. Concrete EF repos implement this in full wherever the aggregate has a genuine tracked-load-and-mutate need; see [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md) for the Read/Write persistence-service layer built on top of it and when a repo may stay a thin/empty marker instead.
  - `Outbox/IOutboxStore.cs`, `Outbox/OutboxMessageSnapshot.cs` — primitive-typed outbox contract + read-model record (no entity type exposed). Includes `DeleteProcessedBeforeAsync(olderThanUtc, batchSize, ct)` for the cleanup job — deletes one batch of already-processed rows, never touches unprocessed ones.
  - `Inbox/IInboxStore.cs` — primitive-typed (messageId, consumerName) dedup contract, plus `DeleteProcessedBeforeAsync(olderThanUtc, batchSize, ct)` for cleanup.
  - `Audit/` — the provider-agnostic half of the audit-graph pipeline: `IAuditHierarchyBuilder`/`AuditHierarchyBuilder` (the `ConfigureAuditHierarchy` fluent API — `IsRoot`/`BelongsTo<TParent>`), `IAuditHierarchyRegistry`/`AuditHierarchyRegistry` (resolved, cached, fail-fast-validated metadata), `AuditTrackedEntity`/`AuditGraphResult`, and `AuditGraphBuilder` (the pure grouping/tree algorithm — exactly one graph per changed Aggregate Root instance, never per entity). `Persistence.Ef`'s `AuditInterceptor` is the only piece that's EF-specific; everything here is reusable by a future Dapper/Mongo provider unchanged. See [reference/audit-trail.md](reference/audit-trail.md) for the full pipeline.
- **`BuildingBlock.Persistence.Ef` (the EF-aware implementation):**
  - `UnitOfWork/EfUnitOfWork.cs` — the EF implementation of `IUnitOfWork`. There is no `RepositoryBase` here (the old empty, zero-inheritor stub was removed as dead code) — every EF `{Aggregate}Repo` implements `BuildingBlock.Persistence.Repository.IRepository<T>` directly, no shared base class
  - `Outbox/OutboxMessage.cs` + `Outbox/IOutboxDbContext.cs` + `Outbox/OutboxConfiguration.cs` + `Outbox/EfOutboxStore<TContext>` — the EF entity, `DbSet<T>` marker, table mapping, and generic store all live together here; `EfOutboxStore` projects the entity to `OutboxMessageSnapshot` before returning, so the entity type never crosses into `BuildingBlock.Persistence`
  - Parallel `Inbox/` trio (`InboxMessage`, `IInboxDbContext`, `InboxConfiguration`, `EfInboxStore<TContext>`)
  - `Interceptors/AuditInterceptor.cs` — implements `ISaveChangesInterceptor`; extracts `AuditTrackedEntity` snapshots from EF's `ChangeTracker` (own identity via EF's primary-key metadata, not reflection-guessing) for entities that are both `IAuditable` and registered via `ConfigureAuditHierarchy`, hands them to `BuildingBlock.Persistence`'s `AuditGraphBuilder`, and enqueues exactly one `AuditIntegrationEvent`-carrying `OutboxMessage` per resulting graph onto the same context's Outbox table. Registered automatically by `AddPersistenceDbContext<TContext>` for every service, via the existing wiring point (`DbContextOptionsBuilderExtensions.UsePersistenceDefaults` resolves every registered `ISaveChangesInterceptor` from DI and adds it via `AddInterceptors(...)`); a default empty `IAuditHierarchyRegistry` and no-op `IAuditMetadataProvider` are also registered there so DI always resolves even for a service that configures neither. See [reference/audit-trail.md](reference/audit-trail.md) for the full pipeline. `ConcurrencyInterceptor.cs` remains a **placeholder, not yet implemented**.
  - `DbContext/DbContextBase.cs` — shared base for every **non-Identity** EF `DbContext` (Auth is the one exception, see below). Seals `OnConfiguring` (calls `DbContextOptionsBuilderExtensions.SuppressPendingModelChangesWarning()`) and `OnModelCreating` (calls `ModelBuilderExtensions.ApplyPersistenceConfigurations(GetType().Assembly)` then `ApplyOutboxInboxConfiguration(this)`, which conditionally applies `OutboxConfiguration`/`InboxConfiguration` based on whether the context implements `IOutboxDbContext`/`IInboxDbContext`), then calls the virtual `ConfigureModel(modelBuilder)` hook for anything a derived context needs beyond assembly scanning + Outbox/Inbox. Inherited by `UserDbContext`, `OrderDbContext`, `InventoryDbContext`, `ProductDbContext` — none of them currently need to override `ConfigureModel`, since `ApplyConfigurationsFromAssembly` already picks up every `IEntityTypeConfiguration<T>` in that service's own assembly.
  - `DbContext/DbContextOptionsBuilderExtensions.cs` — `UsePersistenceDefaults(serviceProvider)` (snake_case naming, detailed errors, DEBUG-only sensitive data logging, registered interceptors) and `SuppressPendingModelChangesWarning()`, both provider-agnostic and reused by `AddPersistenceDbContext` **and** by `AuthDbContext` directly (see below)
  - `DbContext/ModelBuilderExtensions.cs` — `ApplyPersistenceConfigurations(assembly)` (assembly scan + a currently-empty global-conventions hook) and `ApplyOutboxInboxConfiguration(context)`, both reused the same way
  - **DI**: `AddPersistenceDbContext<TContext>(connectionString)` (provider selection/retry policy stays here - Npgsql-specific - and delegates the provider-agnostic part to `UsePersistenceDefaults`), `AddEfOutboxStore<TContext>()`, `AddEfInboxStore<TContext>()` (both require `TContext : DbContext, IOutboxDbContext`/`IInboxDbContext`)

  **Auth Service is intentionally excluded from `DbContextBase`:** `AuthDbContext` must inherit `IdentityDbContext<...>` for ASP.NET Core Identity's own model configuration, and C# doesn't support inheriting from two base classes. Forcing Identity's model building through `DbContextBase`'s hook shape isn't possible without reimplementing what `IdentityDbContext` already does. Instead, `AuthDbContext.OnConfiguring`/`OnModelCreating` call the exact same `DbContextOptionsBuilderExtensions`/`ModelBuilderExtensions` helper methods `DbContextBase` calls internally - so Auth still shares 100% of the *reusable* logic (warning suppression, assembly scan + global conventions, conditional Outbox/Inbox configuration), it just can't share the base *class*. Everything Identity-specific (the `IdentityDbContext<Account, Role, Guid, ...>` type parameters, `UserClaims`/`UserLogins`/`RoleClaims`/`UserTokens` DbSets) remains untouched.
- **`BuildingBlock.Persistence.Mongo` (the MongoDB.Driver-aware implementation, used by [Audit Service](services/audit-service.md)):**
  - `MongoContext/MongoContextBase.cs` — thin wrapper holding the `IMongoDatabase` handle; there's no per-request change tracker to abstract the way `DbContextBase` does for EF
  - `Outbox/OutboxDocument.cs` + `Outbox/IOutboxMongoContext.cs` + `Outbox/MongoOutboxStore<TContext>` — mirrors the EF trio's shape exactly, `[BsonId]` on `Id`, projecting to the same `OutboxMessageSnapshot` record; `DeleteProcessedBeforeAsync` selects matching ids then `DeleteManyAsync`s them (Mongo's `deleteMany` has no native `LIMIT`, so batching is done by id-list instead)
  - Parallel `Inbox/` trio (`InboxDocument`, `IInboxMongoContext`, `MongoInboxStore<TContext>`)
  - `Outbox/OutboxExtensions.EnsureOutboxIndexes()` / `Inbox/InboxExtensions.EnsureInboxIndexes()` — index creation, called once from the consuming service's Mongo context constructor (Mongo has no `OnModelCreating` equivalent to apply configuration declaratively)
  - **DI**: `AddPersistenceMongoContext<TContext>(connectionString, databaseName)` (registers `IMongoClient`/`IMongoDatabase`/`TContext` all as **Singletons** — the Mongo driver's handles are stateless/thread-safe, unlike EF's Scoped `DbContext` — and registers a process-global `CamelCaseElementNameConvention` once, the Mongo equivalent of EF's `UseSnakeCaseNamingConvention()`), `AddMongoOutboxStore<TContext>()`, `AddMongoInboxStore<TContext>()`
- **Service `*.Persistence` adapters** (`Auth.Persistence`, `User.Persistence`, `Order.Persistence`, `Audit.Persistence`, ...) sit one layer up: they implement `BuildingBlock.Application.Abstractions.Outbox.IOutboxStore`/`IInboxStore` by wrapping the primitive `BuildingBlock.Persistence` store, translating typed `IIntegrationEvent`s ⇄ primitive rows on the way in/out (`OutboxStore.cs`, `InboxStore.cs`) — this adapter code is byte-identical whether the underlying store is EF- or Mongo-backed, since it only ever touches the provider-agnostic primitive interface. See [reference/inbox-outbox-runtime.md](reference/inbox-outbox-runtime.md) for the full runtime flow.

## Web
API-layer (ASP.NET Core host) building blocks — everything that only participates in the ASP.NET request pipeline lives here, never in `Infrastructure` or `SharedKernel`. See [decisions/buildingblock-web-extraction.md](decisions/buildingblock-web-extraction.md).
- `CurrentUser/CurrentUserService.cs` — `ICurrentUserService` impl, HttpContext-backed (claims + AccessToken/RefreshToken HttpOnly cookies)
- `ExceptionHandling/` — `ExceptionHandlerHelper.cs` (single exception→`ApiResponse`+status mapping point, see [reference/exceptions.md](reference/exceptions.md)) + `GlobalExceptionHandler.cs` (`IExceptionHandler` impl delegating to it) + `ExceptionHandlingExtensions.cs` (`AddExceptionHandling()`/`UseGlobalExceptionHandling()`) — all self-contained here, since exception→HTTP mapping only participates in the ASP.NET request pipeline
- `Authorization/` — `AppClaimTypes`/`ClaimsPrincipalExtension` come from `SharedKernel`; this folder owns everything ASP.NET-specific built on top: `PermissionAuthorization.HasAnyPermission` (evaluation), `PermissionEndpointExtensions.RequirePermissions(...)` (endpoint declaration), `AuthorizationExtensions.AddBuildingBlockAuthorization()` (DI registration). See [reference/authorization.md](reference/authorization.md).
- `Security/Jwt/JwtBearerAuthenticationExtensions.cs` — binds `"Jwt"` config section to `SharedKernel.Security.JwtSettings`, configures cookie-aware JWT bearer auth
- `Swagger/SwaggerExtensions.cs`, `Cors/CorsExtensions.cs`, `Carter/CarterExtensions.cs`, `HealthChecks/HealthCheckExtensions.cs` — thin, parameterized wrappers (title/description/route-prefix/policy-name driven by `BuildingBlockWebOptions`)
- `RefreshTokens/RefreshTokenCacheExtensions.cs` — **deliberately bypasses `ICacheService`**; raw `IConnectionMultiplexer` + a single `EXISTS refresh_token_by_string:{token}` check, used by the Gateway
- **DI**: each service chains the pieces it needs individually in its own `AddPresentation` (`AddExceptionHandling()`, `AddBuildingBlockAuthorization()`, `AddCurrentUser()`, `AddJwtBearerAuthentication(...)`, `AddSwaggerDocumentation(...)`, `AddCorsPolicy(...)`, `AddCarterModules(...)`, `AddHealthCheckServices()`) — there is no single composed `AddBuildingBlockWeb` entry point today.

## Contract
Cross-service wire contracts — the only BuildingBlock meant to be referenced across service *boundaries*.
- `Events/IIntegrationEvent.cs` + concrete DTOs (`UserCreatedIntegrationEvent`, `UserAccountDeletionIntegrationEvent`) — plain classes, no MediatR coupling
- `Protos/user.proto` — contract-first gRPC (`UserGrpcService.CreateUserProfile`), generates both client+server stubs (`GrpcServices="Both"`)

## Grpc
Reusable gRPC client/server plumbing on top of `Contract`'s generated stubs.
- `Client/GrpcClientExtensions.cs` — `AddGrpcClient<TClient>(...)`, `AddGrpcClientMesh(...)`; 10MB max message + gzip by default
- `Server/GrpcServerExtensions.cs` — `AddGrpcServer()`, `MapGrpcServices(app)`; wires health checks
- `Interceptors/ErrorHandlingInterceptor.cs` (maps BCL exceptions → gRPC status codes), `LoggingInterceptor.cs`

## Messaging + Messaging.Kafka
Broker-agnostic pub/sub abstraction (`Messaging`) + KafkaFlow implementation (`Messaging.Kafka`). Outbound publishing goes through the Outbox (see [Persistence + Persistence.Ef](#persistence--persistenceef)); this layer is the transport underneath it plus inbound Inbox dedup.
- `Messaging/Abstractions/IEventPublisher.cs`, `IOutboxPublisher.cs`, `IEventDispatcher.cs`, `IIntegrationEventConsumer.cs` (topics + handler), `IIntegrationEventHandler<T>.cs`
- `Messaging/Services/IntegrationEventConsumerRegistry.cs` — fans out a received message to all matching consumers, applying Inbox dedup generically before each one runs, isolating per-consumer failures
- `Messaging.Kafka/Publishers/KafkaFlowEventPublisher.cs` — topic = `"{serviceName}.{eventType}"` lowercased, keys by `CorrelationId`; `KafkaOutboxPublisher.cs` — used by `OutboxRelayHostedService`, adds the `message-id` header Inbox dedup keys off of
- `Messaging.Kafka/Consumers/IntegrationEventDispatchHandler.cs` — KafkaFlow bridge → `IntegrationEventConsumerRegistry`
- **DI**: `AddKafkaMessaging(configuration, serviceName)` — binds `"Kafka"` section, **requires consumers already registered** (it eagerly builds a temporary provider to discover `Topics` before configuring KafkaFlow); `AddInboxOutboxInfrastructure(configuration)` (in `BuildingBlock.Infrastructure`) registers `OutboxRelayHostedService` + the Inbox dedup delegates — see [reference/inbox-outbox-runtime.md](reference/inbox-outbox-runtime.md)

## Saga
In-process saga orchestrator with compensation. **Currently unused by any service** — see [reference/saga.md](reference/saga.md) before adopting it.
- `Abstractions/ISagaStep.cs` (Execute + Compensate), `ISagaDefinition.cs` (built via `SagaDefinitionBuilder`), `ISagaContext.cs`, `ISagaOrchestrator.cs`, `ISagaStore.cs` (+ `InMemorySagaStore`, dev-only)
- **DI**: `AddSagaOrchestration()` / `AddSagaOrchestration<TStore>()` / `AddSagaOrchestration(factory)`

## Search
Elasticsearch client + generic indexer — the reusable 20% of the Search read-model pattern introduced for Product Search. Zero project references (root-level, peer of `SharedKernel`), one package (`Elastic.Clients.Elasticsearch`). Full architecture, including what stays Product-specific: [reference/search.md](reference/search.md).
- `Configuration/ElasticsearchOptions.cs` — `Url`/`MaxRetries`/`RequestTimeoutSeconds`, bound from the `"Elasticsearch"` config section.
- `Abstractions/IElasticsearchIndexer.cs` — generic `IElasticsearchIndexer<TDocument>`: `EnsureIndexAsync`/`RecreateIndexAsync`/`IndexAsync`/`DeleteAsync`/`BulkIndexAsync`. The only reusable component allowed to write to Elasticsearch — no generic query/search abstraction exists, since query DSL is too domain-specific to share.
- `Indexing/ElasticsearchIndexer.cs` — the implementation, wraps a singleton `ElasticsearchClient`.
- **DI**: `AddElasticsearchClient(configuration)` registers the singleton `ElasticsearchClient` (retry/timeout wired from `ElasticsearchOptions`) and the open-generic `IElasticsearchIndexer<>`.
