using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Domain.Entities.Apps;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Auth.Application.Features.Apps.Queries.GetApp;

public sealed class GetAppHandler(IAppReadService appReadService) : IQueryHandler<GetAppQuery, AppDetailResponse>
{
    public async Task<AppDetailResponse> Handle(GetAppQuery request, CancellationToken ct = default)
    {
        var app = await appReadService.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("App", request.Id);

        return new AppDetailResponse(
            app.Id,
            app.Code.Value,
            app.Name,
            app.IsActive,
            [.. app.Translations.Select(ToTranslationResponse)],
            app.CreatedAt,
            app.UpdatedAt);
    }

    private static AppTranslationResponse ToTranslationResponse(AppTranslation translation) => new(
        translation.LanguageCode.Value,
        translation.DisplayName,
        translation.Description);
}
