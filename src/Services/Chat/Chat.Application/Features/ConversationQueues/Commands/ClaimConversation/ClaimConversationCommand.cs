namespace NovaCore.Chat.Application.Features.ConversationQueues.Commands.ClaimConversation;

public sealed record ClaimConversationCommand(Guid ConversationId) : ICommand;
