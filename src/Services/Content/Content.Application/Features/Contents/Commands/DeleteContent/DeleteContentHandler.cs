using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Content;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Commands.DeleteContent;

public sealed class DeleteContentHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore) : ICommandHandler<DeleteContentCommand, DeleteContentResponse>
{
    public async Task<DeleteContentResponse> Handle(DeleteContentCommand request, CancellationToken ct = default)
    {
        if (!await contentReadService.ExistsByIdAsync(request.ContentId, ct))
            throw new NotFoundException("Content", request.ContentId);

        var deletedAt = DateTime.UtcNow;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await contentWriteService.DeleteAsync(request.ContentId, ct);
            await outboxStore.EnqueueAsync(new ContentDeletedIntegrationEvent(request.ContentId), ct);
        }, ct: ct);

        return new DeleteContentResponse(request.ContentId, deletedAt);
    }
}
