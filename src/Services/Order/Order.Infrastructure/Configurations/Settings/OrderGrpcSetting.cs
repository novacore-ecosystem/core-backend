using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.Order.Infrastructure.Configurations.Settings;

/// <summary>Endpoint Order calls to reach the Inventory service over gRPC.</summary>
public sealed class OrderGrpcSetting : GrpcClientSettingBase, ISetting
{
    public const string Section = "Grpc:InventoryService";

    public OrderGrpcSetting() => Url = "http://inventory-api:5002";
}
