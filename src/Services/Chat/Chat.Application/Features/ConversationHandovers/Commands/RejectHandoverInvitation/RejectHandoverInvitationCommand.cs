namespace NovaCore.Chat.Application.Features.ConversationHandovers.Commands.RejectHandoverInvitation;

public sealed record RejectHandoverInvitationCommand(Guid TransferRequestId) : ICommand;
