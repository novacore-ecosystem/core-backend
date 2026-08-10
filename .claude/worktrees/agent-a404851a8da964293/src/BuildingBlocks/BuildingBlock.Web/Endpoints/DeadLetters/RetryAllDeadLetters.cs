using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.DeadLetters.Commands;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace NovaCore.BuildingBlock.Web.Endpoints.DeadLetters;

public sealed class RetryAllDeadLetters : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/deadletters/retry-all", RetryAll)
            .WithTags("DeadLetter")
            .RequirePermissions(Permissions.System.MessagingRequeue)
            .WithName("RetryAllDeadLetters")
            .WithDisplayName("Retry All Dead Letters API")
            .WithDescription("Retries every DeadLetter row matching an optional filter, capped at 500 per call.")
            .Produces<ApiResponse<RetryDeadLettersSummary>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RetryAll(
        [FromBody] CriteriaRequest? filter, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new RetryAllDeadLettersCommand(filter), ct);
        return Results.Ok(ApiResponse<RetryDeadLettersSummary>.Ok(response));
    }
}
