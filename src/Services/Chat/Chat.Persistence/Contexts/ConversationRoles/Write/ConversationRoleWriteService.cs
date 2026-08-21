using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationRoles;
using NovaCore.Chat.Persistence.Contexts.ConversationRoles.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRoles.Write;

public sealed class ConversationRoleWriteService(
    IConversationRoleRepository roleRepo,
    IUnitOfWork unitOfWork) : IConversationRoleWriteService
{
    public async Task CreateAsync(ConversationRole role, CancellationToken ct = default)
    {
        await roleRepo.AddAsync(role, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await roleRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
