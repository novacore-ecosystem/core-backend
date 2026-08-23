namespace NovaCore.Content.Application.Features.Contents.Queries.GetContentVersion;

public sealed record GetContentVersionQuery(Guid ContentId, Guid VersionId) : IQuery<GetContentVersionResult>;

public sealed record GetContentVersionResult(
    Guid ContentId,
    Guid VersionId,
    int VersionNumber,
    ContentStatus Status,
    IReadOnlyCollection<GetContentVersionLocalizationResult> Localizations);

public sealed record GetContentVersionLocalizationResult(
    string Culture,
    string Title,
    string Summary,
    string Body,
    DateTime UpdatedAt);
