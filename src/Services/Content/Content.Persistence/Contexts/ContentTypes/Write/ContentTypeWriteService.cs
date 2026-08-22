using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;
using NovaCore.Content.Persistence.Contexts.ContentTypes.Repositories;

namespace NovaCore.Content.Persistence.Contexts.ContentTypes.Write;

public sealed class ContentTypeWriteService(
    IContentTypeRepository contentTypeRepo,
    IUnitOfWork unitOfWork) : IContentTypeWriteService
{
    public async Task CreateAsync(ContentType contentType, CancellationToken ct = default)
    {
        await contentTypeRepo.AddAsync(contentType, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
