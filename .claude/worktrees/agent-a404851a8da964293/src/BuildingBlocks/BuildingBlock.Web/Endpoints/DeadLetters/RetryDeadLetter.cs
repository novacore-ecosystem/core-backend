using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.DeadLetters.Commands;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace NovaCore.BuildingBlock.Web.Endpoints.DeadLetters;

public sealed class RetryDeadLetter : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/deadletters/{id:guid}/retry", RetryOne)
            .WithTags("DeadLetter")
            .RequirePermissions(Permissions.System.MessagingRequeue)
            .WithName("RetryDeadLetter")
            .WithDisplayName("Retry Dead Letter API")
            .WithDescription("Requeues one dead-lettered message and republishes it through the normal Kafka pipeline.")
            .Produces<ApiResponse<RetryDeadLetterResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RetryOne(
        [FromRoute] Guid id, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new RetryDeadLetterCommand(id), ct);
        return Results.Ok(ApiResponse<RetryDeadLetterResponse>.Ok(response));
    }
}
