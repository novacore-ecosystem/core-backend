# Auth Service

**Scope:** Auth-specific facts — routes, config, folder layout, known state. This is **the reference implementation** — when a general pattern doc says "see the reference implementation," it means here. General patterns themselves live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md), not repeated here.

## Projects

`Auth.Domain`, `Auth.Application`, `Auth.Infrastructure`, `Auth.Persistence`, `Auth.API` — standard 5-layer split, see [02-architecture-rules.md](../02-architecture-rules.md#layer-responsibilities).

## Ports & routing

- Internal: `8080` (REST), `5002` (gRPC client target for User Service). Not published to host directly — only reachable through the Gateway.
- Gateway path prefix: `/api/auth/` (`RequireAuth: false` — Auth's own endpoints are anonymous; they issue the tokens other services require).

## Routes (Carter endpoints, `Auth.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/register` | `Register.cs` | Create account, triggers `OnUserRegisteredEvent` → gRPC `CreateUserProfile` on User Service |
| POST | `/login` | `Login.cs` | Issue AccessToken/RefreshToken cookies |
| POST | `/logout` | `Logout.cs` | Revoke refresh token, clear cookies |
| POST | `/refresh-token` | `RefreshToken.cs` | Reads `RefreshToken` cookie, validates against Redis, issues new tokens. **Also filtered at the Gateway** — see [gateway.md](gateway.md) |

## DI composition (`Auth.API/DependencyInjection.cs`, `Auth.API/ApplicationPipeline.cs`)

```csharp
// Program.cs
builder.Services.AddPersistence(config).AddApplication().AddInfrastructure(config).AddPresentation(config);
// AddPresentation → AddBuildingBlockWeb(config, WebOptions) + AddCommonAuthorizationPolicies()
// UseApplication → SeedDatabase, InitializeRefreshTokenCache, UseBackgroundJobsDashboard/Scheduling, UseBuildingBlockWeb, MapEndpoints
```

`Auth.Infrastructure/DependencyInjection.cs` chains: `AddAppLogger → AddRedisCache → AddRoleCaching → AddBackgroundJobs → AddInboxOutboxCleanupJobs → AddSecurityServices → AddApplicationEventDispatcher → AddKafkaMessaging("auth-service") → AddGrpcClients`. (`AddApplicationEventDispatcher` registers `IInternalEventDispatcher` — legacy method name, see [reference/events.md](../reference/events.md#the-two-tiers).) `AddRoleCaching` decorates `IAuthService` with `CachedAuthServiceDecorator` — this is why `AddPersistence` must run before `AddInfrastructure` (see [02-architecture-rules.md](../02-architecture-rules.md#composition-root-convention-per-service)).

## Auth-specific building blocks (not shared with User)

- **Hangfire recurring job** (`Auth.Infrastructure/BackgroundJobs/Jobs/RefreshTokenSync/`) — `RefreshTokenSyncService : IRecurringJob`, dashboard at `/hangfire`. The Hangfire bootstrap itself (`AddHangfireScheduling`, recurring-job discovery, dashboard) now lives in `BuildingBlock.Infrastructure/BackgroundJobs/` and is shared with User (see [user-service.md](user-service.md)) — Auth's `BackgroundJobsExtensions` is a thin wrapper that only supplies its own job-assembly marker and dashboard title. See [workflows/add-background-job.md](../workflows/add-background-job.md).
- **Inbox/Outbox cleanup jobs** — shared `OutboxCleanupJob`/`InboxCleanupJob` (`BuildingBlock.Infrastructure/BackgroundJobs/Cleanup/`), registered via `.AddInboxOutboxCleanupJobs(configuration)`. See [reference/inbox-outbox-runtime.md](../reference/inbox-outbox-runtime.md#cleanup).
- **JWT issuance** — `Auth.Infrastructure/Security/Jwt/JwtTokenGenerator.cs` (`IJwtTokenGenerator`) creates the tokens; `Auth.Infrastructure/Security/RefreshTokens/RefreshTokenService.cs` manages refresh-token lifecycle in Redis (key: `refresh_token_by_string:{token}`, this is the format the Gateway's filter middleware also reads — see [gateway.md](gateway.md)). Token *validation* middleware (`AddJwtBearerAuthentication`) is shared via `BuildingBlock.Web`, same as every other service.
- **gRPC client** — `Auth.Infrastructure/GrpcClients/UserProfileServiceClient.cs` calls User Service's `CreateUserProfile` after registration.
- **Role caching** — `RoleCacheService` + `CachedAuthServiceDecorator`, full pattern in [reference/caching.md](../reference/caching.md).

## Persistence: Read/Write services, and where Identity fits

Phase 3 of the [persistence Read/Write migration](../refactoring/persistence-refactor-plan.md). `Auth.Application/Abstractions/Persistence/{Accounts,RefreshTokens}/` hold `IAccountReadService`/`IAccountWriteService` and `IRefreshTokenReadService`/`IRefreshTokenWriteService` — the only persistence ports Application/Infrastructure inject now; the old `IAccountRepository`/`IRefreshTokenRepository` are gone from Application entirely. Two things distinguish Auth from a typical phase:

- **`AccountReadService`/`AccountWriteService` are intentionally thin.** Tracing every real caller solidly ahead of writing them showed `IAccountRepository` had exactly two live consumers — `OnAccountDeletionInitiatedHandler` (`DeleteIfExistAsync`) and `Auth.API/GrpcServices/AuthGrpcServiceImpl.cs` (`GetByEmailAsync`, a gRPC service, not a MediatR handler — easy to miss with a `*Handler.cs`-only search). Everything else on the old interface (`GetByIdAsync` ×2, `UpdateAsync` ×2, `GetActiveUsersAsync`) had zero callers anywhere in the solution and was dropped rather than carried forward. Actual Account *mutation* (register, update, delete-for-real) goes through `IAuthService`/ASP.NET Identity's `UserManager<Account>` (auto-saves internally), which stayed completely out of this migration's scope — it's a separate concern from the repository.
- **`RefreshTokenWriteService.AddAsync`/`UpdateAsync` are deliberately non-committing.** Their only caller, `RefreshTokenSyncService` (`Auth.Infrastructure/BackgroundJobs/Jobs/RefreshTokenSync/`), batches many adds/updates for a whole user-batch inside one `IUnitOfWork.ExecuteTransactionAsync` it owns itself (with its own deadlock-retry loop) — so unlike every other Write Service in this migration, these two methods just stage the mutation and let the caller decide the commit boundary. This is the same "named exception" shape as Inventory's cross-aggregate stock transactions (Correction 2 in the tracker), just triggered by batching within one aggregate type instead of across aggregate roots. `RefreshTokenReadService.GetByUserIdAsync` is a normal read, used by `RefreshTokenInitializationService` at startup.

## Tenant foundation (Phase 1 - domain & persistence only)

Introduced 2026-08-05 as the first foundation for multi-tenant support, scoped deliberately narrow:
Domain entities, EF configuration, migration, and repository/Read-Write-service wiring only. No
JWT/claims/authentication/authorization/CurrentUser/middleware/bootstrap-API/SignalR/Redis/query
filtering was touched — those are later phases' concern. Four entities, two aggregate roots:

- **`Tenant`** (`Auth.Domain/Entities/Tenants/Tenant.cs`, `AggregateRoot<Guid>`) - one
  customer/company using the platform. Carries only bootstrap/branding identity (`Code`, `Name`,
  `LogoUrl`/`FaviconUrl`, `Version`, `Metadata`, `IsActive`) - deliberately excludes subscription,
  billing, licensing, and feature management, which are out of scope for this aggregate entirely
  (not just this phase).
- **`TenantLocale`** (owned child of `Tenant`) - the entire bootstrap resource set
  (`ConfigurationJson` + `DictionaryJson`) for one locale. A `null` `LanguageCode` is the fallback
  resource, enforced to exactly one per tenant via a partial unique index
  (`ix_tenant_locales_tenant_id_fallback`, `WHERE language_code IS NULL`) alongside the regular
  `(tenant_id, language_code)` unique index. Both JSON payloads are intentionally opaque
  (validated as well-formed JSON in `TenantLocale.Create`/`UpdateContent`, stored as `jsonb`) -
  **not** wrapped in a `MetadataBase`-style typed accessor like `ProductMetadata`, because
  bootstrap resources are denormalized per-locale blobs whose shape is owned by the frontend
  bootstrap contract, not a fixed set of Domain-known fields. Unlike every other translation
  entity in this codebase, `TenantLocale` keeps its own generated `Id` plus a separate `TenantId`
  foreign key rather than reusing the parent's id as a shared composite PK - a `null`
  `LanguageCode` can't participate in a `PRIMARY KEY` column, which the shared-PK shape requires.
- **`Scope`** (`Auth.Domain/Entities/Scopes/Scope.cs`, `AggregateRoot<Guid>`) - a business scope
  inside a Tenant (Branch/Agency/Dealer/Region/...), organized as a self-referencing hierarchy via
  `ParentScopeId`. Mirrors `ProductCategory`'s shape exactly: shadow-navigation FK only (no
  Domain-level `Parent`/`Children` collections), `Path`/`Level` exist purely to simplify hierarchy
  bookkeeping (breadcrumbs, depth) and are recomputed by the caller on `ChangeParent` - this
  aggregate does not do scope/tenant query filtering itself, and `Path` is explicitly not a
  filtering mechanism (that's a later phase). `Code` is unique per `(TenantId, Code)`, not
  platform-wide.
- **`ScopeTranslation`** (owned child of `Scope`) - standard business translation (unlike
  `TenantLocale`'s bootstrap data), following `ProductTranslation`/`RoleTranslation`'s shape
  exactly: `Id` doubles as the owning `Scope`'s id, composite `(Id, LanguageCode)` primary key.

Both `Tenant` and `Scope` follow the standard Repo/Read-Service/Write-Service split (see
[conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md)):
`ITenantRepository`/`IScopeRepository` are empty markers (Scrutor-scanned via
`AuthBaseRepository<T>`, same as `RefreshTokenRepo` - not manually registered), and
`ITenantReadService`/`ITenantWriteService`/`IScopeReadService`/`IScopeWriteService` expose only
the minimal surface inferable without a real caller yet (`GetByCodeAsync`/`ExistsByCodeAsync`,
self-committing `CreateAsync`) - no CQRS commands/handlers/endpoints exist for either aggregate
yet; that's the bootstrap-API phase.

## Tenant Client / Public Key foundation (Phase 1 - domain & persistence only)

Introduced 2026-08-09 as the domain/persistence groundwork for tenant public-key based client
identification - resolving *which Tenant* an unauthenticated client belongs to, before
username/password authentication happens: `PublicKey -> TenantClient -> TenantId -> login ->
access token`. Scoped exactly as narrow as the Tenant Foundation phase before it - no JWT claims,
login API, Gateway, Redis, or revocation-enforcement middleware were touched; those are later
phases' concern.

- **`TenantClient`** (`Auth.Domain/Entities/TenantClients/TenantClient.cs`, `AggregateRoot<Guid>`)
  - an independent aggregate, one row per client credential (Web/Mobile/Admin/...) belonging to
  exactly one `Tenant` (`TenantId`, plain FK - see below). A `Tenant` can have any number of
  `TenantClient`s; there is no back-collection on `Tenant` (same shadow-FK shape as `Scope`, not an
  owned child like `TenantLocale`). Fields: `Name` (admin-facing label), `PublicKey`
  (`ClientPublicKey` VO), `Status` (`TenantClientStatus`: `Active`/`Revoked`/`Expired`),
  `ExpiresAt` (optional), `RevokedAt`/`RevokedReason` (reuses the existing `RevocationReason`
  enum already shared by `Session`/`RefreshToken`). `Revoke`/`MarkExpired` are idempotent no-ops
  once already non-`Active` (same shape as `Session.Revoke`); `IsUsable()` is a computed Domain
  invariant (`Active` and not past `ExpiresAt`), not enforcement - no Redis/Gateway check exists
  yet, this is purely what a later resolution query would rely on.
- **`ClientPublicKey`** (`Auth.Domain/ValueObjects/ClientPublicKey.cs`, `StringValueObject`) - a
  32-byte (256-bit) CSPRNG value, lowercase-hex-encoded (64 chars), generated only via
  `Generate()` (never user-supplied, never derived from `TenantId`/`TenantCode`). Not a secret -
  intentionally safe to ship client-side; it answers "which Tenant is this," never "is this
  request authorized."
- **Deliberately does NOT implement `ITenantEntity`.** This is the load-bearing design decision of
  this phase: the Entity Convention's query filter
  (`ModelBuilderExtensions.ApplyEntityConventions`, see
  [reference/tenant-convention.md](../reference/tenant-convention.md)) compares `TenantId` against
  `RequestContext.Current.TenantId`, which is `Guid.Empty` for exactly the anonymous,
  pre-authentication requests `TenantClient` exists to serve - opting in would make every
  `PublicKey` lookup return zero rows, since no real `TenantClient` ever has `TenantId ==
  Guid.Empty`. `TenantId` is therefore a hand-mapped column/index in `TenantClientConfig`
  (`.Property(x => x.TenantId)` + `.HasIndex(x => x.TenantId)`), not the automatic
  convention-driven mapping `Scope`/`TokenBlacklist`/etc. get for free. No query filter is applied
  to `tenant_clients` at all.
- **EF configuration** (`Auth.Persistence/Configs/TenantClientConfig.cs`): unique index on
  `PublicKey` (the primary future lookup path), plain index on `TenantId`, composite index on
  `(TenantId, Status)` (admin "list this tenant's active clients"). FK to `Tenant` cascades on
  delete, same as `Scope`'s. `Status` persists as `smallint` via `.HasConversion<short>()`, same
  pattern as `InvitationStatus`.
- **Repo/Read-Service/Write-Service**: `ITenantClientRepository` (empty marker, Scrutor-scanned via
  `AuthBaseRepository<TenantClient>`) + `ITenantClientReadService`
  (`GetByPublicKeyAsync`/`ExistsByPublicKeyAsync`) + `ITenantClientWriteService`
  (self-committing `CreateAsync`) - the exact same minimal shape as `Tenant`'s own Phase 1. No CQRS
  commands/handlers/endpoints exist yet.
- **Migration**: `20260809070959_AddTenantClient` - additive only (new `tenant_clients` table),
  not applied to any database.
- **Explicitly out of scope for this phase** (see the phase's own design doc for the full list):
  login API changes, JWT `tenant_id` claim, `PublicKey` resolution endpoint, Redis/Gateway
  revocation enforcement, and any `Session`/`RefreshToken` -> `TenantClient` relationship (no
  such FK exists yet - deferred as an explicit next-phase design decision, not silently assumed).

## Known state

- Mapster is registered but unused — hand-mapping is the actual convention (see [04-coding-rules.md](../04-coding-rules.md#mapping)).
- `Register` → identity persistence goes through ASP.NET Identity's `UserManager` (auto-saves); it does **not** call `IUnitOfWork.SaveChangesAsync` explicitly. The `ExecuteTransactionAsync` pattern (see [04-coding-rules.md#transaction-management](../04-coding-rules.md#transaction-management)) is demonstrated in `RefreshTokenSyncService.SyncCacheToDbAsync`, not in Register/Login.
- gRPC to User Service is config-gated (`Grpc:UserService:Url`, with a stub fallback historically used during local dev without User Service running) — check current `Auth.Infrastructure/GrpcClients/DependencyInjection.cs` wiring before assuming it's always live.
- **Migration history was reset 2026-08-05.** While adding the Tenant Foundation migration, roughly 18 tables (`positions`, `permission_groups`/`permission_definitions` + translations, `invitations`, `sessions`, `devices`, `mfa_methods`/`mfa_backup_codes`, `token_blacklists`, `account_positions`/`account_permissions`, `position_roles`/`position_translations`, `role_permissions`, and others) turned out to exist in `AuthDbContext`/Domain with zero corresponding migration ever committed - `AuthDbContextModelSnapshot.cs` had drifted far ahead of the actual migration file history. Rather than bundle that unrelated backlog into a "clean" Tenant Foundation migration, the entire migration history was collapsed into a single fresh `InitialCreate` (2026-08-05) that captures the complete current model (all pre-existing tables + Tenant/Scope) as a new baseline. Any environment with a real database from before this date needs to be rebuilt from this new baseline rather than upgraded in place.
- A design-time factory (`Auth.Persistence/Engine/AuthDbContextFactory.cs`, `IDesignTimeDbContextFactory<AuthDbContext>`) is required for `dotnet ef migrations` tooling - building `AuthDbContext` through the full application host fails at design time because Identity 10.0's `AddEntityFrameworkStores<AuthDbContext>` registration resolves a `UserStore` generic signature the design-time host's DI container can't satisfy. The factory mirrors `AddPersistenceDbContext`'s options (including `UseSnakeCaseNamingConvention()`) so scaffolded migrations match the runtime model; this doesn't affect runtime resolution (`Auth.API` via `AddPersistence`).
