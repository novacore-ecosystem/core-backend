using NovaCore.Chat.Application.Abstractions.Persistence.Messages;
using NovaCore.Chat.Persistence.Contexts.Messages.Repositories;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Messages.Read;

public sealed class MessageReadService(IMessageRepository messageRepo, ChatDbContext dbContext) : IMessageReadService
{
    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await messageRepo.GetByIdAsync(
            id,
            query => query
                .Include(m => m.Attachments)
                .Include(m => m.References)
                .Include(m => m.Reactions)
                .Include(m => m.Mentions),
            ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await messageRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<long> GetLastSequenceAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .Select(m => (long?)m.Sequence)
            .MaxAsync(ct) ?? 0;
    }

    public async Task<IReadOnlyList<Message>> GetSinceSequenceAsync(Guid conversationId, long afterSequence, int limit, CancellationToken ct = default)
    {
        return await dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && m.Sequence > afterSequence)
            .OrderBy(m => m.Sequence)
            .Take(limit)
            .ToListAsync(ct);
    }
}
