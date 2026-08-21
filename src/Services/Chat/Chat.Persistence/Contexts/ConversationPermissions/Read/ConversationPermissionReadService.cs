using NovaCore.Chat.Application.Abstractions.Persistence.ConversationPermissions;
using NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Read;

public sealed class ConversationPermissionReadService(IConversationPermissionRepository permissionRepo) : IConversationPermissionReadService
{
    public async Task<ConversationPermission?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await permissionRepo.GetByIdAsync(id, query => query.Include(p => p.Translations), ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await permissionRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalized = EntityCode.Create(code);
        return await permissionRepo.ExistsAsync(p => p.Code == normalized, ct);
    }
}
