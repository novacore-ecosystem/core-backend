namespace NovaCore.Chat.Application.Features.ConversationHandovers.Queries.GetMyHandoverInvitations;

public sealed record GetMyHandoverInvitationsQuery : IQuery<IReadOnlyList<HandoverInvitationDto>>;

public sealed record HandoverInvitationDto(
    Guid TransferRequestId,
    Guid ConversationId,
    Guid FromUserId,
    DateTime RequestedAt);
