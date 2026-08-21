using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationSchedules;
using NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Write;

public sealed class ConversationScheduleWriteService(
    IConversationScheduleRepository scheduleRepo,
    IUnitOfWork unitOfWork) : IConversationScheduleWriteService
{
    public async Task CreateAsync(ConversationSchedule schedule, CancellationToken ct = default)
    {
        await scheduleRepo.AddAsync(schedule, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await scheduleRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
