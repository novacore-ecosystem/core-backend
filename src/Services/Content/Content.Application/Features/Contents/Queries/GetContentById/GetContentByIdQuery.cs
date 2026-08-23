namespace NovaCore.Content.Application.Features.Contents.Queries.GetContentById;

public sealed record GetContentByIdQuery(Guid ContentId) : IQuery<GetContentByIdResult>;

public sealed record GetContentByIdResult(
    Guid ContentId,
    Guid ContentTypeId,
    string ContentTypeName,
    string Slug,
    ContentStatus Status,
    ContentVisibility Visibility,
    bool IsDeleted,
    Guid? CurrentVersionId,
    Guid? PublishedVersionId,
    DateTime? PublishedAt,
    IReadOnlyCollection<GetContentByIdVersionResult> Versions);

public sealed record GetContentByIdVersionResult(
    Guid VersionId,
    int VersionNumber,
    ContentStatus Status,
    IReadOnlyCollection<GetContentByIdLocalizationResult> Localizations);

public sealed record GetContentByIdLocalizationResult(
    string Culture,
    string Title,
    string Summary);
