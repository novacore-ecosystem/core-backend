using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Common;

namespace NovaCore.Content.Application.Features.Contents.Commands.UpdateContentDraft;

public sealed class UpdateContentDraftHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateContentDraftCommand, UpdateContentDraftResponse>
{
    public async Task<UpdateContentDraftResponse> Handle(UpdateContentDraftCommand request, CancellationToken ct = default)
    {
        var content = await contentReadService.GetByIdAsync(request.ContentId, ct)
            ?? throw new NotFoundException("Content", request.ContentId);

        if (content.Versions.All(v => v.Id != request.VersionId))
            throw new NotFoundException(nameof(ContentVersion), request.VersionId);

        var language = LanguageCode.Create(ContentLanguageDefaults.OrDefault(request.Language));

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await contentWriteService.UpsertLocalizationAsync(
                request.ContentId, request.VersionId, language, request.Title, request.Summary, request.Body,
                request.UpdatedBy, null, ct);
        }, ct: ct);

        return new UpdateContentDraftResponse(request.ContentId, request.VersionId, language.Value);
    }
}
