using NovaCore.Auth.Application.Abstractions.Persistence.Apps;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Apps.Commands.UpsertAppTranslation;

public sealed class UpsertAppTranslationHandler(
    IUnitOfWork unitOfWork,
    IAppWriteService appWriteService) : ICommandHandler<UpsertAppTranslationCommand>
{
    public async Task Handle(UpsertAppTranslationCommand request, CancellationToken ct = default)
    {
        var languageCode = LanguageCode.Create(request.LanguageCode);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await appWriteService.UpdateWithTranslationsAsync(request.AppId, app =>
            {
                app.Translate(languageCode, request.DisplayName, request.Description);
            }, ct);
        }, ct: ct);
    }
}
