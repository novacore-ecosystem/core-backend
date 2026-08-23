using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;

namespace NovaCore.Content.Application.Features.Contents.Commands.TranslateContentVersion;

public sealed class TranslateContentVersionHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<TranslateContentVersionCommand, TranslateContentVersionResponse>
{
    public async Task<TranslateContentVersionResponse> Handle(TranslateContentVersionCommand request, CancellationToken ct = default)
    {
        var content = await contentReadService.GetByIdAsync(request.ContentId, ct)
            ?? throw new NotFoundException("Content", request.ContentId);

        var version = content.Versions.FirstOrDefault(v => v.Id == request.VersionId)
            ?? throw new NotFoundException(nameof(ContentVersion), request.VersionId);

        var language = LanguageCode.Create(request.TargetLanguage);
        var wasExisting = version.GetLocalization(language) is not null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await contentWriteService.UpsertLocalizationAsync(
                request.ContentId, request.VersionId, language, request.Title, request.Summary, request.Body,
                request.TranslatedBy, null, ct);
        }, ct: ct);

        return new TranslateContentVersionResponse(request.ContentId, request.VersionId, language.Value, wasExisting);
    }
}
