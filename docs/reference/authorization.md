# Reference: Authorization

**Scope:** permission-based authorization inside services. Supersedes the earlier role-based design (`AppRole`, `RoleAuthorizationHandler`, `AuthorizationPolicies`), which has been fully retired. For the Gateway's role in this flow, see [services/gateway.md](../services/gateway.md#what-the-gateway-does--and-deliberately-does-not-do) — the Gateway validates token *integrity* only; it does not extract or forward claims on the service's behalf.

## Responsibility matrix

Each claim/permission component has exactly one job. When adding new functionality, use this table to find where it belongs:

| Component | Project | Responsibility |
|---|---|---|
| `AppClaimTypes` | `BuildingBlock.SharedKernel/Constants` | Claim type key constants only — no logic |
| `ClaimsPrincipalExtension` | `BuildingBlock.SharedKernel/Extensions` | Read raw claim values off `ClaimsPrincipal` — no authorization decisions |
| `Permissions` | `BuildingBlock.SharedKernel/Constants/Permissions/` | The permission key catalog (code-first, one file per owning service, per business module) |
| `PermissionRegistry` | `BuildingBlock.SharedKernel/Authorization` | In-memory discovery/index over `Permissions` (flat + grouped) — no DB, Singleton |
| `PermissionExpression` | `BuildingBlock.Application/Abstractions/Authorization` | The composable permission requirement (single key, `All`/`Or`, nested) and its leaf-matching rule — Root bypass, `{module}:full` aggregation. The one place this logic is defined; both `IAuthorizationGuard` and `PermissionAuthorization` evaluate through it |
| `IAuthorizationGuard` / `AuthorizationGuard` | `BuildingBlock.Application/Abstractions/Authorization`, `BuildingBlock.Application/Authorization` | Application-layer (use-case-level) authorization: `RequirePermissionsAsync(...)` evaluates a `PermissionExpression` against `ICurrentUserService` and throws `ForbiddenException` itself |
| `PermissionAuthorization` | `BuildingBlock.Web/Authorization` | Endpoint-level OR-matching over raw permission strings, delegating leaf matching to `PermissionExpression.IsGranted` |
| `PermissionEndpointExtensions` | `BuildingBlock.Web/Authorization` | `RequirePermissions(...)` — endpoint-level authorization declaration |
| `AuthorizationExtensions` | `BuildingBlock.Web/Authorization` | `AddBuildingBlockAuthorization()` — the single DI registration entry point (endpoint policies + `IAuthorizationGuard`) |

`BuildingBlock.SharedKernel` stays transport- and framework-agnostic: it may hold claim-key constants and plain `ClaimsPrincipal` reads (BCL-only, no ASP.NET types), but never authorization *decisions*. Permission *evaluation* itself (the AND/OR expression tree and its leaf-matching rule) lives in `BuildingBlock.Application` — framework-agnostic, and the only layer both Application-layer callers and `BuildingBlock.Web` (which already depends on Application) can share without inverting the dependency direction. ASP.NET-specific authorization infrastructure (policies, endpoint wiring) stays centralized in `BuildingBlock.Web`.

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

`RequirePermissions` is OR-matched: the caller succeeds if they own *any* of the listed permissions — exactly, via `Permissions.Root` (superuser bypass), or via that permission's module aggregate (`"{module}:full"`). The leaf-matching rule lives in `PermissionExpression.IsGranted` and nowhere else; `PermissionAuthorization.HasAnyPermission` just OR-aggregates over it.

## Requiring permissions inside a handler/service (`IAuthorizationGuard`)

Endpoint-level `RequirePermissions(...)` only covers OR-matched, HTTP-route-shaped checks. When a Command/Query handler or any other Application-layer code needs to enforce a permission — including AND/OR combinations no single endpoint policy can express — inject `IAuthorizationGuard` (`BuildingBlock.Application.Abstractions.Authorization`) instead of checking `ICurrentUserService` and throwing by hand:

```csharp
public sealed class UpdateProductHandler(IAuthorizationGuard authorizationGuard, ...) : ICommandHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand command, CancellationToken ct)
    {
        await authorizationGuard.RequirePermissionsAsync(Permissions.Product.Manage, ct);
        // ...
    }
}
```

A bare permission key converts implicitly to a `PermissionExpression`, so the common single- and multi-permission cases need no ceremony:

```csharp
await authorizationGuard.RequirePermissionsAsync(Permissions.Product.Manage);               // single
await authorizationGuard.RequirePermissionsAsync(Permissions.Product.View, Permissions.Product.Manage); // AND (multiple args)
```

`All`/`Or` compose and nest arbitrarily for the uncommon cross-permission cases — import them with `using static NovaCore.BuildingBlock.Application.Abstractions.Authorization.PermissionExpression;` to drop the qualification:

```csharp
await authorizationGuard.RequirePermissionsAsync(
    PermA,
    Or(PermB, PermC, PermD));                 // PermA AND (PermB OR PermC OR PermD)

await authorizationGuard.RequirePermissionsAsync(
    Or(All(PermA, PermB), All(PermA, PermC))); // (PermA AND PermB) OR (PermA AND PermC)
```

`RequirePermissionsAsync` throws `ForbiddenException` itself on failure - callers never write `if (!await guard.HasPermissionsAsync(...)) throw ...`. A non-throwing `HasPermissions(...)` exists for the rare case a caller genuinely needs a boolean instead. Both resolve the current actor's permission set exactly once (via `ICurrentUserService.GetPermissions()`) and evaluate the whole expression tree in memory - no repeated claim/cache lookups per node, regardless of how deeply the expression nests.

## Permission keys (`Permissions`, `BuildingBlock.SharedKernel.Constants`)

Permission keys are code-first — declared in `Permissions`, seeded into Auth's permission catalog, referenced by `RequirePermissions()` — never free-form user input. `Permissions` is one `public static partial class`, physically split across `Constants/Permissions/Permissions.*.cs` purely for Git ownership (unrelated services never touch the same file):

- `Permissions.Common.cs` — genuinely common/system-level permissions only (`Root`, `User`, `Role.*`, `Permission.*`, `System.*`). Default here is one file; don't split further without a strong reason.
- `Permissions.<Service>.cs` — one file per owning service (`Permissions.Product.cs`, `Permissions.Order.cs`, ...). Add a new service's permissions here, even for a single key — never into `Permissions.Common.cs`.

Within a file, each business module is a nested class decorated `[PermissionGroup("code")]` (structural only — no display name/description/translation, that's DB-owned `PermissionGroup` metadata) and typically exposes a `Full` aggregate key that implicitly grants every other permission in that module. A permission group is a `PermissionRegistry` concept, not a file boundary — two groups can live in the same owner file (`Permissions.Inventory.cs` holds both `[PermissionGroup("inventory")]` and `[PermissionGroup("warehouse")]`), and a const doesn't need to sit inside any `[PermissionGroup]` at all (`Root`/`User` are deliberately ungrouped).

Every const also carries `[PermissionDefinition(Providers = ...)]`, declaring which `PermissionProviderName` categories may hold a grant for it (see `PermissionGrant`/`PermissionGrantService` in the Auth service).

`PermissionRegistry.Instance` (`BuildingBlock.SharedKernel/Authorization`) reflects the whole partial class once at startup into an immutable, in-memory index — `Get`/`Contains`/`GetAll`, `GetGroups`/`GetGroup`/`GetPermissions`, `GetAllowedProviders`/`IsProviderAllowed` — never PostgreSQL. It's a lazy static singleton (usable with no DI) and is also registered as a DI Singleton in `Auth.Persistence`. Auth's `DbMigrator` synchronizes `PermissionGroup`/`PermissionDefinition` DB rows from this registry idempotently — adding a permission only requires adding the const (with its attributes) to the owning service's file; no manual DB seed record.

## Reading claims

```csharp
var permissions = user.GetPermissions();                     // ClaimsPrincipalExtension - raw read, SharedKernel
var allowed = user.HasAnyPermission(Permissions.Order.View); // PermissionAuthorization - decision, BB.Web
```

Inside a Command/Query handler (not an endpoint), prefer injecting `ICurrentUserService` (`BuildingBlock.Application.Abstractions.Services`) over threading a `ClaimsPrincipal` through — it's the same identity data, already available via DI, and works uniformly whether the handler runs in a request or elsewhere.

## Important

- Don't re-authenticate credentials at the service level — trust the JWT's claims once signature/expiry validation passes.
- Don't call an external auth service to check permissions — they're in the token.
- New claim type keys go in `AppClaimTypes` (SharedKernel); new permission keys go in `Permissions` (SharedKernel); new leaf-matching/evaluation rules go in `PermissionExpression` (BuildingBlock.Application) so both the endpoint and Application-layer paths stay in sync — never split a single concern across layers for convenience.
- Prefer `IAuthorizationGuard` over endpoint-only checks whenever the rule doesn't map cleanly to "OR of permissions on this route" - AND requirements, cross-permission composition, or checks that belong to a use-case rather than a route.
