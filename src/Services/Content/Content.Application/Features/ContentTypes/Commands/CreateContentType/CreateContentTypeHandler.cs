using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;

namespace NovaCore.Content.Application.Features.ContentTypes.Commands.CreateContentType;

public sealed class CreateContentTypeHandler(
    IContentTypeReadService contentTypeReadService,
    IContentTypeWriteService contentTypeWriteService) : ICommandHandler<CreateContentTypeCommand, CreateContentTypeResponse>
{
    public async Task<CreateContentTypeResponse> Handle(CreateContentTypeCommand request, CancellationToken ct = default)
    {
        // Validate request
        var key = ContentKey.Create(request.Key);
        if (await contentTypeReadService.GetByKeyAsync(key, ct) is not null)
            throw new ConflictException($"Content type with key ({request.Key}) already exists");

        // Create and persist
        var contentType = ContentType.Create(key, request.Name, request.Description);
        await contentTypeWriteService.CreateAsync(contentType, ct);

        return new CreateContentTypeResponse(contentType.Id);
    }
}
