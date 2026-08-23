using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Common;

namespace NovaCore.Content.Application.Features.Contents.Commands.CreateContentVersion;

/// <summary>Creates a new draft version on an existing Content - the "new edit round" step of the
/// Version+Language model, distinct from CreateContent's required first version.</summary>
public sealed class CreateContentVersionHandler(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateContentVersionCommand, CreateContentVersionResponse>
{
    public async Task<CreateContentVersionResponse> Handle(CreateContentVersionCommand request, CancellationToken ct = default)
    {
        if (!await contentReadService.ExistsByIdAsync(request.ContentId, ct))
            throw new NotFoundException("Content", request.ContentId);

        var language = LanguageCode.Create(ContentLanguageDefaults.OrDefault(request.Language));

        (Guid Id, int VersionNumber) version = default;
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            version = await contentWriteService.CreateDraftVersionAsync(
                request.ContentId, language, request.Title, request.Summary, request.Body, request.CreatedBy, null, ct);
        }, ct: ct);

        return new CreateContentVersionResponse(request.ContentId, version.Id, version.VersionNumber);
    }
}
