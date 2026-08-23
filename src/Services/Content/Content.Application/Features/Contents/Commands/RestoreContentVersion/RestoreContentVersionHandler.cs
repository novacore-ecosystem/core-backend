using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Commands.RestoreContentVersion;

public sealed class RestoreContentVersionHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<RestoreContentVersionCommand, RestoreContentVersionResponse>
{
    public async Task<RestoreContentVersionResponse> Handle(RestoreContentVersionCommand request, CancellationToken ct = default)
    {
        var content = await contentReadService.GetByIdAsync(request.ContentId, ct)
            ?? throw new NotFoundException("Content", request.ContentId);

        if (content.Versions.All(v => v.Id != request.VersionId))
            throw new NotFoundException(nameof(ContentVersion), request.VersionId);

        (Guid Id, int VersionNumber) restored = default;
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            restored = await contentWriteService.RestoreVersionAsync(request.ContentId, request.VersionId, request.RestoredBy, ct);
        }, ct: ct);

        return new RestoreContentVersionResponse(request.ContentId, restored.Id, restored.VersionNumber);
    }
}
