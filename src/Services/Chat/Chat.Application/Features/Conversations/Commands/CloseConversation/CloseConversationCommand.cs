namespace NovaCore.Chat.Application.Features.Conversations.Commands.CloseConversation;

/// <summary>
/// Shared by guest/user self-close and CS-initiated close - no separate "admin close" endpoint for
/// this phase (see ConversationAccessGuard: an internal user may close any conversation they're
/// authenticated for, a stricter CS-specific rule is deferred to a real permission system).
/// </summary>
public sealed record CloseConversationCommand(Guid ConversationId) : ICommand;
