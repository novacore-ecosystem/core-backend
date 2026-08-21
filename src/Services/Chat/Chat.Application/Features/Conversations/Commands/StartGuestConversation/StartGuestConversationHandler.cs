using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Chat.Application.Abstractions.Persistence.Contacts;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;
using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Abstractions.Services;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.StartGuestConversation;

public sealed class StartGuestConversationHandler(
    IContactWriteService contactWriteService,
    IConversationWriteService conversationWriteService,
    IConversationQueueReadService queueReadService,
    IConversationQueueItemWriteService queueItemWriteService,
    IGuestTokenGenerator guestTokenGenerator) : ICommandHandler<StartGuestConversationCommand, StartGuestConversationResponse>
{
    /// <summary>Every new guest conversation enters this well-known queue - same "configured-by-convention default" pattern Inventory's DeductStockHandler uses for its platform warehouse. An admin must seed a ConversationQueue with this code before guest chat is usable.</summary>
    private const string DefaultQueueCode = "DEFAULT";

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

        var defaultQueue = await queueReadService.GetByCodeAsync(DefaultQueueCode, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Default conversation queue '{DefaultQueueCode}' is not configured.");
        await queueItemWriteService.EnqueueAsync(defaultQueue.Id, conversation.Id, priority: 0, ct);

        var accessToken = guestTokenGenerator.GenerateAccessToken(contact.Id, request.DisplayName);

        return new StartGuestConversationResponse(contact.Id, conversation.Id, accessToken);
    }
}
