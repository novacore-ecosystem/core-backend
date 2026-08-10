using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;
using NovaCore.BuildingBlock.Application.DeadLetters.Queries;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace NovaCore.BuildingBlock.Web.Endpoints.DeadLetters;

public sealed class GetDeadLetter : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/deadletters/{id:guid}", GetById)
            .WithTags("DeadLetter")
            .RequirePermissions(Permissions.System.MessagingView)
            .WithName("GetDeadLetter")
            .WithDisplayName("Get Dead Letter API")
            .WithDescription("Full detail for one dead-lettered row, including its retry history.")
            .Produces<ApiResponse<DeadLetterDetailResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new GetDeadLetterQuery(id), ct);
        return Results.Ok(ApiResponse<DeadLetterDetailResponse>.Ok(response));
    }
}
