namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationPermissions;

public interface IConversationPermissionReadService
{
    Task<ConversationPermission?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
}
