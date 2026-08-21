using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;

namespace NovaCore.Chat.Application.Features.ConversationHandovers.Queries.GetMyHandoverInvitations;

/// <summary>The user id always comes from the authenticated token, never a client-supplied parameter - there is no route/body id to spoof here by design.</summary>
public sealed class GetMyHandoverInvitationsHandler(
    IConversationTransferRequestReadService transferRequestReadService,
    ICurrentUserService currentUser)
    : IQueryHandler<GetMyHandoverInvitationsQuery, IReadOnlyList<HandoverInvitationDto>>
{
    public async Task<IReadOnlyList<HandoverInvitationDto>> Handle(GetMyHandoverInvitationsQuery request, CancellationToken ct = default)
    {
        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();

        var invitations = await transferRequestReadService.GetPendingForUserAsync(callerId, ct);

        return [.. invitations.Select(i => new HandoverInvitationDto(i.Id, i.ConversationId, i.FromUserId, i.RequestedAt))];
    }
}
