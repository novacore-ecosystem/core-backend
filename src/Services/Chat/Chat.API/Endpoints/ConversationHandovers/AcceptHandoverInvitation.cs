using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationHandovers.Commands.AcceptHandoverInvitation;

namespace NovaCore.Chat.API.Endpoints.ConversationHandovers;

public sealed class AcceptHandoverInvitationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Accept Handover Invitation",
        "",
        "Accepts a pending handover request addressed to the caller - releases the conversation's",
        "current assignment (if any) and creates a new one for the caller.",
        "",
        "### Error Responses",
        "- **400**: Request is not pending",
        "- **403**: Invitation was not addressed to the caller",
        "- **404**: Invitation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/handovers/{transferRequestId}/accept", Handle)
            .WithTags("ConversationHandovers")
            .RequireAuthorization()
            .WithName("AcceptHandoverInvitation")
            .WithDisplayName("Accept Handover Invitation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid transferRequestId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new AcceptHandoverInvitationCommand(transferRequestId), ct);

        return Results.NoContent();
    }
}
