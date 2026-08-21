namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationRoles;

public interface IConversationRoleReadService
{
    Task<ConversationRole?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
}
