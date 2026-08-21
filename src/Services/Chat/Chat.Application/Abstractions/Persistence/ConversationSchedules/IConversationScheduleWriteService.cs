namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationSchedules;

public interface IConversationScheduleWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationSchedule schedule, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
