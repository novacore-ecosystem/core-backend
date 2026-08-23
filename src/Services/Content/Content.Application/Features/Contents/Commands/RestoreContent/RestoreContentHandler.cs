using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Exceptions;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Commands.RestoreContent;

public sealed class RestoreContentHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<RestoreContentCommand, RestoreContentResponse>
{
    public async Task<RestoreContentResponse> Handle(RestoreContentCommand request, CancellationToken ct = default)
    {
        var content = await contentReadService.GetByIdIncludingDeletedAsync(request.ContentId, ct)
            ?? throw new NotFoundException("Content", request.ContentId);

        if (!content.IsDeleted)
            throw ExceptionFactory.InvalidState("Content is not deleted.");

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await contentWriteService.RestoreAsync(request.ContentId, ct);
        }, ct: ct);

        return new RestoreContentResponse(request.ContentId);
    }
}
