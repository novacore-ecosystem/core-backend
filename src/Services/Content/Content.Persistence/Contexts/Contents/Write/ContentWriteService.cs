using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Persistence.Contexts.Contents.Repositories;

namespace NovaCore.Content.Persistence.Contexts.Contents.Write;

public sealed class ContentWriteService(
    IContentRepository contentRepo,
    IUnitOfWork unitOfWork) : IContentWriteService
{
    public async Task CreateAsync(ContentEntity content, CancellationToken ct = default)
    {
        await contentRepo.AddAsync(content, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task PublishAsync(Guid id, Guid versionId, DateTime publishedAt, CancellationToken ct = default)
    {
        await contentRepo.UpdateAsync(id, c => c.Publish(versionId, publishedAt), ct);
        // no commit here - the calling handler wraps this in its own ExecuteTransactionAsync
        // alongside the ContentPublishedIntegrationEvent Outbox enqueue.
    }
}
