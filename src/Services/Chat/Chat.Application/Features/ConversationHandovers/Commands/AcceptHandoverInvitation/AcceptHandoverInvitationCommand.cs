namespace NovaCore.Chat.Application.Features.ConversationHandovers.Commands.AcceptHandoverInvitation;

public sealed record AcceptHandoverInvitationCommand(Guid TransferRequestId) : ICommand;
