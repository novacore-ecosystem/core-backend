using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Queries.GetContentById;

public sealed class GetContentByIdHandler(IContentReadService contentReadService) : IQueryHandler<GetContentByIdQuery, GetContentByIdResult>
{
    public async Task<GetContentByIdResult> Handle(GetContentByIdQuery request, CancellationToken ct = default)
    {
        var content = await contentReadService.GetByIdAsync(request.ContentId, ct)
            ?? throw new NotFoundException("Content", request.ContentId);

        return new GetContentByIdResult(
            content.Id,
            content.ContentTypeId,
            content.ContentType.Name,
            content.Slug.Value,
            content.Status,
            content.Visibility,
            content.CurrentVersionId,
            content.PublishedVersionId,
            content.PublishedAt,
            [.. content.Versions
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new GetContentByIdVersionResult(v.Id, v.VersionNumber, v.Status, v.Title, v.Summary))]);
    }
}
