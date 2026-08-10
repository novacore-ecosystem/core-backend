using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Order.Domain.Metadata;

public sealed class DiscountMetadata : MetadataBase
{
    public string? SourceName
    {
        get => Get<string>("source_name");
        set => Set(value, "source_name");
    }
}
