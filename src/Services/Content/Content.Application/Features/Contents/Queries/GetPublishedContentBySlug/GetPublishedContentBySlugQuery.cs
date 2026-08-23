namespace NovaCore.Content.Application.Features.Contents.Queries.GetPublishedContentBySlug;

/// <summary>Public single-item read - resolves the currently published version only, in the
/// requested language falling back to the service default, exactly like GetLandingContents. Never
/// exposes drafts, deleted content, or other versions.</summary>
public sealed record GetPublishedContentBySlugQuery(string Slug, string? Language) : IQuery<GetPublishedContentBySlugResult>;

public sealed record GetPublishedContentBySlugResult(
    Guid ContentId,
    Guid ContentTypeId,
    string Slug,
    string Language,
    string Title,
    string Summary,
    string Body,
    DateTime PublishedAt);
