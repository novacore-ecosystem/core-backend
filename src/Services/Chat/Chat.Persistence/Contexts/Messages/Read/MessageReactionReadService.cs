using NovaCore.Chat.Application.Abstractions.Persistence.Messages;
using NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Messages.Read;

/// <summary>MessageReaction is owned by Message (see Message.AddReaction) - queried directly off ChatDbContext, no dedicated repository (composite key, no internal Create gate), same reasoning as MessageMentionReadService.</summary>
public sealed class MessageReactionReadService(ChatDbContext dbContext) : IMessageReactionReadService
{
    public async Task<IReadOnlyList<MyInteractionDto>> GetOnUserMessagesAsync(Guid userId, long? beforeTicks, int take, CancellationToken ct = default)
    {
        var query =
            from reaction in dbContext.MessageReactions.AsNoTracking()
            join message in dbContext.Messages.AsNoTracking() on reaction.MessageId equals message.Id
            where message.SenderUserId == userId
            select new { reaction, message };

        if (beforeTicks is not null)
        {
            var before = new DateTime(beforeTicks.Value, DateTimeKind.Utc);
            query = query.Where(x => x.reaction.CreatedAt < before);
        }

        return await query
            .OrderByDescending(x => x.reaction.CreatedAt)
            .Take(take)
            .Select(x => new MyInteractionDto(
                InteractionType.Reaction,
                x.message.ConversationId,
                x.reaction.MessageId,
                x.reaction.UserId,
                x.reaction.CreatedAt,
                null,
                x.reaction.ReactionType))
            .ToListAsync(ct);
    }
}
