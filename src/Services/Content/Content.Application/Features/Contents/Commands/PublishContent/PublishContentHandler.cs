using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Content;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Commands.PublishContent;

public sealed class PublishContentHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore) : ICommandHandler<PublishContentCommand, PublishContentResponse>
{
    public async Task<PublishContentResponse> Handle(PublishContentCommand request, CancellationToken ct = default)
    {
        // Load and validate against current state
        var content = await contentReadService.GetByIdAsync(request.ContentId, ct)
            ?? throw new NotFoundException("Content", request.ContentId);

        if (content.Versions.All(v => v.Id != request.VersionId))
            throw new NotFoundException(nameof(ContentVersion), request.VersionId);

        // Publish and enqueue the resulting integration event atomically
        var publishedAt = DateTime.UtcNow;
        await PublishAsync(content, request.VersionId, publishedAt, ct);

        return new PublishContentResponse(content.Id, request.VersionId, publishedAt);
    }

    #region Persistence & Events

    private async Task PublishAsync(ContentEntity content, Guid versionId, DateTime publishedAt, CancellationToken ct)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await contentWriteService.PublishAsync(content.Id, versionId, publishedAt, ct);

            await outboxStore.EnqueueAsync(
                new ContentPublishedIntegrationEvent(content.Id, versionId, content.Slug.Value, publishedAt),
                ct);
        }, ct: ct);
    }

    #endregion
}
