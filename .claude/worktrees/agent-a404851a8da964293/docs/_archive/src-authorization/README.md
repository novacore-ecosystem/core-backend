# Authorization Policy Pattern

## Architecture

- **API Gateway**: Handles JWT authentication and validation. Extracts claims into ClaimsPrincipal.
- **Services**: Only read claims to make authorization decisions. No re-authentication.

## Usage in Services

### 1. Register Authorization Policies

In your service's `DependencyInjection.cs`:

```csharp
using BuildingBlock.Infrastructure.Authorization;

public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration config)
{
    services
        .AddCommonAuthorizationPolicies()  // Register handlers
        .AddCarterModules()
        .AddAuthorization(options => 
            AuthorizationExtensions.ConfigureCommonPolicies(options));  // Register policies
    
    return services;
}
```

### 2. Use in Endpoints

#### Using Policy Attribute

```csharp
using BuildingBlock.Infrastructure.Authorization.Attributes;
using Mapster;
using Carter;

public class GetUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id}", GetUser)
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("GetUser")
            .WithOpenApi();
    }

    private async Task<IResult> GetUser(string id, IUserService userService)
    {
        var user = await userService.GetUserByIdAsync(id);
        return user is null ? Results.NotFound() : Results.Ok(user.Adapt<UserDto>());
    }
}
```

#### Using ClaimsPrincipal Extension Methods

```csharp
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using Carter;

public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", CreateProduct)
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .RequireAuthorization()
            .WithName("CreateProduct")
            .WithOpenApi();
    }

    private async Task<IResult> CreateProduct(CreateProductRequest req, ClaimsPrincipal user, IProductService svc)
    {
        var userId = user.GetUserIdSafe() ?? throw new UnauthorizedAccessException();
        
        if (!user.HasRole(AppRole.Admin))
            return Results.Forbid();

        var product = await svc.CreateAsync(userId, req);
        return Results.Created($"/api/products/{product.Id}", product);
    }
}
```

#### Using Role Attributes

```csharp
using BuildingBlock.Infrastructure.Authorization.Attributes;
using Carter;

public class DeleteProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", DeleteProduct)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)  // or use policy name
            .WithName("DeleteProduct")
            .WithOpenApi();
    }

    private async Task<IResult> DeleteProduct(string id, IProductService svc)
    {
        await svc.DeleteAsync(id);
        return Results.NoContent();
    }
}
```

### 3. Read Claims Safely

```csharp
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using System.Security.Claims;

// In your handler or service
public async Task ProcessOrderAsync(ClaimsPrincipal user, Order order)
{
    var userId = user.GetUserIdSafe();
    var email = user.GetEmail();
    var roles = user.GetRoles();
    
    if (user.HasRole(AppRole.Admin))
    {
        // Admin actions
    }
    
    if (user.HasAnyRole(AppRole.User, AppRole.Admin))
    {
        // User/Admin actions
    }
}
```

## Available Methods

### ClaimsPrincipalExtensions

- `GetUserId()` - Get user ID from NameIdentifier claim
- `GetUserIdSafe()` - Get user ID from 'sub' or NameIdentifier claim
- `GetEmail()` - Get email claim
- `GetRoles()` - Get all role claims
- `HasRole(string)` - Check if user has specific role (Root always passes)
- `HasAnyRole(params string[])` - Check if user has any of the roles (Root always passes)
- `HasAllRoles(params string[])` - Check if user has all of the roles (Root always passes)
- `GetClaim(string)` - Get specific claim by type
- `GetClaimValues(string)` - Get all values for a claim type

## Common Authorization Constants

### Roles (`BuildingBlock.SharedKernel.Constants.AppRole`)
- `AppRole.Root` - superuser; satisfies every role check and policy
- `AppRole.Admin`
- `AppRole.User`

### Policies
- `AuthorizationPolicies.RequireAuthenticated` - User must be logged in
- `AuthorizationPolicies.RequireAdmin` - User must be Root or Admin
- `AuthorizationPolicies.RequireUser` - User must be Root, Admin, or User

## Custom Policies (Per Service)

Services can define custom policies in their own `DependencyInjection.cs`:

```csharp
public static IServiceCollection AddPresentation(this IServiceCollection services)
{
    services
        .AddCommonAuthorizationPolicies()
        .AddAuthorizationBuilder()
        .AddPolicy("ManageCatalog", policy =>
            policy.RequireAuthenticatedUser()
                .RequireRole(AppRole.Root, AppRole.Admin))
        .Services
        .AddCarterModules();
    
    return services;
}
```

Then use it:

```csharp
.RequireAuthorization("ManageCatalog")
```

## Flow Summary

1. Client sends request with JWT token (in Authorization header or cookie)
2. **API Gateway**:
   - Validates JWT signature, issuer, audience, expiration
   - Extracts claims into `ClaimsPrincipal`
   - Forwards request to service with ClaimsPrincipal attached
3. **Service**:
   - Uses `RequireAuthorization()` to check if user is authenticated
   - Uses `ClaimsPrincipalExtensions` methods to read claims
   - Makes authorization decisions based on roles/claims
   - No re-authentication needed

## Important Notes

- **No re-authentication in services** - Trust API Gateway validation
- **Read claims, don't call external auth services** - Claims are already set
- **Use extension methods for safety** - They handle missing claims gracefully
- **Define custom policies per service** - Services are independent
