namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationPermissions;

public interface IConversationPermissionWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationPermission permission, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
