using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationHandovers.Queries.GetMyHandoverInvitations;

namespace NovaCore.Chat.API.Endpoints.ConversationHandovers;

public sealed class GetMyHandoverInvitationsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get My Handover Invitations",
        "",
        "Pending conversation handover requests addressed to the authenticated caller.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/handovers/invitations", Handle)
            .WithTags("ConversationHandovers")
            .RequireAuthorization()
            .WithName("GetMyHandoverInvitations")
            .WithDisplayName("Get My Handover Invitations API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<IReadOnlyList<HandoverInvitationDto>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle([FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new GetMyHandoverInvitationsQuery(), ct);

        return Results.Ok(ApiResponse<IReadOnlyList<HandoverInvitationDto>>.Ok(response));
    }
}
