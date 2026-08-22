using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Content;

using NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;
using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Commands.CreateContent;

public sealed class CreateContentHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IContentTypeReadService contentTypeReadService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore) : ICommandHandler<CreateContentCommand, CreateContentResponse>
{
    public async Task<CreateContentResponse> Handle(CreateContentCommand request, CancellationToken ct = default)
    {
        // Validate request
        var slug = ContentSlug.Create(request.Slug);
        await ValidateRequestAsync(request, slug, ct);

        // Create content and publish integration events
        var content = await SaveContentAsync(request, slug, ct);

        return new CreateContentResponse(content.Id, content.CurrentVersionId!.Value);
    }

    #region Validation

    private async Task ValidateRequestAsync(CreateContentCommand request, ContentSlug slug, CancellationToken ct)
    {
        if (!await contentTypeReadService.ExistsByIdAsync(request.ContentTypeId, ct))
            throw new NotFoundException(nameof(ContentType), request.ContentTypeId);

        if (await contentReadService.ExistsBySlugAsync(slug, ct))
            throw new ConflictException($"Content with slug ({request.Slug}) already exists");
    }

    #endregion

    #region Persistence & Events

    private async Task<ContentEntity> SaveContentAsync(CreateContentCommand request, ContentSlug slug, CancellationToken ct)
    {
        var content = ContentEntity.Create(
            request.ContentTypeId,
            slug,
            request.Title,
            request.Summary,
            request.Body,
            request.CreatedBy,
            request.Visibility);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await contentWriteService.CreateAsync(content, ct);

            await outboxStore.EnqueueAsync(
                new ContentCreatedIntegrationEvent(content.Id, content.ContentTypeId, content.Slug.Value, content.CreatedBy),
                ct);
        }, ct: ct);

        return content;
    }

    #endregion
}
