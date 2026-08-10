using NovaCore.Auth.Application.Abstractions.Services;

using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Contract.Protos.User;

using Grpc.Core;

namespace NovaCore.Auth.Infrastructure.GrpcClients;

public sealed class UserProfileServiceClient(UserGrpcService.UserGrpcServiceClient client) : IUserProfileService
{
    public async Task<GrpcServiceResult<UserProfileData>> CreateUserProfileAsync(
        Guid accountId,
        string email,
        string userName,
        string firstName,
        string middleName,
        string lastName,
        string phoneNumber,
        string correlationId,
        CancellationToken ct = default)
    {
        var request = new CreateUserProfileRequest
        {
            UserId = accountId.ToString(),
            Email = email,
            UserName = userName,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            CorrelationId = correlationId,
        };

        try
        {
            var response = await client.CreateUserProfileAsync(request, cancellationToken: ct);

            return response.Success
                ? new GrpcServiceResult<UserProfileData>(
                    Success: true,
                    Data: new UserProfileData(Guid.Parse(response.UserId)),
                    Message: response.Message)
                : new GrpcServiceResult<UserProfileData>(
                    Success: false,
                    ErrorCode: response.ErrorCode,
                    Message: response.Message);
        }
        catch (RpcException ex)
        {
            return new GrpcServiceResult<UserProfileData>(
                Success: false,
                ErrorCode: ex.StatusCode.ToString(),
                Message: ex.Status.Detail);
        }
    }
}
