using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.Audit.Infrastructure.Configurations.Settings;

/// <summary>Endpoint Audit calls to reach the User service over gRPC.</summary>
public sealed class AuditGrpcSetting : GrpcClientSettingBase, ISetting
{
    public const string Section = "Grpc:UserService";

    public AuditGrpcSetting() => Url = "http://user-api:5002";
}
