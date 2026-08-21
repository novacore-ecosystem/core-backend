using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationPermissions;
using NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Write;

public sealed class ConversationPermissionWriteService(
    IConversationPermissionRepository permissionRepo,
    IUnitOfWork unitOfWork) : IConversationPermissionWriteService
{
    public async Task CreateAsync(ConversationPermission permission, CancellationToken ct = default)
    {
        await permissionRepo.AddAsync(permission, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await permissionRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
