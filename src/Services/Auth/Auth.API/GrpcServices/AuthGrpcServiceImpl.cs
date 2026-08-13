using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Contract.Protos.Auth;

using Grpc.Core;

namespace NovaCore.Auth.API.GrpcServices;

/// <summary>
/// gRPC adapter for email validation and tenant version lookup. Presentation-layer only: queries
/// the persistence layer and maps the result to the wire response - no business logic here.
/// Exceptions are left to propagate to NovaCore.BuildingBlock.Grpc's ErrorHandlingInterceptor.
/// </summary>
public sealed class AuthGrpcServiceImpl(
    IAccountReadService accountReadService,
    IAuthService authService,
    ITenantReadService tenantReadService) : AuthGrpcService.AuthGrpcServiceBase
{
    public override async Task<GetUserRolesResponse> GetUserRoles(
        GetUserRolesRequest request,
        ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);
        var roles = await authService.GetUserRolesAsync(
            userId,
            context.CancellationToken);

        var res = new GetUserRolesResponse();
        res.Roles.AddRange(roles);

        return res;
    }

    public override async Task<CheckEmailExistsResponse> CheckEmailExists(
        CheckEmailExistsRequest request,
        ServerCallContext context)
    {
        var account = await accountReadService.GetByEmailAsync(request.Email.Trim(), context.CancellationToken);

        return new CheckEmailExistsResponse
        {
            Exists = account != null,
            UserId = account?.Id.ToString() ?? "",
        };
    }

    public override async Task<GetTenantVersionResponse> GetTenantVersion(
        GetTenantVersionRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.TenantId, out var tenantId))
            return new GetTenantVersionResponse { Found = false, TenantId = request.TenantId };

        var result = await tenantReadService.GetVersionAsync(tenantId, context.CancellationToken);

        return result is null
            ? new GetTenantVersionResponse { Found = false, TenantId = request.TenantId }
            : new GetTenantVersionResponse
            {
                Found = true,
                TenantId = request.TenantId,
                Version = result.Value.Version,
                IsActive = result.Value.IsActive,
            };
    }
}
