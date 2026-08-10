using NovaCore.Audit.Application.Abstractions.Services;

using NovaCore.BuildingBlock.Contract.Protos.User;

namespace NovaCore.Audit.Infrastructure.GrpcClients;

/// <summary>
/// Thin adapter over UserGrpcService's new GetUser RPC (see docs/tasks/2026-07-28/Task13_grpc-proto-getuser-getusers.md)
/// - Audit's first-ever gRPC client, and User's first real gRPC read consumer anywhere in the repo.
/// </summary>
public sealed class UserClientService(UserGrpcService.UserGrpcServiceClient client) : IUserClientService
{
    public async Task<ActorProfile?> GetActorAsync(Guid userId, CancellationToken ct = default)
    {
        var request = new GetUserRequest { UserId = userId.ToString() };
        var response = await client.GetUserAsync(request, cancellationToken: ct);

        return response.Found ? new ActorProfile(userId, response.DisplayName) : null;
    }
}
