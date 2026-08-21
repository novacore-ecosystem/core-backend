namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationSchedules;

public interface IConversationScheduleReadService
{
    Task<ConversationSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
