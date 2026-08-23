using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Common;

namespace NovaCore.Content.Application.Features.Contents.Queries.GetPublishedContentBySlug;

/// <summary>The "normal public read" flow (WCM spec section 23): published content, resolved to
/// the requested language with fallback to the service default, and finally to whatever language
/// the published version does carry if neither is present.</summary>
public sealed class GetPublishedContentBySlugHandler(IContentReadService contentReadService)
    : IQueryHandler<GetPublishedContentBySlugQuery, GetPublishedContentBySlugResult>
{
    public async Task<GetPublishedContentBySlugResult> Handle(GetPublishedContentBySlugQuery request, CancellationToken ct = default)
    {
        var slug = ContentSlug.Create(request.Slug);
        var content = await contentReadService.GetBySlugAsync(slug, ct)
            ?? throw new NotFoundException("Content", request.Slug);

        if (content.Status != ContentStatus.Published || content.PublishedVersion is null)
            throw new NotFoundException("Content", request.Slug);

        var requested = ContentLanguageDefaults.OrDefault(request.Language);
        var fallback = ContentLanguageDefaults.Default;

        var localization = content.PublishedVersion.Localizations.FirstOrDefault(l => l.Culture.Value == requested)
            ?? content.PublishedVersion.Localizations.FirstOrDefault(l => l.Culture.Value == fallback)
            ?? content.PublishedVersion.Localizations.FirstOrDefault()
            ?? throw new NotFoundException("Content", request.Slug);

        return new GetPublishedContentBySlugResult(
            content.Id,
            content.ContentTypeId,
            content.Slug.Value,
            localization.Culture.Value,
            localization.Title,
            localization.Summary,
            localization.Body,
            content.PublishedAt!.Value);
    }
}
