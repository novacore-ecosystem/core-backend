using NovaCore.Chat.Application.Abstractions.Persistence.ConversationRoles;
using NovaCore.Chat.Persistence.Contexts.ConversationRoles.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRoles.Read;

public sealed class ConversationRoleReadService(IConversationRoleRepository roleRepo) : IConversationRoleReadService
{
    public async Task<ConversationRole?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await roleRepo.GetByIdAsync(id, query => query.Include(r => r.Translations), ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await roleRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalized = EntityCode.Create(code);
        return await roleRepo.ExistsAsync(r => r.Code == normalized, ct);
    }
}
