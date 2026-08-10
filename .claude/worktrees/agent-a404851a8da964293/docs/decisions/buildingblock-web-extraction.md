# ADR: BuildingBlock.Web Extraction

**Scope:** why `BuildingBlock.Web` exists and the tradeoffs behind its shape. See [03-building-blocks-reference.md](../03-building-blocks-reference.md#web) for what it contains today — this doc is the "why."

## Problem

Before this extraction, API-layer concerns were duplicated per service or misplaced in layers that shouldn't own them:

- `CurrentUserService`/`CurrentUserExtensions` (HttpContext-based, needs `IHttpContextAccessor`) lived in `BuildingBlock.Infrastructure` — an Infrastructure project taking on an ASP.NET Core dependency it shouldn't need.
- `GlobalExceptionHandler` (an `IExceptionHandler`) existed only in `Auth.Infrastructure`; User Service had no working equivalent and silently fell through to a default, unmapped error response.
- `JwtSettings` and the JWT-bearer authentication setup (`AddAuthentication().AddJwtBearer(...)`, including the cookie-based token extraction) were byte-for-byte duplicated between `Auth.Infrastructure` and `User.Infrastructure`.
- Swagger, CORS, Carter registration, and health-check registration were near-identically duplicated in each service's `API/DependencyInjection.cs`.

## Decision

Created `BuildingBlock.Web` as the single home for ASP.NET Core-specific, API-layer building blocks, exposed through one composition entry point: `AddBuildingBlockWeb(configuration, BuildingBlockWebOptions)` / `UseBuildingBlockWeb(options)`. Each piece (current-user, exception handling, JWT bearer auth, Swagger, CORS, Carter, health checks) is also individually callable for services that don't want the full bundle.

`JwtSettings` moved to `BuildingBlock.SharedKernel` (zero-dependency, reachable from both `BuildingBlock.Web` and each service's `*.Infrastructure` token-*issuing* code) — this let the JWT *validation* middleware (API-layer concern, now in Web) and JWT *issuance* (business/infra concern in Auth, unchanged) share one settings type without Infrastructure depending on Web.

`Authorization` (`BuildingBlock.Infrastructure/Authorization/**`) deliberately **stayed** in `BuildingBlock.Infrastructure`, not moved to Web, because the API Gateway consumes it directly and the Gateway was kept out of scope for the Web migration at the time — see [services/gateway.md](../services/gateway.md#dependencies--deliberately-minimal).

## Consequence: a per-service `*.Infrastructure`/`*.API` project may now depend on `BuildingBlock.Web`

This is new and worth calling out because it looks like it could violate [02-architecture-rules.md](../02-architecture-rules.md#dependency-direction-must-never-be-violated) at first glance — `BuildingBlock.Web` is an "outer" layer (ASP.NET Core host concerns), and Infrastructure depending on it looks backwards. In practice, the wiring happens at the **API project's composition root** (`Program.cs`/`DependencyInjection.cs`), not inside `*.Infrastructure` itself — `*.Infrastructure` registers business/data concerns, `*.API` composes `AddInfrastructure(...)` *and* `AddBuildingBlockWeb(...)` side by side. No `*.Infrastructure` project actually references `BuildingBlock.Web`.

## Known tradeoff (see [07-solid-recommendations.md](../07-solid-recommendations.md#dependency-inversion))

`BuildingBlock.Web` depends on `BuildingBlock.Infrastructure` (for `GlobalExceptionHandler` → `ExceptionHandlerHelper`), which means anything referencing `BuildingBlock.Web` transitively pulls in all of Infrastructure's packages. This was accepted as-is rather than restructured further, because the two real consumers (Auth.API, User.API) already needed Infrastructure's full package set anyway. The **API Gateway** is the one consumer that doesn't — it uses only `BuildingBlock.Web`'s minimal `RefreshTokenCacheExtensions` (a standalone Redis lookup that deliberately bypasses `ICacheService`), not the full bundle — see [services/gateway.md](../services/gateway.md#dependencies--deliberately-minimal). If a third minimal consumer appears, revisit splitting `ExceptionHandlerHelper`'s pure mapping logic out of `BuildingBlock.Infrastructure` rather than continuing to accept the transitive weight.

## Related follow-up work in the same session

The Gateway's JWT handling was simultaneously simplified to token-integrity-only validation (no role/permission resolution), and a minimal Redis-backed refresh-token existence check was added to the Gateway, reusing Auth's own key format (`refresh_token_by_string:{token}`) via a shared constant in `BuildingBlock.SharedKernel.Constants.CacheKeys.RefreshTokens`. See [services/gateway.md](../services/gateway.md).
