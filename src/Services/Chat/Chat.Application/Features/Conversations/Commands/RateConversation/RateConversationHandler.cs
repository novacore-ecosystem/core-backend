using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationRatings;
using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Common;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.RateConversation;

public sealed class RateConversationHandler(
    IConversationReadService conversationReadService,
    IConversationRatingReadService ratingReadService,
    IConversationRatingWriteService ratingWriteService,
    ICurrentUserService currentUser) : ICommandHandler<RateConversationCommand, RateConversationResponse>
{
    public async Task<RateConversationResponse> Handle(RateConversationCommand request, CancellationToken ct = default)
    {
        var conversation = await conversationReadService.GetByIdAsync(request.ConversationId, ct)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        var isGuest = currentUser.GetRoles().Contains(ChatRoleConstants.Guest);
        ConversationAccessGuard.EnsureCallerCanAccess(conversation, callerId, isGuest);

        if (conversation.Status != ConversationStatus.Closed)
            throw new BadRequestException("Only a closed conversation can be rated.");

        if (await ratingReadService.GetByConversationIdAsync(conversation.Id, ct) is not null)
            throw new ConflictException("This conversation has already been rated.");

        var rating = ConversationRating.Create(conversation.Id, callerId, request.Stars, request.Review);

        await ratingWriteService.CreateAsync(rating, ct);

        return new RateConversationResponse(rating.Id);
    }
}
