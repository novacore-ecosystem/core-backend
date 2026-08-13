# NovaCore Local Development Environment

Minimal Docker Compose stack for `nova-console` frontend development. Runs only what
`nova-console` actually needs: the **Gateway**, **Auth**, **User**, **Audit**, and
**Notification** services, plus a lightweight local **Mongo** container.

Not started here: Product/Inventory/Order/Payment/Promotion (out of scope for `nova-console`
right now), and Kafka/Elasticsearch/Seq/Kibana/APM (see [Why some infra is missing](#why-some-infrastructure-is-intentionally-missing) below).

## Architecture

```text
nova-console (:3000)
     |
     v
 Gateway (:5000)  <-- only host-published, browser-facing entrypoint
     |
     +--> auth-api          --> global Postgres (auth_db, auth_hangfire_db) + global Redis
     +--> user-api           --> global Postgres (user_db, user_hangfire_db) + global Redis
     +--> audit-api          --> global Postgres (audit_hangfire_db) + local Mongo (audit_logs)
     +--> notification-api   --> global Postgres (notification_hangfire_db) + local Mongo (notification_db)
                                  + SignalR hub at /hubs/global

Global PostgreSQL (already running on your machine, NOT started by this compose)
Global Redis      (already running on your machine, NOT started by this compose)
Local Mongo       (started by this compose - the one piece of infra it owns)
```

auth-api and user-api also talk to each other directly over gRPC (port 5002 inside the
compose network) for account/profile lookups; audit-api calls user-api's gRPC for actor
display-name enrichment (fails open if user-api is down).

## Prerequisites

- Docker Desktop (or another Docker Engine) running.
- A PostgreSQL instance already running and reachable from containers - e.g. a long-lived
  `pg` container you use across projects. Default assumed reachable at
  `host.docker.internal:5432`.
- A Redis instance already running and reachable the same way. Default
  `host.docker.internal:6379`.
- If your Postgres/Redis are themselves Docker containers, `host.docker.internal` reaches
  whatever they publish on the **host**, not their container DNS name - make sure their
  ports are actually published to the host (`-p 5432:5432` / `-p 6379:6379`).

If your global Postgres/Redis live on a shared Docker network instead of published host
ports, see [Alternative: shared Docker network](#alternative-shared-docker-network-instead-of-hostdockerinternal).

## Setup

```bash
cd deploy/local
cp .env.example .env
# fill in POSTGRES_*, REDIS_*, MONGO_*, JWT_*, and the *_PUBLIC_HTTP_PORT / GATEWAY_PUBLIC_PORT values
```

`JWT_SECRET_KEY`/`JWT_ISSUER`/`JWT_AUDIENCE` must be non-empty - Auth issues tokens with
these, and Gateway/User/Audit/Notification only validate them, so all five must agree. Any
local value works (min ~32 chars for the secret); it never needs to match production.

## Start

```bash
docker compose up -d
```

First run: `db-init` connects to your global Postgres and creates `auth_db`, `user_db`,
`audit_hangfire_db`, `notification_hangfire_db`, `auth_hangfire_db`, `user_hangfire_db` if
they don't already exist (safe to re-run), then exits. `auth-api`/`user-api` run their own
EF Core migrations on startup automatically - no separate migration step. Mongo bootstraps
via the root project's own `scripts/mongodb/init-mongo.js`, mounted read-only.

## Stop

```bash
docker compose down          # stop, keep the local Mongo volume
docker compose down -v       # stop, also wipe the local Mongo volume
```

This never touches your global Postgres/Redis - they're external to this compose file.

## Logs

```bash
docker compose logs -f auth-api
docker compose logs -f            # everything
```

## Rebuild after code changes

```bash
docker compose up -d --build auth-api    # rebuild + restart one service
docker compose up -d --build             # rebuild everything
```

## Start an individual service

Real dependencies only - Gateway doesn't wait on any of the four services (its own routes
just 502 until they're up), and Audit/Notification don't depend on Auth/User at all:

```bash
docker compose up -d auth-api
docker compose up -d user-api
docker compose up -d audit-api
docker compose up -d notification-api
docker compose up -d gateway
```

## Database setup

One global PostgreSQL instance, six databases inside it (`db-init`'s job, see
[init-db.sql](init-db.sql)):

| Database | Used by |
|---|---|
| `auth_db` | auth-api (domain data) |
| `auth_hangfire_db` | auth-api (Hangfire storage) |
| `user_db` | user-api (domain data) |
| `user_hangfire_db` | user-api (Hangfire storage) |
| `audit_hangfire_db` | audit-api (Hangfire storage only - domain data is Mongo) |
| `notification_hangfire_db` | notification-api (Hangfire storage only - domain data is Mongo) |

Hangfire needs Postgres even for the two Mongo-backed services - it's the shared
background-job storage across every service in this codebase, independent of each
service's primary datastore.

## Redis setup

One global Redis instance, shared with no per-service database index (matches the root
project's own convention) - namespaced only by `Cache:KeyPrefix` (`auth:`, `user:`).
Required by **Gateway** (refresh-token existence check, connects eagerly at startup) and
**Auth**/**User** (role cache, refresh-token cache - also eager at startup). **Not used at
all** by Audit or Notification.

## nova-console integration

Point `nova-console`'s `.env.local` at:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000/api   # matches nova-console's own default
```

Notification's SignalR hub is reachable through the Gateway at
`http://localhost:5000/hubs/global` (requires the same `AccessToken` cookie as REST calls -
the hub is `[Authorize]`). `nova-console` doesn't have a SignalR client wired up yet as of
this writing.

CORS already allows `http://localhost:3000` (nova-console's default dev origin) at the
Gateway - see [Application code changes](#application-code-changes-made-to-support-this) below.

## Why some infrastructure is intentionally missing

Verified from source (not assumed) that none of these block startup for the four target
services:

- **Kafka**: every service registers a KafkaFlow producer/consumer, but the client library
  connects and retries asynchronously in the background - an unreachable broker logs
  periodic "message timed out" / connection errors and nothing else. No container needed
  for local frontend development, which doesn't exercise cross-service eventing anyway
  since Product/Order/Inventory/Payment/Promotion aren't running.
- **Elasticsearch**: not referenced anywhere in Audit or Notification. In Auth, not
  referenced at all. In User, it's a soft dependency - `EnsureIndexAsync()` is wrapped in a
  try/catch at startup (search endpoints degrade rather than the API failing to boot; a
  startup-ordering bug that broke this was fixed as part of this work, see below). Simply
  omit `Elasticsearch:Url` from a service's environment and the Serilog ES sink and search
  client are skipped entirely - no code path even attempts a connection.
- **Seq/APM**: both are Serilog/OpenTelemetry sinks that batch and retry in the background;
  an unreachable endpoint never blocks startup or request handling.

If a future task genuinely needs one of these locally, add it back as its own container in
this compose file rather than pulling in the root project's full observability stack.

## Application code changes made to support this

All of the following are pre-existing repository bugs, not new local-only behavior -
each one reproduces identically outside Docker (`dotnet run`) and would affect the root
`docker-compose.yml` on any build without a stale image cache. Fixed here because this task
cannot be validated end-to-end without them; each is committed separately from the compose
files themselves.

| File | Change | Why |
|---|---|---|
| `src/Services/{Auth,User,Audit,Notification}.API/Dockerfile`, `YarpApiGateway/Dockerfile` | Copy `Directory.Packages.props` before `dotnet restore` | The solution uses Central Package Management (`ManagePackageVersionsCentrally=true`); without this file present at restore time, every `PackageReference` fails with `NU1015` on a clean build. |
| Same 5 Dockerfiles | `ENTRYPOINT` now targets `NovaCore.<X>.API.dll` / `NovaCore.YarpApiGateway.dll` | Every `.csproj` sets `<AssemblyName>NovaCore.*</AssemblyName>`, but the Dockerfiles referenced the bare project name - the container crashed immediately (`Auth.API.dll does not exist`) on a real build. |
| `BuildingBlock.Web/Cors/CorsExtensions.cs` | `policy.WithMethods(allowOrigins)` -> `policy.WithOrigins(allowOrigins)` | Typo meant the CORS policy allowed zero origins (`WithMethods` was fed a list of URLs, not HTTP verbs) - every cross-origin request was rejected everywhere this policy is used, not just locally. |
| `YarpApiGateway/DependencyInjection.cs`, `Program.cs` | Gateway now registers and applies its own CORS policy (`http://localhost:3000`, `http://localhost:5000`), applied *before* `UseGatewayAuthorization` | The Gateway had no CORS middleware at all. Ordering matters: CORS must run before the custom auth middleware, because a browser's preflight `OPTIONS` request carries no cookies (per the CORS spec) and would otherwise be rejected as unauthenticated before ever reaching the CORS handler. |
| `Auth.Domain/Entities/Accounts/AccountRole.cs` | Parameterless constructor `private` -> `public` | ASP.NET Core Identity's `UserStore` requires `TUserRole : IdentityUserRole<TKey>, new()` with an *accessible* constructor (verified via reflection against the installed package) - a private constructor throws `TypeLoadException` the moment `AddEntityFrameworkStores<AuthDbContext>()` runs, crashing Auth on every startup. |
| `BuildingBlock.Persistence.Ef/DependencyInjection/ServiceCollectionExtensions.cs` | Removed `TryAddScoped(typeof(IRepository<>), typeof(GenericRepository<,>))` and the `IRepository<,>` equivalent | Both mapped an interface to an *abstract* class with a mismatched generic arity - this registration could never have resolved successfully. It's dead code masking real gaps (see next two rows); removing it surfaces the actual, fixable problem instead of a generic-arity crash. |
| `Auth.Persistence/Contexts/RefreshTokens/Repositories/{IRefreshTokenRepository,RefreshTokenRepo}.cs` | Now built on `IRepository<RefreshToken, Guid>` / `EntityGenericRepository<AuthDbContext, RefreshToken, Guid>` instead of the id-less `IRepository<RefreshToken>` / `AuthBaseRepository<RefreshToken>` | `RefreshTokenWriteService` calls the by-id `UpdateAsync(Guid id, ...)` overload, which only exists on the two-type-param repository interface. Nothing provided it. |
| `User.Persistence/Contexts/Users/Repositories/{IUserRepository,UserRepo}.cs`; removed unused `UserBaseRepository.cs` | Same fix, for `UserWriteService` | Identical shape of the same bug in User's persistence layer. |
| `User.API/Program.cs` | Moved `GetRequiredService<IUserSearchIndexer>()` inside the existing try/catch | The Elasticsearch client is built lazily on first resolution; resolving it (not just calling `EnsureIndexAsync()`) is what throws when `Elasticsearch:Url` is unset, and that resolution was happening *outside* the try/catch clearly intended to make this non-fatal. |
| `Auth.Persistence/Contexts/TenantClients/Read/TenantClientReadService.cs` | Query now compares `c.PublicKey == key` instead of `c.PublicKey.Value == publicKey` | `PublicKey` is mapped via `HasConversion` in `TenantClientConfig`, which EF Core can translate for equality on the property itself - but not for an arbitrary member access (`.Value`) inside the expression tree. This broke every login attempt with a 500. |

None of these change the *intended* behavior of any service - each restores behavior the
surrounding code (comments, try/catch blocks, `HasConversion` config) already declared as
the intent, or fixes a build/deploy artifact that was never actually exercised end-to-end.

## Alternative: shared Docker network instead of `host.docker.internal`

If your global Postgres/Redis containers are attached to a known external Docker network:

1. Add to this file's `services:` (each service that needs Postgres/Redis):
   ```yaml
   networks:
     - default
     - global-infra   # your network name
   ```
2. Add at the bottom:
   ```yaml
   networks:
     global-infra:
       external: true
       name: <your-network-name>
   ```
3. Set `POSTGRES_HOST`/`REDIS_HOST` in `.env` to the containers' names instead of
   `host.docker.internal`.

## Troubleshooting

| Symptom | Fix |
|---|---|
| `db-init` fails / exits non-zero | Check `POSTGRES_HOST`/`POSTGRES_PORT`/`POSTGRES_USER`/`POSTGRES_PASSWORD` in `.env`. Run `docker compose logs db-init`. |
| `auth-api`/`user-api` crash-loop with a Postgres connection error | Confirm your global Postgres is actually reachable from a container: `docker run --rm --add-host=host.docker.internal:host-gateway postgres:16-alpine pg_isready -h host.docker.internal -p 5432`. |
| `auth-api`/`user-api`/`gateway` crash-loop with a Redis error | Same idea: confirm Redis is reachable at `REDIS_HOST:REDIS_PORT` from inside a container, not just from your host shell. |
| `database "X" does not exist` | `db-init` didn't run or failed silently - `docker compose up -d db-init` again and check its logs. |
| Port already in use on `docker compose up` | Another process (possibly the root project's own stack) is using that host port. Change the conflicting `*_PUBLIC_HTTP_PORT` / `GATEWAY_PUBLIC_PORT` in `.env`. |
| CORS error in the browser console | Confirm you're calling `http://localhost:5000` (the Gateway), not a service directly - only the Gateway has a CORS policy for `http://localhost:3000`. Confirm nova-console's dev server is actually on port 3000. |
| 401 on every request from nova-console | Check the `AccessToken` cookie is actually being set (`Set-Cookie` on the login response) and that `nova-console`'s HTTP client sends credentials (`withCredentials: true`). |
| SignalR hub connection fails | Same cookie/credentials requirement as above - the hub is `[Authorize]` and reads the JWT from the `AccessToken` cookie, not a bearer header or `access_token` query string. |
| Elasticsearch/tracing errors in logs | Expected and harmless if you haven't set `Logging__Elasticsearch__Url` / haven't started an ES container - these are non-blocking sinks, not requirements. Only User's search *endpoints* (not the service itself) are actually degraded. |
