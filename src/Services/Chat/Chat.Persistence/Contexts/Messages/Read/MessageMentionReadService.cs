using NovaCore.Chat.Application.Abstractions.Persistence.Messages;
using NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Messages.Read;

/// <summary>MessageMention is owned by Message (see Message.AddMention) - queried directly off ChatDbContext, no dedicated repository, same reasoning as Messages' own GetLastSequenceAsync.</summary>
public sealed class MessageMentionReadService(ChatDbContext dbContext) : IMessageMentionReadService
{
    public async Task<IReadOnlyList<MyInteractionDto>> GetForUserAsync(Guid userId, long? beforeTicks, int take, CancellationToken ct = default)
    {
        var query = dbContext.MessageMentions
            .AsNoTracking()
            .Where(m => m.UserId == userId);

        if (beforeTicks is not null)
        {
            var before = new DateTime(beforeTicks.Value, DateTimeKind.Utc);
            query = query.Where(m => m.CreatedAt < before);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .Join(dbContext.Messages.AsNoTracking(),
                mention => mention.MessageId,
                message => message.Id,
                (mention, message) => new MyInteractionDto(
                    InteractionType.Mention,
                    message.ConversationId,
                    mention.MessageId,
                    message.SenderUserId,
                    mention.CreatedAt,
                    mention.MentionType,
                    null))
            .ToListAsync(ct);
    }
}
