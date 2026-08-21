using NovaCore.Chat.Application.Abstractions.Persistence.ConversationSchedules;
using NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Read;

public sealed class ConversationScheduleReadService(IConversationScheduleRepository scheduleRepo) : IConversationScheduleReadService
{
    public async Task<ConversationSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await scheduleRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await scheduleRepo.ExistsByIdAsync(id, ct);
    }
}
