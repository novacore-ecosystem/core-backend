using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Chat.Application.Abstractions.Persistence.Contacts;
using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Abstractions.Services;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.StartGuestConversation;

public sealed class StartGuestConversationHandler(
    IContactWriteService contactWriteService,
    IConversationWriteService conversationWriteService,
    IGuestTokenGenerator guestTokenGenerator) : ICommandHandler<StartGuestConversationCommand, StartGuestConversationResponse>
{
    public async Task<StartGuestConversationResponse> Handle(StartGuestConversationCommand request, CancellationToken ct = default)
    {
        var contact = Contact.Create(
            request.DisplayName,
            email: request.Email is null ? null : Email.Create(request.Email),
            phone: request.Phone is null ? null : PhoneNumber.Create(request.Phone));
        await contactWriteService.CreateAsync(contact, ct);

        var conversation = Conversation.Create(
            ConversationType.OneToOne,
            ConversationLifecycle.Session,
            reason: request.Reason,
            status: ConversationStatus.Queued);
        conversation.AssignContact(contact.Id);
        await conversationWriteService.CreateAsync(conversation, ct);

        var accessToken = guestTokenGenerator.GenerateAccessToken(contact.Id, request.DisplayName);

        return new StartGuestConversationResponse(contact.Id, conversation.Id, accessToken);
    }
}
