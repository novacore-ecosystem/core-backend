# Authorization Policy Implementation Summary

## Overview

Created a common authorization policy infrastructure following Clean Architecture principles. API Gateway handles authentication; services read claims to make authorization decisions.

## Files Created

### Core Interfaces & Extensions
- **IAuthorizationPolicy.cs** - Marker interface for policies
- **ClaimsPrincipalExtensions.cs** - Safe methods to read claims (GetUserId, GetRoles, HasRole, etc.)
- **AuthorizationExtensions.cs** - DI setup for policies and handlers
- **AuthorizationConstants.cs** - Policy names (roles live in `BuildingBlock.SharedKernel.Constants.AppRole`)

### Authorization Requirements & Handlers
- **Requirements/RoleRequirement.cs** - Authorization requirements for role checking
- **Handlers/RoleAuthorizationHandler.cs** - Handlers for role-based policies

### Attributes for Endpoints
- **Attributes/AuthorizeAttribute.cs** - Custom authorization attributes for endpoints

### Documentation
- **README.md** - Complete usage guide with examples
- **EXAMPLES.md** - Real-world endpoint examples
- **IMPLEMENTATION_SUMMARY.md** - This file

## How It Works

### 1. API Gateway (YarpApiGateway)
- Validates JWT signature, issuer, audience, expiration
- Extracts claims into ClaimsPrincipal
- Forwards to services with authenticated user context

### 2. Services (User, Auth, etc.)
- Call `AddCommonAuthorizationPolicies()` to register handlers
- Call `AddAuthorization(options => AuthorizationExtensions.ConfigureCommonPolicies(options))`
- Use `RequireAuthorization()` on endpoints to check authentication
- Use `ClaimsPrincipalExtensions` methods to read claims in handlers
- Make authorization decisions based on roles/claims

## Common Usage Patterns

### Public Endpoint
```csharp
app.MapGet("/users/{id}", Handle)
    .AllowAnonymous();
```

### Authenticated-Only Endpoint
```csharp
app.MapPut("/profile", Handle)
    .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);
```

### Admin-Only Endpoint
```csharp
app.MapDelete("/users/{id}", Handle)
    .RequireAuthorization(AuthorizationPolicies.RequireAdmin);
```

### Role-Based Access
```csharp
private async Task<IResult> Handle(CreateProductRequest req, ClaimsPrincipal user)
{
    var userId = user.GetUserIdSafe();
    
    if (!user.HasAnyRole(AppRole.User, AppRole.Admin))
        return Results.Forbid();
        
    // Process...
}
```

## Available Constants

### Roles (`BuildingBlock.SharedKernel.Constants.AppRole`)
- `AppRole.Root` - superuser; satisfies every role check and policy
- `AppRole.Admin`
- `AppRole.User`

### Policies
- `AuthorizationPolicies.RequireAuthenticated`
- `AuthorizationPolicies.RequireAdmin` - Root or Admin
- `AuthorizationPolicies.RequireUser` - Root, Admin, or User

## Modified Services

### BuildingBlock.Infrastructure
- Added Microsoft.AspNetCore.Authorization package reference
- Created Authorization folder with all core infrastructure

### YarpApiGateway
- Updated DependencyInjection to call AddCommonAuthorizationPolicies and configure policies
- Now uses common policy definitions

### User.API
- Updated DependencyInjection to call AddCommonAuthorizationPolicies and configure policies

### Auth.API
- Updated DependencyInjection to call AddCommonAuthorizationPolicies and configure policies

## Key Design Decisions

1. **No Re-authentication in Services** - JWT validation happens once at gateway
2. **Marker-Based Discovery** - Policies can be auto-discovered by implementing IAuthorizationPolicy
3. **Extension Methods** - Safe claim reading with null checking built-in
4. **Separation of Concerns** - Each service defines its own authorization policies
5. **Common Defaults** - Predefined policies for typical scenarios (Authenticated, Admin, User)

## Next Steps for Services

When adding new services, follow this pattern:

1. In `DependencyInjection.cs`:
```csharp
services
    .AddCommonAuthorizationPolicies()
    .AddAuthorization(options => 
        AuthorizationExtensions.ConfigureCommonPolicies(options))
    .AddCarterModules();
```

2. In endpoints, use:
```csharp
.RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
```

3. In handlers/services, use:
```csharp
var userId = user.GetUserIdSafe();
if (!user.HasRole(AppRole.Admin))
    return Results.Forbid();
```

## Testing Authorization

- Unit tests: Mock ClaimsPrincipal and use extension methods
- Integration tests: Generate valid JWT and pass through API Gateway
- Endpoint tests: Use Carter's test helpers with authenticated user context
