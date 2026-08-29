namespace NovaCore.BuildingBlock.Grpc.Client;

/// <summary>
/// Baseline shape for a single gRPC client endpoint. Subclasses inherit this and bind their own
/// section, adding routing extensions directly on the child if a service needs them. Doesn't
/// implement ISetting itself - this project doesn't reference BuildingBlock.Infrastructure - so the
/// concrete service setting adds that marker itself.
/// </summary>
public abstract class GrpcClientSettingBase
{
    public string Url { get; set; } = string.Empty;
}
