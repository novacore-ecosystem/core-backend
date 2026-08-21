using NovaCore.BuildingBlock.Application.Exceptions;

using Mapster;

using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;

namespace NovaCore.Chat.Application.Features.Conversations.Queries.GetConversation;

public sealed class GetConversationHandler(IConversationReadService conversationReadService)
    : IQueryHandler<GetConversationQuery, GetConversationResponse>
{
    public async Task<GetConversationResponse> Handle(GetConversationQuery request, CancellationToken ct = default)
    {
        var conversation = await conversationReadService.GetByIdAsync(request.ConversationId, ct)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        return conversation.Adapt<GetConversationResponse>();
    }
}
