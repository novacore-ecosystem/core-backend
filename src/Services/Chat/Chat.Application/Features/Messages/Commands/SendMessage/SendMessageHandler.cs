using Mapster;

using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence;
using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Abstractions.Persistence.Messages;
using NovaCore.Chat.Application.Abstractions.Services;
using NovaCore.Chat.Application.Common;
using NovaCore.Chat.Application.Features.Messages.DTOs;

namespace NovaCore.Chat.Application.Features.Messages.Commands.SendMessage;

/// <summary>
/// Conversation.LastMessageSequence is the authoritative per-conversation counter (not a
/// MAX(Sequence) scan, which races under concurrent sends) - read it, create the Message with the
/// next value, commit both the Message insert and the Conversation's RecordActivity in one
/// transaction, retrying the whole read-act-write cycle on a concurrent-send conflict via
/// OptimisticConcurrencyRetry (same structural race DeductStockHandler already handles this way).
/// </summary>
public sealed class SendMessageHandler(
    IConversationReadService conversationReadService,
    IConversationWriteService conversationWriteService,
    IMessageWriteService messageWriteService,
    IChatRealtimeNotifier realtimeNotifier,
    ICurrentUserService currentUser,
    OptimisticConcurrencyRetry concurrencyRetry,
    IUnitOfWork unitOfWork) : ICommandHandler<SendMessageCommand, SendMessageResponse>
{
    /// <summary>Placeholder business limit for message attachments - not yet exposed as configuration; MaxParseSizeBytes matches FileParsingOptions' own default ceiling.</summary>
    private static readonly FileSizePolicy AttachmentSizePolicy = new(AllowedSizeBytes: 25 * 1024 * 1024, MaxParseSizeBytes: 100 * 1024 * 1024);

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken ct = default)
    {
        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        var isGuest = currentUser.GetRoles().Contains(ChatRoleConstants.Guest);

        var attachments = (request.Attachments ?? [])
            .Select(a => FileUploadClassifier.Classify(
                a.FileName,
                a.ContentType,
                a.SizeBytes,
                a.StorageKey,
                AttachmentSizePolicy,
                a.Width,
                a.Height,
                a.DurationSeconds is null ? null : TimeSpan.FromSeconds(a.DurationSeconds.Value)))
            .ToList();

        var (message, sequence) = await concurrencyRetry.ExecuteAsync(async cancellationToken =>
        {
            var conversation = await conversationReadService.GetByIdAsync(request.ConversationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

            ConversationAccessGuard.EnsureCallerCanAccess(conversation, callerId, isGuest);

            if (conversation.Status == ConversationStatus.Closed)
                throw new BadRequestException("Cannot send a message to a closed conversation.");

            var nextSequence = conversation.LastMessageSequence + 1;

            var newMessage = Message.Create(
                conversation.Id,
                MessageSenderType.User,
                request.ClientMessageId,
                nextSequence,
                request.Type,
                request.Content,
                senderUserId: callerId,
                format: request.Format);

            foreach (var attachment in attachments)
                AddAttachment(newMessage, attachment);

            await unitOfWork.ExecuteTransactionAsync(async () =>
            {
                await messageWriteService.CreateAsync(newMessage, cancellationToken);
                await conversationWriteService.RecordActivityAsync(conversation.Id, nextSequence, cancellationToken);
            }, ct: cancellationToken);

            return (Message: newMessage, Sequence: nextSequence);
        }, ct: ct);

        await realtimeNotifier.PushMessageAsync(request.ConversationId, message.Adapt<ChatMessageDto>(), ct);

        return new SendMessageResponse(message.Id, sequence);
    }

    private static void AddAttachment(Message message, FileUpload upload)
    {
        switch (upload)
        {
            case ImageUpload image:
                message.AddAttachment(AttachmentType.Image, image.FileName, image.ContentType, image.SizeBytes, image.StorageKey, image.Width, image.Height);
                break;
            case MediaUpload media:
                message.AddAttachment(AttachmentType.File, media.FileName, media.ContentType, media.SizeBytes, media.StorageKey, duration: media.Duration);
                break;
            default:
                message.AddAttachment(AttachmentType.File, upload.FileName, upload.ContentType, upload.SizeBytes, upload.StorageKey);
                break;
        }
    }
}
