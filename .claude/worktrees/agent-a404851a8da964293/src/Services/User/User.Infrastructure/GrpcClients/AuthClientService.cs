using NovaCore.BuildingBlock.Contract.Protos.Auth;

using Grpc.Core;

using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Infrastructure.GrpcClients;

public sealed class AuthClientService(AuthGrpcService.AuthGrpcServiceClient client) : IAuthClientService
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var request = new CheckEmailExistsRequest { Email = email.Trim() };

        try
        {
            var response = await client.CheckEmailExistsAsync(request, cancellationToken: ct);
            return response.Exists;
        }
        catch (RpcException)
        {
            throw;
        }
    }

    public async Task<string[]> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var request = new GetUserRolesRequest { UserId = userId.ToString() };
        try
        {
            var response = await client.GetUserRolesAsync(request, cancellationToken: ct);
            return [.. response.Roles];
        }
        catch (RpcException)
        {
            throw;
        }
    }
}
