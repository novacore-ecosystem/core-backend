using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Queries.GetContentVersion;

public sealed class GetContentVersionHandler(IContentReadService contentReadService) : IQueryHandler<GetContentVersionQuery, GetContentVersionResult>
{
    public async Task<GetContentVersionResult> Handle(GetContentVersionQuery request, CancellationToken ct = default)
    {
        var content = await contentReadService.GetByIdAsync(request.ContentId, ct)
            ?? throw new NotFoundException("Content", request.ContentId);

        var version = content.Versions.FirstOrDefault(v => v.Id == request.VersionId)
            ?? throw new NotFoundException(nameof(ContentVersion), request.VersionId);

        return new GetContentVersionResult(
            content.Id,
            version.Id,
            version.VersionNumber,
            version.Status,
            [.. version.Localizations.Select(l => new GetContentVersionLocalizationResult(
                l.Culture.Value, l.Title, l.Summary, l.Body, l.UpdatedAt))]);
    }
}
