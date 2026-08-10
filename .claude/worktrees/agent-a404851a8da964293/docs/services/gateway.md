# API Gateway (YarpApiGateway)

**Scope:** Gateway-specific facts. The Gateway is architecturally special-cased — see [02-architecture-rules.md](../02-architecture-rules.md#dependency-direction-must-never-be-violated) for why it doesn't follow the standard 5-layer service pattern.

## Role

Single published entry point (host port `5000`, see [01-architecture-map.md](../01-architecture-map.md#networking)). Reverse-proxies to internal services via YARP, based on `Gateway:Services:{Key}` config (`appsettings.json`), matching by path prefix (e.g. `/api/auth/` → `auth-api:8080`).

## What the Gateway does — and deliberately does not do

The Gateway performs **JWT integrity validation only**: signature match against the configured secret key, expiry, issuer/audience, token format. It does **not** load users, query any database, resolve roles/permissions, or run any business validation — every downstream service performs its own complete authorization independently. This is a deliberate, minimal-responsibility design, not an oversight:

- `AddAuthentication().AddJwtBearer(...)` in `YarpApiGateway/DependencyInjection.cs` — token integrity check.
- **No** `AddAuthorization()`/role-policy registration, and **no** `app.UseAuthorization()` middleware call. `YarpApiGateway/Middleware/AuthorizationMiddleware.cs` (`UseGatewayAuthorization`) only checks `context.User.Identity?.IsAuthenticated`, per-route, driven by `Gateway:Services:{Key}:RequireAuth` — it never inspects roles/claims beyond authentication status.

If you're tempted to add role-based logic to the Gateway, don't — put it in the target service instead, and see [reference/authorization.md](../reference/authorization.md) for how services do it.

## Refresh-token filtering

`YarpApiGateway/Middleware/RefreshTokenFilterMiddleware.cs` intercepts `POST {AuthServicePath}/refresh-token` (the exact path is derived at runtime from `Gateway:Services:Auth:Path` config, not hardcoded) and checks the `RefreshToken` cookie against Redis **before** the request reaches Auth.API — reusing Auth's own design (the raw refresh token is the Redis key; Redis is the source of truth for validity, see `Auth.Infrastructure/Caching/RefreshTokenCacheService.cs` and `docs/reference/caching.md`). If the token doesn't exist in Redis, the Gateway returns 401 immediately without forwarding to Auth.API.

This uses a **minimal, standalone Redis connection** (`BuildingBlock.Web/RefreshTokens/RefreshTokenCacheExtensions.cs` — a raw `IConnectionMultiplexer` + a single `EXISTS` check via the shared key format in `BuildingBlock.SharedKernel.Constants.CacheKeys.RefreshTokens`), not the full `ICacheService`/`CacheOptions` abstraction — see [reference/caching.md](../reference/caching.md#gateway-minimal-lookup-vs-icacheservice).

## Dependencies — deliberately minimal

`YarpApiGateway.csproj` references only `BuildingBlock.SharedKernel` and `BuildingBlock.Web` — **not** `BuildingBlock.Application`/`BuildingBlock.Infrastructure` directly. This is intentional: the Gateway should stay one of the smallest, fastest services in the solution. Before adding a new project reference to the Gateway, check whether the functionality already exists in `BuildingBlock.Web` in a minimal form, or should be added there rather than pulling in a broader shared module. See [decisions/buildingblock-web-extraction.md](../decisions/buildingblock-web-extraction.md).

## Config

- `Gateway:Services:{Key}` — `Url`, `Name`, `Path`, `SwaggerUrl`, `RequireAuth` (per-service routing + auth requirement)
- `Gateway:Jwt` — `SecretKey`/`Issuer`/`Audience` (env: `GATEWAY_JWT_*`)
- `Gateway:Redis:ConnectionString` — shared Redis instance, refresh-token lookup only (env: `GATEWAY_REDIS_CONNECTION_STRING`)

All three are populated purely via `docker-compose.override.yml` env vars / `.env` — `appsettings.json`/`appsettings.Development.json` intentionally carry no secrets (see [setup/environment-config.md](../setup/environment-config.md)).

## Swagger aggregation

`YarpApiGateway/Services/SwaggerAggregator.cs` serves a merged `/swagger` index across all registered services' `SwaggerUrl`.
