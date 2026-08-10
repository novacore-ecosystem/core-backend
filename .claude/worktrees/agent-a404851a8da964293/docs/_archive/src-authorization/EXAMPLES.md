# Authorization Usage Examples

## Example 1: Public Endpoint (AllowAnonymous)

```csharp
namespace User.API.Endpoints;

using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using Carter;
using User.Application.Features.Users.Queries.GetUser;

public sealed class GetPublicUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{userId}", Handle)
            .AllowAnonymous()  // No authentication required
            .WithName("GetPublicUser")
            .WithOpenApi()
            .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid userId,
        [FromServices] ISender sender)
    {
        var query = new GetUserQuery(userId);
        var response = await sender.Send(query);
        return Results.Ok(response);
    }
}
```

## Example 2: Authenticated Endpoint

```csharp
namespace User.API.Endpoints;

using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using Carter;
using System.Security.Claims;
using User.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/profile", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("UpdateMyProfile")
            .WithOpenApi()
            .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        UpdateProfileRequest req,
        ClaimsPrincipal user,
        [FromServices] ISender sender)
    {
        var userId = user.GetUserIdSafe();
        if (userId == null)
            return Results.Unauthorized();

        var command = new UpdateProfileCommand(Guid.Parse(userId), req.FirstName, req.LastName);
        var response = await sender.Send(command);
        
        return Results.Ok(ApiResponse<UserDto>.Ok(response));
    }
}
```

## Example 3: Admin-Only Endpoint

```csharp
namespace User.API.Endpoints;

using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using Carter;
using User.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{userId}", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("DeleteUser")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid userId,
        [FromServices] ISender sender)
    {
        await sender.Send(new DeleteUserCommand(userId));
        return Results.NoContent();
    }
}
```

## Example 4: Role-Based Endpoint

```csharp
namespace Product.API.Endpoints;

using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using Carter;
using System.Security.Claims;
using Product.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireUser)  // User or Admin
            .WithName("CreateProduct")
            .WithOpenApi()
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        CreateProductRequest req,
        ClaimsPrincipal user,
        [FromServices] ISender sender)
    {
        var userId = user.GetUserIdSafe();
        if (userId == null)
            return Results.Unauthorized();

        // Only Users and Admins can create products
        if (!user.HasAnyRole(AppRole.User, AppRole.Admin))
            return Results.Forbid();

        var command = new CreateProductCommand(
            Guid.Parse(userId),
            req.Name,
            req.Description,
            req.Price
        );

        var response = await sender.Send(command);
        return Results.Created($"/api/products/{response.Id}", 
            ApiResponse<ProductDto>.Ok(response));
    }
}
```

## Example 5: Advanced Authorization with Custom Claim

```csharp
namespace Order.API.Endpoints;

using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using Carter;
using System.Security.Claims;
using Order.Application.Features.Orders.Commands.ApproveOrder;

public sealed class ApproveOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{orderId}/approve", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("ApproveOrder")
            .WithOpenApi()
            .Produces<ApiResponse<OrderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        ClaimsPrincipal user,
        [FromServices] ISender sender)
    {
        var userId = user.GetUserIdSafe();
        if (userId == null)
            return Results.Unauthorized();

        // Check multiple authorization conditions
        var roles = user.GetRoles().ToList();
        var hasAdminRole = user.HasRole(AppRole.Admin);

        if (!hasAdminRole)
            return Results.Forbid();

        // Get custom permissions claim (if present in token)
        var permissions = user.GetClaimValues("permissions").ToList();
        if (!permissions.Contains("approve_order") && !hasAdminRole)
            return Results.Forbid();

        var command = new ApproveOrderCommand(orderId, Guid.Parse(userId));
        var response = await sender.Send(command);

        return Results.Ok(ApiResponse<OrderDto>.Ok(response));
    }
}
```

## Example 6: Service-to-Service Authorization (Using Claims)

```csharp
namespace Product.API.GrpcServices;

using BuildingBlock.Infrastructure.Authorization;
using Grpc.Core;
using System.Security.Claims;
using Product.Application.Services;

public class ProductGrpcService : Product.ProductGrpc.ProductGrpcBase
{
    private readonly IProductService _productService;

    public ProductGrpcService(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<GetProductResponse> GetProduct(
        GetProductRequest request,
        ServerCallContext context)
    {
        // Get claims from gRPC metadata
        var userIdMeta = context.RequestHeaders.FirstOrDefault(m => m.Key == "user-id")?.Value;
        var roleMeta = context.RequestHeaders.FirstOrDefault(m => m.Key == "user-role")?.Value;

        if (string.IsNullOrEmpty(userIdMeta))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing user-id"));

        var product = await _productService.GetProductByIdAsync(Guid.Parse(request.ProductId));
        
        return new GetProductResponse
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Price = product.Price.ToString()
        };
    }
}
```

## Example 7: Multiple Roles

```csharp
namespace Inventory.API.Endpoints;

using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using Carter;
using System.Security.Claims;
using Inventory.Application.Features.Inventory.Commands.UpdateStock;

public sealed class UpdateStockEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventory/{skuId}/update-stock", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("UpdateStock")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string skuId,
        UpdateStockRequest req,
        ClaimsPrincipal user,
        [FromServices] ISender sender)
    {
        var userId = user.GetUserIdSafe();
        if (userId == null)
            return Results.Unauthorized();

        // Multiple role check: User OR Admin
        if (!user.HasAnyRole(
            AppRole.User,
            AppRole.Admin))
        {
            return Results.Forbid();
        }

        var command = new UpdateStockCommand(skuId, req.Quantity);
        await sender.Send(command);

        return Results.NoContent();
    }
}
```

## Example 8: Authorization in Handler (Business Logic)

```csharp
namespace User.Application.Features.Users.Commands.UpdateProfile;

using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using User.Application.Abstractions.Persistence;
using MediatR;
using System.Security.Claims;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            throw new UnauthorizedAccessException("User not found in context");

        var currentUserId = user.GetUserIdSafe();
        if (currentUserId == null)
            throw new UnauthorizedAccessException("User ID not found in claims");

        // Users can only update their own profile; admins can update anyone's
        if (currentUserId != request.UserId.ToString() && !user.HasRole(AppRole.Admin))
            throw new UnauthorizedAccessException("You can only update your own profile");

        var existingUser = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (existingUser == null)
            throw new NotFoundException($"User {request.UserId} not found");

        existingUser.FirstName = request.FirstName;
        existingUser.LastName = request.LastName;

        await _userRepository.UpdateAsync(existingUser, ct);
        return existingUser.Adapt<UserDto>();
    }
}
```

## Key Takeaways

1. **API Gateway** validates JWT and extracts claims
2. **Services** use `RequireAuthorization()` to check if user is authenticated
3. **Services** use `ClaimsPrincipalExtensions` to read claims safely
4. **Services** make authorization decisions based on roles/claims
5. **No re-authentication** needed in services
6. **Predefined policies** for common scenarios (RequireAuthenticated, RequireAdmin, RequireUser)
