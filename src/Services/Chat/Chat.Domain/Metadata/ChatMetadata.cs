using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Chat.Domain.Metadata;

/// <summary>
/// Shared extensible metadata bag reused across every Chat aggregate/entity with a Metadata
/// property. Promotion.Domain's entire 70+-entity domain has exactly one MetadataBase subclass
/// (PromotionMetadata) - this mirrors that shape (Note/ExternalReference) rather than declaring a
/// near-duplicate class per entity.
/// </summary>
public sealed class ChatMetadata : MetadataBase
{
    public string? Note
    {
        get => Get<string>("note");
        set => Set(value, "note");
    }

    public string? ExternalReference
    {
        get => Get<string>("external_reference");
        set => Set(value, "external_reference");
    }
}
