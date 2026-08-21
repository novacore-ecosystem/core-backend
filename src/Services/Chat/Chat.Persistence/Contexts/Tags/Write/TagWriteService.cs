using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.Tags;
using NovaCore.Chat.Persistence.Contexts.Tags.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Tags.Write;

public sealed class TagWriteService(
    ITagRepository tagRepo,
    IUnitOfWork unitOfWork) : ITagWriteService
{
    public async Task CreateAsync(Tag tag, CancellationToken ct = default)
    {
        await tagRepo.AddAsync(tag, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await tagRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
