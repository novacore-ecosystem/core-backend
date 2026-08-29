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
| POST | `/login` | `Login.cs` | Resolves tenant context from the `X-Tenant-Client-Key` header, then issues AccessToken/RefreshToken cookies |
| POST | `/logout` | `Logout.cs` | Revoke refresh token, clear cookies |
| POST | `/refresh-token` | `RefreshToken.cs` | Reads `RefreshToken` cookie, validates against Redis, issues new tokens. **Also filtered at the Gateway** — see [gateway.md](gateway.md) |
| GET | `/tenants` | `ListTenants.cs` | Paginated/searchable Tenant Management list, `tenant:view` |
| GET | `/tenants/{id}` | `GetTenant.cs` | Full Tenant Management editing payload, `tenant:view` |
| POST | `/tenants` | `CreateTenant.cs` | Create tenant, `tenant:manage` |
| PUT | `/tenants/{id}` | `UpdateTenant.cs` | Update name/branding, bumps bootstrap Version, `tenant:manage` |
| POST | `/tenants/{id}/disable` | `DisableTenant.cs` | Idempotent deactivate, `tenant:manage` |
| DELETE | `/tenants/{id}` | `DeleteTenant.cs` | Soft delete (`ISoftDeleteEntity`), `tenant:manage` |
| PUT | `/tenants/{id}/translations` | `UpsertTenantTranslation.cs` | Key-level dictionary upsert, `tenant:manage` |
| PUT | `/tenants/{id}/dictionary/{language}` | `UpdateTenantDictionary.cs` | Bulk per-language dictionary merge-update, `tenant:manage` |
| PUT | `/tenants/{id}/config` | `UpdateTenantConfig.cs` | Per-locale config merge-update (fallback if no `language` query param), `tenant:manage` |
| POST | `/tenants/{id}/client/rotate` | `RotateTenantClient.cs` | Revoke every Active TenantClient, issue a new one, `tenant:rotate-client` |
| GET | `/bootstrap` | `GetTenantBootstrap.cs` | Pre-authentication bootstrap, identified by `X-Tenant-Client-Key` (anonymous) |
| POST/GET/PUT/DELETE | `/roles`, `/roles/{id}`, `/roles/{id}/permissions` | `CreateRole.cs`/`ListRoles.cs`/`GetRole.cs`/`UpdateRole.cs`/`UpdateRolePermissions.cs`/`DeleteRole.cs` | Role CRUD + permission-set replacement, `role:view`/`role:manage` |
| GET/PUT/DELETE | `/permissions`, `/permissions/{id}` | `ListPermissions.cs`/`GetPermission.cs`/`UpdatePermission.cs`/`DeletePermission.cs` | Permission catalog read + regroup + delete (system permissions blocked), `permission:view`/`permission:manage` |

See "Tenant Management & Bootstrap APIs (Phase 5)" below for the full contract of every `/tenants*`/`/bootstrap` route.

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
  **Superseded by Phase 2 below** for login/JWT claim - the rest still stands.

## Tenant-aware Login (Phase 2)

Introduced 2026-08-09, immediately after the Phase 1 commit. Makes `POST /login` resolve tenant
context from a `TenantClient` PublicKey *before* checking credentials, for both tenant clients and
Root, under the one existing endpoint - no `/root/login`/`/tenant/login` split. Two structural gaps
surfaced during audit and were resolved with the user before implementation (see the design
decisions below); everything else preserves Login/Refresh/Logout's existing behavior.

- **`X-Tenant-Client-Key` header** (`HeaderKeyConstant.TenantClientKey`) carries the `TenantClient`
  PublicKey on `POST /login` (`[FromHeader]` on the Carter route, not `RequestContext` - the raw
  key is a pre-auth *credential to resolve*, not identity itself, so it doesn't belong in
  `RequestContextData`). Deliberately a new constant, not a reuse of the pre-existing-but-unused
  `HeaderKeyConstant.TenantId` ("X-Tenant-Id") - that one's own doc comment describes it as
  carrying an already-resolved TenantId, never an opaque public key; the two are not
  interchangeable and `X-Tenant-Id` is left untouched/still unused.
- **`TenantClient.TenantId` is now `Guid?`** (was a required `Guid`). A null `TenantId` is the Root
  client - a global identity, not a Tenant. Reuses the one `TenantClient`/`ClientPublicKey`
  aggregate rather than a sentinel Tenant row or a parallel `RootClient` type (`IsRootClient =>
  TenantId is null` reads the condition). `TenantClientConfig`'s FK to `Tenant` became optional;
  `Status`/`PublicKey`/lifecycle methods are unchanged.
- **`Account` gained `TenantId`** (`Guid`, default `Guid.Empty`). Uses the same
  Guid.Empty-means-"no tenant" sentinel every sibling entity (`Session`, `RefreshToken`, `Device`,
  ...) already uses - the seeded Root account is `TenantId == Guid.Empty`, not a distinct
  null/global representation. **Deliberately does NOT implement `ITenantEntity`**, unlike those
  siblings: `Account` is wrapped end-to-end by ASP.NET Core Identity's `UserManager`
  (`FindByIdAsync`, `FindByEmailAsync`, `CheckPasswordAsync`, ...), which queries `Users` with no
  `RequestContext` awareness. Opting into the Entity Convention's automatic query filter would have
  silently scoped every one of those calls to `RequestContext.Current.TenantId` (`Guid.Empty` for
  literally every request today, since no code path emitted `tenant_id` before this phase) -
  breaking `GetUserByIdAsync`/`GetUserRolesAsync`/`AssignRoleAsync`/etc. for any real tenant user,
  invisibly. `TenantId` is a plain, hand-mapped column instead (`AccountConfig`), read explicitly
  only where tenant-scoped lookup is needed.
- **Username uniqueness is now tenant-scoped.** ASP.NET Core Identity's own `OnModelCreating`
  declares a global-unique index on `NormalizedUserName` ("UserNameIndex"). `AccountConfig`
  re-targets that same index to non-unique and adds `(TenantId, NormalizedUserName)` as the real
  unique constraint - the same username may now exist in two different tenants. Register's own
  behavior is unchanged (still creates `TenantId == Guid.Empty` accounts by default); tenant-scoped
  self-registration is not implemented in this phase.
- **`IAccountReadService.GetByEmailAsync(email, tenantId)`** (new overload) is what Login actually
  calls - `dbContext.Users.Where(u => u.Email == email && u.TenantId == tenantId)`, bypassing
  `UserManager` entirely for this one lookup so the tenant boundary is explicit, not ambient.
  Password verification then goes through a new `IAuthService.ValidateCredentialsAsync(Account,
  password)` overload (`UserManager.CheckPasswordAsync` against the already-resolved entity) -
  the existing `ValidateCredentialsAsync(email, password)` overload still exists for other callers
  but is no longer used by Login, since its internal `FindByEmailAsync` has no tenant awareness.
- **`JwtTokenGenerator.GenerateAccessToken`** gained a required `tenantId` parameter and emits
  `AppClaimTypes.TenantId` ("tenant_id") only when it is not `Guid.Empty` - completing the claim
  `RequestContextMiddleware` was already wired to read but nothing emitted yet (see
  [reference/tenant-convention.md](../reference/tenant-convention.md)). Root logins therefore carry
  no `tenant_id` claim at all, which resolves back to `Guid.Empty` on read - consistent with every
  other entity's sentinel, no special-casing needed. All three call sites (`LoginHandler`,
  `RefreshTokenHandler`, `RegisterHandler`) now pass this by name (`tenantId: ...`) - the old
  positional call (`..., permissions, jwtId)`) would otherwise have silently bound `jwtId` into the
  new `tenantId` slot and left the real `jti` claim unset, since both are `Guid`/`Guid?` in the same
  position. `RefreshTokenHandler` reads `user.TenantId` off the already-fetched `Account` (no new
  lookup, no header) - refresh preserves whatever tenant context Login established.
- **`TenantClientSeeder`** (new, wired into `DatabaseSeeder`) seeds exactly one Root `TenantClient`
  (`TenantId == null`) on a fresh database, idempotent like `AccountSeeder`/`RoleSeeder`. Its
  generated `PublicKey` is logged once (`LogWarning`) at creation - there is no other bootstrap
  channel for it yet, same local-dev-only tradeoff `SeedData.Accounts.RootPassword` already accepts.
- **`GET /tenants`** (`ListTenants.cs`, `Permissions.Root`) - Root Portal tenant discovery/selection
  only. Returns `Id`/`Code`/`Name`/`LogoUrl`/`IsActive` - never `PublicKey`, `Metadata`, or any
  per-tenant business data. Backed by new `ITenantReadService.ListAsync` (unpaginated, same
  reasoning as `INotificationChannelReadService.ListAsync` - an operator-facing picker, not a
  customer-facing catalog).
- **Migration**: `20260809075108_AddAccountTenantAndRootClientSupport` - adds `users.tenant_id`
  (default `Guid.Empty`), re-scopes `UserNameIndex` to non-unique, adds the
  `(tenant_id, normalized_user_name)` unique index, and makes `tenant_clients.tenant_id` nullable.
  Not applied to any database.
- **Two design decisions were confirmed with the user before implementation** (both because the
  spec explicitly calls for stopping rather than silently resolving a structural gap): making
  `TenantClient.TenantId` nullable for Root (rather than a sentinel Tenant row or a separate
  `RootClient` type), and adding `TenantId` directly to `Account` (rather than an `AccountTenant`
  membership entity, or deferring tenant-user binding entirely).
- **Refresh/Logout required no other changes.** Refresh already re-fetches the `Account` by id
  every rotation, so `user.TenantId` is available for free once `Account` carries it; Logout only
  ever revokes the current refresh-token cookie, no tenant awareness applies.
- **Explicitly out of scope, unchanged from Phase 1's list**: `PublicKey` revocation enforcement
  (Redis/Gateway), Redis Pub/Sub cache sync, tenant impersonation/investigation, tenant
  subdomain/custom-domain routing, and tenant-scoped self-registration.

## Authorization foundation (Phase 3)

Introduced 2026-08-09. Replaces `RolePermissionMap`'s hard-coded C# switch with the real,
DB-backed `Permission -> Role -> RolePermission[]` graph already modeled in `Auth.Domain` since
before this phase (nothing new there) - this phase seeds it, resolves it, exposes it through a
management API, and propagates changes to User Service. **No `RoleGroup` was introduced anywhere**
- `Role` groups permissions, `Position` remains the sole organizational-hierarchy concept, exactly
as before.

- **Global vs tenant-scoped, confirmed by audit, not changed**: `Role`/`PermissionDefinition`/
  `PermissionGroup` have no `TenantId` - one shared, platform-wide catalog. `RolePermission`/
  `PositionRole`/`AccountPosition` all already implemented `ITenantEntity` before this phase - the
  *grant* of a Role's permissions is tenant-scoped even though the Role/Permission identities
  themselves are global. This phase's seeding and resolution code only had to use that shape
  correctly, not invent it.
- **`Permissions.User`** (`"system:user"`) added next to the existing `Permissions.Root`
  (`"system:root"`) - the two mandatory permissions. Plus `Permissions.Role.{View,Manage,Full}` and
  `Permissions.Permission.{View,Manage,Full}` for the new management API below.
- **`PermissionCatalogSeeder`** (new) seeds one `PermissionGroup` per `Permissions.cs` module and
  one `PermissionDefinition` per `Permissions.SupportedValues` entry - mechanical, driven entirely
  by already-declared constants, not a second permission-definition system. **`RolePermissionSeeder`**
  (new) then grants the seeded Root/Admin/User system Roles their `RolePermission` rows, exactly
  mirroring `RolePermissionMap`'s former mapping for Root/Admin (no regression) plus the new
  `Permissions.User` grant on the `User` role (previously empty). Both run via
  `TenantAssignmentInterceptor` like any other write, so every seeded grant lands on
  `TenantId == Guid.Empty` - Root/global scope, matching `Account`'s own seeded Root row.
- **`PermissionDefinition.MoveToGroup` no longer blocks `IsSystemPermission`** - only deletion is
  blocked (in `DeletePermissionHandler`, the one place deletion is possible; `PermissionDefinition`
  has no Domain-level Delete method to guard itself). This is a deliberate Phase 3 correction: the
  "`root`/`user` can be updated, cannot be deleted" invariant does not mean *no* mutation, and the
  prior blanket guard on `MoveToGroup` was stricter than required.
- **`IEffectivePermissionReadService`** (new, `Auth.Persistence/Contexts/Authorization/`) replaces
  `RolePermissionMap.Resolve(roles)` everywhere it was called (`LoginHandler`/`RefreshTokenHandler`/
  `RegisterHandler`, now deleted). `GetEffectivePermissionsAsync(accountId, tenantId)` unions direct
  `AccountRole` grants with Position-derived ones (`AccountPosition` -> `Position` -> `PositionRole`),
  then resolves `RolePermission` -> `PermissionDefinition.Key`, deduplicated. `tenantId` is always
  an explicit parameter, never `RequestContext.Current` - the primary caller (Login) resolves this
  before any tenant claim exists, and `AccountPosition`/`PositionRole`/`RolePermission` are all
  `ITenantEntity`, so the query uses `IgnoreQueryFilters()` + an explicit `TenantId` equality
  instead of the ambient automatic filter (first real use of that escape hatch in this codebase -
  Phase 1/2 avoided it entirely by not implementing `ITenantEntity` on `TenantClient`/`Account`).
  **All three `GenerateAccessToken` call sites now pass every argument by name** - the old
  positional call (`..., permissions, jwtId)`) would otherwise have silently bound `jwtId` into the
  new `tenantId` parameter (both `Guid`/`Guid?` in the same position) and left `jti` unset, caught
  during this phase's own review before it shipped.
- **Role/Permission management API** (`Auth.API/Endpoints/{Create,List,Get,Update,Delete}Role*.cs`,
  `{List,Get,Update,Delete}Permission.cs`) - standard Read/Write-Service CRUD, same shape as every
  other Auth aggregate. `PUT /roles/{id}/permissions` replaces a Role's permission set wholesale
  (client sends the desired `PermissionKey[]`, the handler diffs against current grants and applies
  `AssignPermission`/`RemovePermission` internally) rather than separate assign/remove endpoints.
  `DeleteRoleHandler`/`DeletePermissionHandler` block deletion of `IsSystemRole`/`IsSystemPermission`
  rows. Protected by the new `role:*`/`permission:*` permissions - Root satisfies these
  automatically (`Permissions.Root` bypasses every check).
- **`AccountEffectivePermissionsChangedIntegrationEvent`** (new,
  `BuildingBlock.Contract/Events/User/`) - published by `UpdateRolePermissionsHandler` for every
  Account holding the changed Role (directly or via an effective Position), carrying the
  already-recomputed, final permission array. **Two-phase commit, not one atomic transaction**: the
  `RolePermission` mutation commits first (`RoleWriteService` self-commits, matching every other
  Auth Write Service), then affected Accounts' recomputed permissions are enqueued as a second
  `SaveChangesAsync` - the usual Outbox atomicity guarantee is deliberately traded away here,
  documented in `UpdateRolePermissionsHandler`'s own class doc comment. Direct `AccountRole`
  assignment (outside Register's fixed "User" role grant) has no dedicated admin endpoint yet, so
  it does not publish this event - an explicitly deferred gap, not an oversight.
- **User Service**: new `UserAuthorizationSnapshot` (`User.Domain/Entities/Users/`, 1:1 owned,
  `text[]` + GIN index, same shape as the pre-existing `UserPermissionSnapshot`) stores Auth's
  security permission projection. **Deliberately a new type, not a reuse of
  `UserPermissionSnapshot`/`UserRole`/`PermissionCollection`** - those are User's own, independent,
  already-documented-as-unrelated business-segmentation concept (see
  [user-service.md](user-service.md#denormalized-roles): "the two happen to share the word 'Roles'
  but nothing else"), confirmed unwired and untouched by this phase. New
  `AccountEffectivePermissionsChangedConsumer` (`User.Infrastructure/Messaging/Consumers/`)
  deserializes the integration event and dispatches an internal `OnAccountEffectivePermissionsChangedEvent`
  (mirroring every other User consumer's shape, e.g. `UserAccountDeletionIntegrationEventConsumer`)
  → `IUserWriteService.RebuildAuthorizationSnapshotAsync` stores the array verbatim; User never
  recomputes or queries Auth's Role/RolePermission graph itself.
  `GetUserDetailResponse.Permissions` (new field, direct uncached DB read via
  `IUserReadService.GetEffectivePermissionsAsync`) - a client-side UI-behavior signal only, never
  the server-side authorization boundary, same caveat as `Roles`.
- **Migration**: Auth has none this phase (seeding only, no schema change - confirmed via
  `dotnet ef migrations has-pending-model-changes`). User:
  `20260809085343_AddUserAuthorizationSnapshot` (new table, additive only). Neither applied to any
  database.
- **Deferred / explicitly out of scope**: Position management API (audited - existing Domain shape
  is already sufficient for the resolver's needs, no gap found, no new endpoints built), direct
  `AccountRole` assignment propagating this event, `RoleUpdated`/`RoleDeleted`-triggered propagation
  (only `RolePermission` changes propagate), any caching layer for
  `IUserReadService.GetEffectivePermissionsAsync` (kept as a plain DB read, matching how `Roles`
  worked before `IRoleCacheReader` existed). Root Tenant Management / Bootstrap / Configuration /
  Dictionary APIs were deferred at the time this note was written - see "Tenant Management &
  Bootstrap APIs (Phase 5)" below, they are now implemented.
- **Found, not fixed (pre-existing, unrelated)**: `UserWriteService`'s constructor injects
  `IRepository<UserEntity, Guid>`, but only `IRepository<TEntity>` (no `TId` overload) is ever
  Scrutor-registered anywhere in this codebase (`AddScopedByInterface(typeof(IRepository<>), ...)`
  only matches the single-generic interface) - this may be an unresolvable DI dependency on
  existing, unrelated methods (`UpdateProfileDetailsAsync`/`DeleteAsync`). This phase's own new
  `RebuildAuthorizationSnapshotAsync` reuses the same injected `repo` field for consistency (no
  worse off than the rest of the class) but was not the place to investigate or fix this further.

## Persistence Service pattern + automatic DI (Phase 4)

Introduced 2026-08-09. Two narrow objectives only: classify the authentication flow's genuine
persistence concerns explicitly, and eliminate manual DI registration for them going forward. Not
a new architecture, not a token-flow redesign, not a mass refactor of every existing service.

- **`IPersistenceService`** (new, `BuildingBlock.Persistence/IPersistenceService.cs`) - a
  no-member marker, placed alongside `IRepository<T>` (`BuildingBlock.Persistence/Repository/`)
  since it's the same kind of reusable, cross-service classification concept. A class implements it
  *in addition to* its own Application-facing interface
  (`sealed class RoleReadService : IRoleReadService, IPersistenceService`) purely to become eligible
  for automatic discovery - it carries no behavior of its own.
- **No new scanner was written.** `AddScopedByInterface(Type interfaceType, params Type[]
  assembliesToScan)` (`BuildingBlock.Application/Extensions/ServiceScanningExtensions.cs`) already
  existed as a fully generic Scrutor helper - `IRepository<>` was just its first caller. Auth's
  `AddPersistenceServices` (new, `Auth.Persistence/DependencyInjection.cs`) calls the exact same
  method with `typeof(IPersistenceService)` instead - one line, same `Scoped` lifetime `IRepository<>`
  already uses, same `AsImplementedInterfaces()` behavior (so resolving `IRoleReadService` still
  works - the scan registers the concrete class against every interface it implements, not just the
  marker). Any other service can adopt the identical one-line call against its own DbContext
  assembly; nothing AuthService-specific needed duplicating.
- **Marked (`IPersistenceService`), now auto-registered**: `AccountReadService`,
  `RefreshTokenReadService`, `TenantClientReadService`, `EffectivePermissionReadService`,
  `RoleReadService`, `RoleWriteService`, `PermissionReadService`, `PermissionWriteService` - each
  represents a genuine, non-trivial data-access concern (custom filtered/joined queries, or a
  Write Service covering an aggregate's full Create/Update/Delete lifecycle), confirmed by reading
  every method body before marking it, not by name pattern.
- **Deliberately NOT marked**: `AccountWriteService.DeleteIfExistAsync`,
  `RefreshTokenWriteService.AddAsync`/`UpdateAsync`, `TenantClientWriteService.CreateAsync` - each
  is a single-method, single-Repository-call decorator with no added responsibility (e.g.
  `AccountWriteService.DeleteIfExistAsync` is a bare `return repo.DeleteIfExistAsync(id, ct);`) -
  exactly the anti-pattern this phase's own spec calls out by name. They stay manually registered
  in `AddRepositories`, unchanged.
- **`ITenantReadService`/`ITenantWriteService`/`IScopeReadService`/`IScopeWriteService` were left
  untouched** - pre-existing, unrelated to the authentication flow this phase scoped itself to (see
  "do not fix every existing Persistence Service" in the phase's own spec). They remain manually
  registered.
- **`authService.GetUserRolesAsync` stays on `IAuthService`, not moved.** Audited per the phase's
  own explicit example: `IAuthService` is a broad, cohesive `UserManager<Account>`-wrapping
  Identity abstraction (`GetUserByIdAsync`, `ValidateCredentialsAsync`, `CreateUserAsync`,
  `UpdatePasswordAsync`, `ConfirmEmailAsync`, `IsInRoleAsync`, `AssignRoleAsync`,
  `DeleteUserAsync`, ...) that already owns this operation correctly - pulling just this one method
  into a Persistence Service would fragment a working abstraction for no clear benefit, which the
  phase's own spec explicitly permits declining to do.
- **No `AuthenticationTokenService` or equivalent was created.** `LoginHandler`/
  `RefreshTokenHandler`/`RegisterHandler` are unchanged in this phase - they already called
  `IEffectivePermissionReadService`/`IAccountReadService`/`ITenantClientReadService` directly
  (built across Phases 1-3), which *is* the Login flow correctly calling Persistence Services; there
  was no leftover raw `DbContext`/inline query to extract, and no merge-multiple-calls-into-one
  service was introduced.
- **No token/permission/refresh-token/tenant-context behavior changed.** This phase touched zero
  business logic - only which interface(s) five pre-existing classes implement and how they reach
  the DI container.

## Tenant Management & Bootstrap APIs (Phase 5)

Introduced 2026-08-13. Fills the gap the earlier phases explicitly deferred: full CRUD/search over
`Tenant` (list/detail/create/update/disable/delete), translation/config/dictionary editing, client
rotation, a pre-authentication bootstrap endpoint, and the backend-only foundation for a future
Notification Hub version-check flow. `Tenant`'s domain surface itself did not change beyond adding
`ISoftDeleteEntity` and a `Delete()` method - every other operation composes the `Create`/`Rename`/
`UpdateBranding`/`UpdateMetadata`/`Activate`/`Deactivate`/`SetLocale`/`RemoveLocale`/
`IncrementVersion` methods that already existed from Phase 1.

### Tenant Management APIs

| API | Route | Permission | Notes |
|---|---|---|---|
| List Tenants | `GET /tenants?search=&page=&pageSize=` | `tenant:view` | DB-level `ILIKE` search on Code/Name (`TenantReadService.SearchAsync`), `PaginatedResult<TenantSummaryResponse>` - same abstraction Promotion/Notification's paginated lists use. Lightweight by design: `Id`/`Code`/`Name`/`LogoUrl`/`IsActive` only. |
| Get Tenant | `GET /tenants/{id}` | `tenant:view` | `TenantDetailResponse` - comprehensive, never used for listing. Returns raw per-locale `Configuration`/`Dictionary` (for editing, including the fallback row) plus a separately-computed `Translations` merged-effective view per supported non-default language (see "Merged Tenant Translations" below), and non-secret client identity (`PublicKey` is safe to expose - see `TenantClient`'s class doc comment, there is no secret to redact). |
| Create Tenant | `POST /tenants` | `tenant:manage` | `{ code, name, logoUrl?, faviconUrl? }`. Code uniqueness checked in the handler (`ExistsByCodeAsync` → `ConflictException`); format validated by `TenantCode.Create` (domain). |
| Update Tenant | `PUT /tenants/{id}` | `tenant:manage` | `{ name, logoUrl?, faviconUrl? }`. Bumps `Version` and enqueues `TenantVersionChangedIntegrationEvent` atomically (see "Versioning" below) - name/branding are bootstrap-relevant. |
| Disable Tenant | `POST /tenants/{id}/disable` | `tenant:manage` | `Tenant.Deactivate()` - idempotent no-op if already disabled. Does not bump `Version`: a disabled tenant's bootstrap is rejected outright (`ConflictException`), not served with different content. |
| Delete Tenant | `DELETE /tenants/{id}` | `tenant:manage` | Soft delete only (`Tenant.Delete()`, `ISoftDeleteEntity` - first non-`User` adopter). Drops out of every normal query via the global `!IsDeleted` filter; a repeat delete surfaces as `NotFoundException`, same as any other operation against an already-deleted tenant. |
| Upsert Translation | `PUT /tenants/{id}/translations` | `tenant:manage` | `{ language, key, value }` - merges one key into that language's `DictionaryJson`, every other key preserved (`JsonMergeHelper`). Bumps `Version`. |
| Update Dictionary | `PUT /tenants/{id}/dictionary/{language}` | `tenant:manage` | Bulk payload merged onto the stored dictionary for one language - unspecified keys preserved, other languages' rows untouched (each is a separate `TenantLocale` row). Bumps `Version`. |
| Update Config | `PUT /tenants/{id}/config?language=` | `tenant:manage` | Merged onto one locale's `ConfigurationJson`; omit `language` to target the tenant-wide fallback/default resource. Bumps `Version`. |
| Rotate Client | `POST /tenants/{id}/client/rotate` | `tenant:rotate-client` (separate from `tenant:manage` - a credential-affecting operation) | Revokes every currently-`Active` `TenantClient` for the tenant (`RevocationReason.Superseded`) and issues a new one, atomically. Returns only the new `PublicKey` - a previously stored key is never returned again. Does **not** bump `Version` - rotation changes which client key resolves to this tenant, not the bootstrap content served once resolved. |

### Merged Tenant Translations

`Tenant` has no "default language" field - the existing null-`LanguageCode` fallback row (see
"Tenant foundation" above) already plays that role. For each language in
`LanguageCodeConstant.SupportedLanguages` (`en`, `vi`), `TenantTranslationMerger.BuildEffective`
(`Auth.Application/Common/`) computes `effective(language) = fallback merged with that language's
override, override wins key by key` for both `ConfigurationJson` and `DictionaryJson`
(`JsonMergeHelper` - recursive `JsonObject` merge). The fallback resource itself is **never**
returned as an entry in the merged collection, since it has no language code to key it by - this
is structural, not a filter that could be bypassed. Shared between `GetTenantQuery` (detail) and
`GetTenantBootstrapQuery` (bootstrap) so both return identical merge semantics.

### Tenant Bootstrap API

`GET /bootstrap` is deliberately **not** `GET /tenants/{id}/bootstrap` behind Root authorization -
it is pre-authentication, identified by the `X-Tenant-Client-Key` header, the exact same mechanism
`Login` already uses to resolve a Tenant before credentials are checked (`ITenantClientReadService.
GetByPublicKeyAsync` + `TenantClient.IsUsable()`). This is the tenant's own frontend application
calling before a user has logged in, to render its initial shell - not a Root Portal concern.
`AllowAnonymous()`, same generic-failure shape as `Login` (unknown/invalid/revoked/expired key all
surface as one `UnauthorizedException`, so a caller can't enumerate valid keys). The Root client is
rejected (`BadRequestException` - no tenant to bootstrap); a disabled tenant is rejected
(`ConflictException`). Response (`TenantBootstrapResponse`): `Version`, `Tenant` (`Id`/`Code`/`Name`/
`LogoUrl`/`FaviconUrl`), `SupportedLanguages`, and `Translations` (the same merged-effective view
Detail returns). Deliberately lightweight - never the full editing payload.

> **TODO** (left in `GetTenantBootstrapHandler`): confirm whether the Root Portal (`nova-console`)
> needs its own bootstrap contract, or genuinely never calls this endpoint. Not decidable from the
> existing domain model - `TenantClient.IsRootClient` exists, but no consumer of a "Root bootstrap"
> has been identified yet.

### Versioning

`Tenant.Version`/`Tenant.IncrementVersion()` already existed from Phase 1 but were never called
anywhere - this phase is what wires them up. Bumped by: Update Tenant, Upsert Translation, Update
Dictionary, Update Config. **Not** bumped by: Create (starts at `1`), Disable, Delete, Rotate
Client - each of those either sets an initial value or changes something other than served
bootstrap content (see each API's row above for the specific reasoning).

```text
Tenant update / translation / dictionary / config change
        v
tenant.IncrementVersion()  (inside the same IUnitOfWork.ExecuteTransactionAsync as the write)
        v
outboxStore.EnqueueAsync(TenantVersionChangedIntegrationEvent)   <- same transaction, atomic
        v
Outbox relay -> Kafka topic "tenantversionchangedintegrationevent"
        v
Notification.Infrastructure's NotificationTriggerConsumer (existing fan-in consumer, extended
with a new topic/case - not a new consumer type, see its own class doc comment for why a
Hub-dependent type can't be constructor-injected directly into a Kafka consumer)
        v
NotifyTenantVersionChangedCommand -> IRealtimeNotifier.PushTenantBootstrapVersionChangedAsync
        v
ActorHubFacade.Tenant(tenantId).BootstrapVersionChanged(version)  -> every connection in
GlobalHub's "tenant:{tenantId}" group (joined in OnConnectedAsync from the access token's
AppClaimTypes.TenantId claim)
```

**Implemented now**: the full chain above, end to end, including the SignalR push. Also: a
read-through Redis cache (`Auth.Infrastructure.Caching.TenantVersionCache` - cache miss falls back
to `TenantReadService.GetVersionAsync`, a lean Version/IsActive-only projection) and a
`GetTenantVersion` gRPC RPC (`auth.proto`/`AuthGrpcServiceImpl`) as the fast-path/source-of-truth
pair a future Hub *connection* handler is expected to read from.

**Deliberately deferred** (see the task's own "Important Scope Boundary" - no client-side refresh
orchestration yet): a Hub connection handler that actually performs the version-check-on-connect
comparison; anything that reads `TenantVersionCache`/calls the gRPC RPC at runtime (both exist,
neither is called yet); Notification writing to Redis itself (`AddRedisCache` throws without a
configured connection string, and Notification has never depended on Redis before - wiring it in
just for this would turn an optional piece of infrastructure into a mandatory one for the whole
service, so `NotifyTenantVersionChangedHandler` only pushes over SignalR); any client-side
auto-reload, state reconciliation, or bootstrap re-fetch orchestration in `nova-console`.

### Known frontend/backend contract gap

`nova-console`'s Tenant feature UI, hooks, Zod schemas, and DTOs were already built ahead of this
phase, deliberately isolated behind an in-memory dev adapter (`services/tenant/tenant.dev-adapter.ts`,
explicit comment: "MUST be replaced with real service calls once the backend exposes Tenant CRUD
endpoints"). Two contract mismatches to resolve when wiring the real endpoints up:
- `GET /tenants` now returns `PaginatedResult<TenantSummaryResponse>` (`{ items, pageNumber,
  pageSize, totalCount, ... }`), not a bare `TenantSummaryDto[]` - `listTenants()` needs updating.
- The frontend's Zod `code` pattern (`/^[a-z0-9][a-z0-9-]*[a-z0-9]$/`, kebab-case) does not match
  the domain's actual `TenantCode` format (`^[a-z][a-z0-9]*(_[a-z0-9]+)*$`, snake_case) - the
  domain, not the frontend's pre-existing assumption, is the source of truth.

## Provider-based Roles & PermissionGrant foundation (Phase 6)

Introduced 2026-08-29. Replaces the Role-only `RolePermission` join with a centralized,
provider-generic `PermissionGrant` model, and makes `Role` provider-aware - the foundation for a
future Client/Guest/direct-User grant path to reuse the exact same tables instead of a new
`*_permissions`/`*Role` table per principal type. `Position → PositionRole → Role` is unchanged;
Position still does not receive direct permission grants (audited this phase - see below).

- **`PermissionProviderName`** (new, `BuildingBlock.SharedKernel/Authorization/`) - `[Flags] enum
  { Role, User, Client, Guest, ServiceAccount }`, with `ToName()`/`ParseName()` giving the stable
  persisted string every DB column and `[PermissionDefinition]` attribute actually uses - enum
  numeric values are never a database contract.
- **`PermissionDefinitionAttribute`** (new, same folder) - decorates every `Permissions.cs` const
  with `Providers = ...`, declaring which provider categories may hold a grant for that key. All
  default to `Role` only this phase (the only wired grant path) - widening a single permission's
  `Providers` later, when a direct User/Client/Guest grant is actually implemented, needs no schema
  change.
- **`PermissionRegistry`** (new, same folder) - reflects `Permissions.cs` once into an immutable
  `FrozenDictionary`; `PermissionRegistry.Instance` is a lazy static singleton (usable from
  `Auth.Domain` with no DI - `PermissionKey.Create` now validates against it instead of the
  hand-maintained `Permissions.SupportedValues` `FrozenSet`, which is deleted) and is also
  registered as a DI singleton in `Auth.Persistence/DependencyInjection.cs` (not
  `Auth.Application`, because `Auth.DbMigrator` calls `AddPersistence` but not `AddApplication`,
  and `PermissionGrantService` - the DI consumer - lives in `Auth.Persistence`). `Get`/`GetAll`/
  `Contains`/`GetAllowedProviders`/`IsProviderAllowed` are all pure in-memory lookups, never SQL.
- **`Role.ProviderName`/`Role.ProviderKey`** (new columns) - `ProviderName` classifies which
  principal-category catalog a Role belongs to (every Role assignable to an Account today is
  `ProviderName == User`) - it is **not** a per-instance owner; Role remains the single, global,
  reusable catalog it always was (see "Global vs tenant-scoped" below), just now filterable by
  provider (`Role.ProviderName == Client` is how a future Client-role catalog gets queried, with no
  new `ClientRole` table). `Role.Create` rejects `ProviderName == Role`/`None`/combined flags -
  a Role is a catalog entry, not itself a grantable principal category. `ProviderKey` is a reserved,
  currently-unused narrower-scoping hook (nullable, always `null` today). `Role.Permissions`
  (`ICollection<RolePermission>`) and `Role.AssignPermission`/`RemovePermission` are gone - Role no
  longer owns a permission-grant collection, since a generic `PermissionGrant` can't carry a real FK
  back to one specific provider type.
- **`PermissionGrant`** (new, `Auth.Domain/Entities/Permissions/`, replaces `RolePermission`/
  `role_permissions`) - `PermissionDefinitionId` + `ProviderName` + `ProviderKey` (string - has to
  hold non-Guid keys too, e.g. a future Guest `"*"`) + `TenantId` (`ITenantEntity`, tenant-scoped
  exactly like `RolePermission` was). Deliberately has no navigation to `Role` or any other
  provider-specific type. Today the only wired path is `ProviderName = Role, ProviderKey =
  <Role.Id>.ToString()`. Table `permission_grants`: unique index
  `(tenant_id, permission_definition_id, provider_name, provider_key)`, secondary index
  `(tenant_id, provider_name, provider_key)` for "every grant for this provider instance" lookups.
- **`PermissionGrantService`** (new, `Auth.Persistence/Contexts/Permissions/Write/`,
  `IPersistenceService`) - `GrantAsync`/`RevokeAsync`/`ReplaceForProviderAsync`, the generic
  replacement for `Role.AssignPermission`/`RemovePermission`. **Every write validates the requested
  key against `PermissionRegistry.Instance.IsProviderAllowed` before persisting, throwing
  `InvalidArgumentException` otherwise** - the server-side security boundary: a client cannot bypass
  UI/attribute-based filtering by posting an unsupported provider directly (e.g. a `Guest` grant on
  a `Role`-only permission is rejected even if the permission key itself exists).
  `RoleWriteService.UpdatePermissionsAsync` now calls `ReplaceForProviderAsync(Role, roleId, ...)`
  instead of mutating `role.Permissions` - it also gained a required `tenantId` parameter (grants
  are tenant-scoped, Role itself is not), threaded from `RequestContext.Current.TenantId` in
  `UpdateRolePermissionsHandler` exactly like Login already does. `RoleReadService` gained
  `GetPermissionKeysAsync(roleId)` (a normal, ambient-tenant-filtered `PermissionGrants` query) for
  `GetRoleHandler`, which no longer has `role.Permissions` to read from directly.
- **`EffectivePermissionReadService`** rewritten to join through `PermissionGrant` instead of
  `RolePermission`. Since `PermissionGrant.ProviderKey` is a generic string (not a typed FK), the
  Role→grant join happens against **materialized** `roleId.ToString()` keys, not SQL-side
  `Guid`-to-text translation - each method now does two round trips (resolve role ids, then resolve
  grants) instead of one combined query, trading a small amount of query-plan efficiency for
  translation certainty.
- **`PermissionDefinition.Status`** (new, `PermissionDefinitionStatus { Active, Deprecated,
  Disabled }`, `smallint`, default `Active`) - definition lifecycle only, via new
  `Activate()`/`Deprecate()`/`Disable()`. Deliberately does not cascade to or affect any existing
  `PermissionGrant` - a deprecated/disabled definition's grants are untouched; this is a
  maintenance/discoverability signal, not a revocation mechanism.
- **`PermissionCatalogSeeder`** rewritten to be registry-driven: source of truth is
  `PermissionRegistry.Instance.GetAll()`, not a hand-maintained `Catalog` tuple array. Runs on every
  `DatabaseSeeder.SeedAsync()` call (not just an empty DB) and is per-key idempotent (diffs
  registry keys against existing `PermissionDefinition.Key` rows, only inserting what's missing) -
  a newly-added `[PermissionDefinition]` const gets its DB row created automatically on the next
  deploy, no manual seed edit required, and existing rows' DB-owned metadata (translations, status)
  is never touched. Group-code assignment mirrors the `"module:action"` key convention, with one
  preserved exception: `Root`/`User` group as `"platform"` (not `"system"`) despite the `system:`
  key prefix, matching the prior hardcoded catalog exactly (no grouping regression).
- **`RolePermissionSeeder` → `RoleGrantSeeder`** (renamed, rewritten) - grants the seeded
  Root/Admin/User system Roles their permissions as `PermissionGrant` rows
  (`ProviderName = Role, ProviderKey = role.Id`) instead of `role.AssignPermission(...)`. Same
  mapping, same idempotency check (now `PermissionGrants.AnyAsync()`), same
  `TenantId == Guid.Empty` seeding scope.
- **Global vs tenant-scoped - unchanged from Phase 3's decision.** `Role`/`PermissionDefinition`/
  `PermissionGroup` still have no `TenantId` - one shared, platform-wide catalog.
  `PermissionGrant`/`PositionRole`/`AccountPosition` are the tenant-scoped layer, exactly as
  `RolePermission` was before it. Nothing about this phase changes that split.
- **Position - audited, intentionally unchanged.** `Position → PositionRole → Role` is untouched;
  Position still does not receive direct `PermissionGrant`s (`ProviderName` has no `Position`
  value). **Position has no hierarchy today** - no `ParentPositionId`, no recursion, no CRUD API
  (confirmed by re-reading `Position.cs`/`AccountPosition.cs` - flat, single-level - and this
  doc's own Phase 3 note: "Position management API... audited, no gap found, no new endpoints
  built"). A recursive Position hierarchy with a delegation-containment constraint ("a parent can
  only delegate authorization it itself possesses") is a real future authorization milestone, not
  something this phase preserved or built - noted here so it isn't rediscovered as a surprise.
- **Migration**: full history reset (all 4 prior migrations + snapshot deleted, one fresh
  `InitialCreate` regenerated from the new model) - same approach as the 2026-08-05 reset, since the
  app is still pre-production. `role_permissions` is gone; `permission_grants` exists with its two
  indexes; `roles` carries `provider_name`/`provider_key`; `permission_definitions` carries
  `status`. Verified end-to-end against a real local Postgres: dropped the stale pre-refactor
  `auth_db` (no migration-history table, so `dotnet ef database update` couldn't tell it wasn't
  empty), ran `Auth.DbMigrator` against the now-truly-empty database (migration + full seed chain
  succeeded), then ran it a second time to confirm every seeder's idempotency check holds
  (`"No pending migrations"`, every seeder logged success with zero duplicate inserts).
- **Tests**: new `tests/unit/Auth.Domain.Tests` project (`RoleTests`, `PermissionGrantTests` -
  `Create` validation, provider-flag rejection). `BuildingBlock.SharedKernel.Tests` gained
  `Authorization/PermissionProviderNameTests` and `Authorization/PermissionRegistryTests`
  (`ToName`/`ParseName` round-trip, `Discover` attribute-reflection behavior including duplicate-key
  detection via a private fixture catalog, and `PermissionRegistry.Instance` sanity checks against
  the real `Permissions.cs`). `Auth.Application.Tests` gained
  `UpdateRolePermissionsHandlerTests` (tenantId threading, outbox-enqueue-only-when-changed,
  skip-when-no-accounts-affected). `PermissionGrantService`/`EffectivePermissionReadService`
  themselves are not unit-tested in isolation - no existing pattern in this repo tests an
  `AuthDbContext`-backed Persistence Service without a real database, and building one was judged
  out of scope for this foundation milestone; their correctness is instead covered by the live
  DbMigrator verification above (which exercises `RoleGrantSeeder` → `PermissionGrant` creation end
  to end) plus the interface-level handler test.
- **Deliberately deferred / out of scope**: Client/Guest/ServiceAccount provider workflows and UI,
  any Position management API, Position hierarchy/delegation-containment (see above), `AccountRole`
  merged into `PermissionGrant` (kept as-is - it's the existing direct User↔Role join, unrelated to
  the grant-centralization problem this phase solves), and the still-unwired `AccountPermission`
  cache (`Account.RefreshPermissionSnapshot` remains dead code, unchanged by this phase).
- **Incidental fix, unrelated to this phase's scope**: `Auth.Persistence/Engine/
  AuthDbContextFactory.cs`'s hardcoded design-time connection password (`postgres`) no longer
  matched the actual local Postgres password (`local-dev-postgres-password`, per
  `Auth.DbMigrator/appsettings.json`) - `dotnet ef database drop`/`migrations add` failed
  authentication until corrected. Fixed as part of this phase's own empty-database verification
  requirement, not a deliberate scope addition.

## Known state

- Mapster is registered but unused — hand-mapping is the actual convention (see [04-coding-rules.md](../04-coding-rules.md#mapping)).
- `Register` → identity persistence goes through ASP.NET Identity's `UserManager` (auto-saves); it does **not** call `IUnitOfWork.SaveChangesAsync` explicitly. The `ExecuteTransactionAsync` pattern (see [04-coding-rules.md#transaction-management](../04-coding-rules.md#transaction-management)) is demonstrated in `RefreshTokenSyncService.SyncCacheToDbAsync`, not in Register/Login.
- gRPC to User Service is config-gated (`Grpc:UserService:Url`, with a stub fallback historically used during local dev without User Service running) — check current `Auth.Infrastructure/GrpcClients/DependencyInjection.cs` wiring before assuming it's always live.
- **Migration history was reset 2026-08-05.** While adding the Tenant Foundation migration, roughly 18 tables (`positions`, `permission_groups`/`permission_definitions` + translations, `invitations`, `sessions`, `devices`, `mfa_methods`/`mfa_backup_codes`, `token_blacklists`, `account_positions`/`account_permissions`, `position_roles`/`position_translations`, `role_permissions`, and others) turned out to exist in `AuthDbContext`/Domain with zero corresponding migration ever committed - `AuthDbContextModelSnapshot.cs` had drifted far ahead of the actual migration file history. Rather than bundle that unrelated backlog into a "clean" Tenant Foundation migration, the entire migration history was collapsed into a single fresh `InitialCreate` (2026-08-05) that captures the complete current model (all pre-existing tables + Tenant/Scope) as a new baseline. Any environment with a real database from before this date needs to be rebuilt from this new baseline rather than upgraded in place.
- A design-time factory (`Auth.Persistence/Engine/AuthDbContextFactory.cs`, `IDesignTimeDbContextFactory<AuthDbContext>`) is required for `dotnet ef migrations` tooling - building `AuthDbContext` through the full application host fails at design time because Identity 10.0's `AddEntityFrameworkStores<AuthDbContext>` registration resolves a `UserStore` generic signature the design-time host's DI container can't satisfy. The factory mirrors `AddPersistenceDbContext`'s options (including `UseSnakeCaseNamingConvention()`) so scaffolded migrations match the runtime model; this doesn't affect runtime resolution (`Auth.API` via `AddPersistence`).
