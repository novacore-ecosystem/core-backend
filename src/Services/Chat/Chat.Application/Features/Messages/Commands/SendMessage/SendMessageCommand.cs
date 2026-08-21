using NovaCore.Chat.Application.Features.Messages.DTOs;

namespace NovaCore.Chat.Application.Features.Messages.Commands.SendMessage;

public sealed record SendMessageCommand(
    Guid ConversationId,
    string ClientMessageId,
    MessageType Type,
    string Content,
    MessageFormat Format = MessageFormat.PlainText,
    List<MessageAttachmentRequest>? Attachments = null) : ICommand<SendMessageResponse>;

public sealed record SendMessageResponse(Guid MessageId, long Sequence);
