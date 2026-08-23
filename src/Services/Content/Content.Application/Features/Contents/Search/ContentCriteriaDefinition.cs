using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Content.Application.Features.Contents.Search;

/// <summary>Admin search whitelist for <see cref="ContentEntity"/>. Built once (static singleton) -
/// no per-request reflection scan. Sorting is intentionally not exposed here - the Admin list is
/// cursor-paginated on a fixed (CreatedAt, Id) keyset (see AdminListContentsHandler), so only
/// Filters from the incoming CriteriaRequest are honored.</summary>
public static class ContentCriteriaDefinition
{
    public static readonly CriteriaDefinition<ContentEntity> Instance = CriteriaDefinition<ContentEntity>.Create()
        .Field(x => x.ContentTypeId).Guid()
        .Field(x => x.Status).Enum()
        .Field(x => x.Visibility).Enum()
        .Field(x => x.CreatedAt).DateTime()
        .Build();
}
