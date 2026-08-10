# Audit Service

**Scope:** Audit-specific facts and its documented divergences from the [User Service](user-service.md) reference implementation. General patterns live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md) — not repeated here. Unlike every other service, Audit's persistence layer is MongoDB, not EF/Postgres — that difference is the focus of this doc.

## Projects

`Audit.Domain`, `Audit.Application`, `Audit.Infrastructure`, `Audit.Persistence`, `Audit.API` — same 5-layer split as User, but `Audit.Persistence` references `BuildingBlock.Persistence.Mongo` instead of `BuildingBlock.Persistence.Ef`.

## Entities

- **AuditLogEntry** (`Audit.Domain/Entities/AuditLogEntry.cs`) — one recorded Aggregate Root audit graph. `RootEntityType`/`RootEntityId`/`Service`/`CorrelationId` stay flat, top-level, indexed fields (see `scripts/mongodb/init-mongo.js`) even though `RootEntityType`/`RootEntityId` are also duplicated inside `Root`, so a query never has to descend into the nested document just to filter by aggregate identity or originating service. `Root` (`AuditTrailNode`) is the actual nested audit tree — `NodeId`/`ParentNodeId`/`Depth`/`EntityType`/`EntityId`/`Action`/`Changes`/`Children`, a real recursive hierarchy, not a flattened JSON blob — persisted as native nested Mongo documents, not a serialized string. `Metadata` (`AuditTrailMetadata`, nullable) carries optional context (actor, client IP, trace id, ...). `Timestamp` is the source event's `PublishedAt`, `ReceivedAt` is when Audit actually persisted it. Plain `BaseEntity<Guid>`, not an aggregate root — immutable, append-only, same shape as Inventory's `InventoryTransaction`. `AuditTrailNode`/`AuditTrailFieldChange`/`AuditTrailMetadata` are Audit.Domain's own types (Mongo-embedded mirrors of `BuildingBlock.Contract.Events.Audit`'s `AuditNode`/`AuditFieldChange`/`AuditMetadata`), kept separate so `Audit.Domain` stays free of a `BuildingBlock.Contract` dependency — mapping happens in `Audit.Application`'s command/query handlers. No naming-collision alias needed (see the naming note below) — this is deliberately not named `Audit`. See [reference/audit-trail.md](../reference/audit-trail.md) for the full producer-side pipeline.

## Purpose: the single sink for the project's one audit event

Audit's whole job is to consume `AuditIntegrationEvent` — the only audit event type in the project, published by every EF-backed service's `AuditInterceptor` — and persist a durable, queryable record of each Aggregate Root's change graph. It doesn't consume business integration events for audit purposes anymore (see "Messaging" below); it publishes nothing of its own consequence today (see "Outbox is wired but unused" below).

## Ports & routing

Internal `8080` (REST) only. Gateway path prefix `/api/audit/` (`RequireAuth: true`).

## Routes (Carter endpoints, `Audit.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| GET | `/audit-logs` | `ListAuditLogs.cs` | Paginated, filterable (`service`, `from`, `to`) list of audit entries (RequireAdmin) |
| GET | `/audit-logs/{auditLogId}` | `GetAuditLog.cs` | Fetch a single audit entry with its raw payload (RequireAdmin) |

Both endpoints are admin-only — audit data (raw event payloads) is sensitive.

## Documented divergence from User: no gRPC surface

Same as [Product Service](product-service.md#documented-divergence-from-user-no-grpc-surface) — no gRPC listener or `GrpcServices/` folder.

## Messaging: consumes the single AuditIntegrationEvent, publishes nothing (yet)

`Audit.Infrastructure/Messaging/Consumers/` has exactly one consumer — `AuditIntegrationEventConsumer`, for `BuildingBlock.Contract.Events.Audit.AuditIntegrationEvent` (topic `auditintegrationevent`) — deserializing and sending a `RecordAuditLogCommand` directly via MediatR's `ISender` (not routed through an internal event + handler, since nothing else needs to react to "an audit row should be written" — this is a deliberate, minimal simplification, not a missed pattern).

This replaces five older per-business-event consumers (`ProductCreatedAuditConsumer`, `UserDeletionAuditConsumer`, `UserProfileCreatedAuditConsumer`, `OrderCreatedAuditConsumer`, `OrderCancelledAuditConsumer`), removed as part of the aggregate-graph audit redesign. `ProductCreatedIntegrationEvent`/`UserDeletionIntegrationEvent`/`UserProfileCreatedIntegrationEvent` still exist and are still published — they have real business consumers elsewhere (Inventory, Order, User, Auth) — Audit simply no longer subscribes to them. `OrderCreatedIntegrationEvent`/`OrderCancelledIntegrationEvent` were deleted entirely: Audit was their only consumer, so once Audit stopped needing them there was nothing left to keep them alive for.

`RecordAuditLogHandler` maps the event's `AuditNode` tree (Contract) to `AuditTrailNode` (Domain) recursively, then is a pure sink: `IAuditLogWriteService.AddAsync(entry, ct)`, which internally does `repo.AddAsync` then `unitOfWork.SaveChangesAsync` (the latter a documented no-op for Mongo, see below) — no Outbox enqueue.

**Outbox is wired but unused.** `AuditMongoContext` implements both `IOutboxMongoContext` and `IInboxMongoContext`, and `Audit.Infrastructure` registers the full `AddInboxOutboxCleanupJobs()` helper (dual shape, like User) rather than hand-registering one job. Nothing currently calls `outboxStore.EnqueueAsync` — this was a deliberate choice (confirmed during scaffolding) to prove `BuildingBlock.Persistence.Mongo`'s `MongoOutboxStore` end-to-end even though Audit has no outbound event of its own yet. If Audit ever needs to publish (e.g. an `AuditLogRecordedIntegrationEvent` for a downstream alerting service), the wiring is already in place.

## Persistence: MongoDB, not EF — the differences from every other service

This is the first (and so far only) service using `BuildingBlock.Persistence.Mongo` instead of `BuildingBlock.Persistence.Ef`. Concretely, that changes:

- **`AuditMongoContext`** (`Audit.Persistence/AuditMongoContext.cs`) replaces a `DbContext` — it implements `IOutboxMongoContext`/`IInboxMongoContext` (the Mongo-provider equivalents of `IOutboxDbContext`/`IInboxDbContext`) and exposes `IMongoCollection<T>` properties instead of `DbSet<T>`. The `AuditLogs` collection is named `"logs"` and lives in the `audit_logs` Mongo database — both names are pre-provisioned by `scripts/mongodb/init-mongo.js` (which also creates the `timestamp`/`service` indexes `AuditLogReadService.SearchAsync` relies on) and must not be renamed without updating that script.
- **No migrations.** Mongo is schemaless; there is no `Migrations/` folder and `Audit.API/Program.cs` has no `MigrateAsync()` call. `AuditMongoContext`'s constructor calls `OutboxMessages.EnsureOutboxIndexes()`/`InboxMessages.EnsureInboxIndexes()` instead — the closest Mongo equivalent to EF's `OnModelCreating`, run once since the context is registered as a Singleton (see below).
- **`TContext` is registered as a Singleton, not Scoped.** `AddPersistenceMongoContext<TContext>` (`BuildingBlock.Persistence.Mongo`) registers `IMongoClient`/`IMongoDatabase`/`TContext` all as singletons — the Mongo driver's client and collection handles are stateless and thread-safe by design, and there's no per-request change tracker to isolate the way EF's `DbContext` needs. Stores (`MongoOutboxStore`/`MongoInboxStore`) stay Scoped, same as the EF equivalents.
- **`UnitOfWork.SaveChangesAsync` is a documented no-op.** Mongo writes (`InsertOneAsync`, `ReplaceOneAsync`, ...) commit immediately per call — there's nothing to batch or flush. `Audit.Persistence/UnitOfWork/UnitOfWork.cs` implements `IUnitOfWork` directly (no `EfUnitOfWork<TContext>`-style base class exists for Mongo, since there's nothing to abstract) so Application handlers can still depend on the same `IUnitOfWork` interface every other service uses without special-casing Audit.
- **Read/Write persistence services, per the [persistence refactor](../refactoring/persistence-refactor-plan.md) (Phase 2, the reference Mongo implementation).** `Audit.Application/Abstractions/Persistence/AuditLogs/{IAuditLogReadService,IAuditLogWriteService}` are what handlers inject — never a repository interface. `IAuditLogRepository` (`Audit.Persistence/AuditLogs/Repositories/`) is Persistence-internal now, trimmed to just `AddAsync` (append-only, so that's the whole mutation surface); `GetByIdAsync`/`SearchAsync` moved to `AuditLogReadService`, which queries `AuditMongoContext` directly rather than through the repo. None of this is a generic `IRepository<T>` — same reasoning as Inventory's `IInventoryTransactionRepository` — and all three are registered explicitly in `Audit.Persistence/DependencyInjection.cs` (Mongo services don't use the `AddScopedByInterface(typeof(IRepository<>), ...)` scan at all).
- **Element names are camelCase by convention, not attribute-mapped.** `BuildingBlock.Persistence.Mongo.AddPersistenceMongoContext` registers a process-global `CamelCaseElementNameConvention` once (the Mongo equivalent of EF's `UseSnakeCaseNamingConvention()`), so `AuditLogEntry.Timestamp`/`.Service` serialize as `timestamp`/`service` — matching the pre-built indexes — without any `[BsonElement]` attributes on the Domain entity itself (which would leak a Mongo-specific dependency into `Audit.Domain`) or a manual `BsonClassMap` registration in `Audit.Persistence`.
- **Hangfire storage is still Postgres**, regardless of Audit's primary datastore being Mongo. `BuildingBlock.Infrastructure`'s Hangfire wiring (`HangfireSchedulingExtensions`) is hard-coded to `Hangfire.PostgreSql` — there's no Mongo storage provider for Hangfire anywhere in this codebase, and adding one was out of scope for this service. Audit therefore gets its own `audit_hangfire_db` database on the shared `pg` container purely for Hangfire's own bookkeeping tables (recurring-job schedule, job queue); Audit's actual domain data (`AuditLogEntry` documents) never touches Postgres.

## Naming note: no `Audit`/`Audit` collision, unlike Product/Inventory/Order

Product's entity is `Product`, Inventory's is `Inventory`, Order's is `Order` — each colliding with its own root namespace and requiring a `GlobalUsings` alias (see [order-service.md](order-service.md#naming-note-the-order-entity-vs-the-order-root-namespace)). Audit's primary entity is deliberately named `AuditLogEntry`, not `Audit` — "Audit Service" is a capability name, not a noun the service "is" the way Product *is a* Product; the pre-staged Mongo collection is literally named `logs`, signaling "log entries" as the domain noun. This avoids the alias workaround entirely; `Audit.Application`/`Audit.Persistence` have no `GlobalUsings` alias for this reason.

## Known issues

- No per-node index collection over the nested `Root` tree — queries only ever filter on the flat top-level fields (`RootEntityType`/`RootEntityId`/`Service`/`Timestamp`). Fine today since nothing needs to search *inside* arbitrary graphs; would need to be added if that ever changes. See [reference/audit-trail.md](../reference/audit-trail.md#mongodb-persistence-strategy-audit-service).
