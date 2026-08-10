# Reference: Authorization

**Scope:** permission-based authorization inside services. Supersedes the earlier role-based design (`AppRole`, `RoleAuthorizationHandler`, `AuthorizationPolicies`), which has been fully retired. For the Gateway's role in this flow, see [services/gateway.md](../services/gateway.md#what-the-gateway-does--and-deliberately-does-not-do) — the Gateway validates token *integrity* only; it does not extract or forward claims on the service's behalf.

## Responsibility matrix

Each claim/permission component has exactly one job. When adding new functionality, use this table to find where it belongs:

| Component | Project | Responsibility |
|---|---|---|
| `AppClaimTypes` | `BuildingBlock.SharedKernel/Constants` | Claim type key constants only — no logic |
| `ClaimsPrincipalExtension` | `BuildingBlock.SharedKernel/Extensions` | Read raw claim values off `ClaimsPrincipal` — no authorization decisions |
| `Permissions` | `BuildingBlock.SharedKernel/Constants` | The permission key catalog (code-first, per business module) |
| `PermissionAuthorization` | `BuildingBlock.Web/Authorization` | Permission evaluation — Root bypass, `{module}:full` aggregation, any future strategy |
| `PermissionEndpointExtensions` | `BuildingBlock.Web/Authorization` | `RequirePermissions(...)` — endpoint-level authorization declaration |
| `AuthorizationExtensions` | `BuildingBlock.Web/Authorization` | `AddBuildingBlockAuthorization()` — the single DI registration entry point |

`BuildingBlock.SharedKernel` stays transport- and framework-agnostic: it may hold claim-key constants and plain `ClaimsPrincipal` reads (BCL-only, no ASP.NET types), but never authorization *decisions* or ASP.NET authorization infrastructure. All permission evaluation and endpoint wiring is centralized in `BuildingBlock.Web`.

## Flow

1. Client sends a request with a JWT (Authorization header or `AccessToken` cookie).
2. **Gateway**: validates signature/expiry/issuer/audience only, checks `RequireAuth` per route, does **not** resolve permissions or attach claims for the service — see [services/gateway.md](../services/gateway.md).
3. **Service**: independently validates the same JWT via its own JWT bearer authentication, populating `HttpContext.User` itself. No database lookup, no call back to Auth Service — the JWT's claims (embedded at issuance by `Auth.Infrastructure/Security/Jwt/JwtTokenGenerator.cs`, one `AppClaimTypes.Permission` claim per permission key) are the sole source of truth at this point.
4. Endpoint code declares required permissions via `.RequirePermissions(...)`; MediatR handlers that need identity data inject `ICurrentUserService` instead of touching `ClaimsPrincipal` directly.

## Registering authorization (per service)

```csharp
// {Service}.API/DependencyInjection.cs, inside AddPresentation
services
    .AddBuildingBlockAuthorization();
```

This is the only call a service needs — policy/handler wiring lives entirely in `BuildingBlock.Web.Authorization`.

## Declaring permissions on an endpoint

```csharp
app.MapGroup("/products")
   .MapProductEndpoints()
   .RequirePermissions(Permissions.Product.Manage);
```

`RequirePermissions` is OR-matched: the caller succeeds if they own *any* of the listed permissions — exactly, via `Permissions.Root` (superuser bypass), or via that permission's module aggregate (`"{module}:full"`). This resolution logic lives in `PermissionAuthorization.HasAnyPermission` and nowhere else.

## Permission keys (`Permissions`, `BuildingBlock.SharedKernel.Constants`)

Permission keys are code-first — declared in `Permissions`, seeded into Auth's permission catalog, referenced by `RequirePermissions()` — never free-form user input. Grouped by business module (`Permissions.Product`, `Permissions.Order`, ...), each module typically exposes a `Full` aggregate key that implicitly grants every other permission in that module.

## Reading claims

```csharp
var permissions = user.GetPermissions();                     // ClaimsPrincipalExtension - raw read, SharedKernel
var allowed = user.HasAnyPermission(Permissions.Order.View); // PermissionAuthorization - decision, BB.Web
```

Inside a Command/Query handler (not an endpoint), prefer injecting `ICurrentUserService` (`BuildingBlock.Application.Abstractions.Services`) over threading a `ClaimsPrincipal` through — it's the same identity data, already available via DI, and works uniformly whether the handler runs in a request or elsewhere.

## Important

- Don't re-authenticate credentials at the service level — trust the JWT's claims once signature/expiry validation passes.
- Don't call an external auth service to check permissions — they're in the token.
- New claim type keys go in `AppClaimTypes` (SharedKernel); new permission keys go in `Permissions` (SharedKernel); new evaluation strategies go in `BuildingBlock.Web/Authorization` — never split a single concern across layers for convenience.
