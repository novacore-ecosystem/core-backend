using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.Auth.Infrastructure.Configurations.Settings;

/// <summary>Endpoint Auth calls to reach the User service over gRPC.</summary>
public sealed class AuthGrpcSetting : GrpcClientSettingBase, ISetting
{
    public const string Section = "Grpc:UserService";

    public AuthGrpcSetting() => Url = "http://user-api:5002";
}
