using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.Product.Infrastructure.Configurations.Settings;

/// <summary>Endpoint Product calls to reach the Inventory service over gRPC.</summary>
public sealed class ProductGrpcSetting : GrpcClientSettingBase, ISetting
{
    public const string Section = "Grpc:InventoryService";

    public ProductGrpcSetting() => Url = "http://inventory-api:5002";
}
