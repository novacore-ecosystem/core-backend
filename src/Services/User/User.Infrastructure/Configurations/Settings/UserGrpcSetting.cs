using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.User.Infrastructure.Configurations.Settings;

/// <summary>Endpoint User calls to reach the Auth service over gRPC.</summary>
public sealed class UserGrpcSetting : GrpcClientSettingBase, ISetting
{
    public const string Section = "Grpc:AuthService";

    public UserGrpcSetting() => Url = "http://auth-api:5002";
}
