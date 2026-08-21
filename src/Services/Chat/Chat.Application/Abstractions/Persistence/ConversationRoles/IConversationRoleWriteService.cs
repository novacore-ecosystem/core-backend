namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationRoles;

public interface IConversationRoleWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationRole role, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
