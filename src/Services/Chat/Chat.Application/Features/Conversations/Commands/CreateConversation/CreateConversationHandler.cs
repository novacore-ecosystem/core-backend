using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.CreateConversation;

public sealed class CreateConversationHandler(IConversationWriteService conversationWriteService)
    : ICommandHandler<CreateConversationCommand, CreateConversationResponse>
{
    public async Task<CreateConversationResponse> Handle(CreateConversationCommand request, CancellationToken ct = default)
    {
        var conversation = Conversation.Create(
            request.Type,
            request.Lifecycle,
            request.Title,
            request.Description,
            request.Avatar,
            request.Reason,
            request.Priority);

        await conversationWriteService.CreateAsync(conversation, ct);

        return new CreateConversationResponse(conversation.Id);
    }
}
