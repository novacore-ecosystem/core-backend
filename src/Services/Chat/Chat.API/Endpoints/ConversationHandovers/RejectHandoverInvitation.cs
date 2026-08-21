using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationHandovers.Commands.RejectHandoverInvitation;

namespace NovaCore.Chat.API.Endpoints.ConversationHandovers;

public sealed class RejectHandoverInvitationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Reject Handover Invitation",
        "",
        "Rejects a pending handover request addressed to the caller.",
        "",
        "### Error Responses",
        "- **400**: Request is not pending",
        "- **403**: Invitation was not addressed to the caller",
        "- **404**: Invitation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/handovers/{transferRequestId}/reject", Handle)
            .WithTags("ConversationHandovers")
            .RequireAuthorization()
            .WithName("RejectHandoverInvitation")
            .WithDisplayName("Reject Handover Invitation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid transferRequestId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new RejectHandoverInvitationCommand(transferRequestId), ct);

        return Results.NoContent();
    }
}
