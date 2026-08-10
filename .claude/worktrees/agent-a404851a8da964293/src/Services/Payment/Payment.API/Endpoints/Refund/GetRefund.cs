using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Payment.Application.Features.Refunds.Queries.GetRefund;

namespace NovaCore.Payment.API.Endpoints.Refund;

public sealed class GetRefundEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Refund Details",
        "",
        "Retrieves a Refund by id.",
        "",
        "### Route Parameters",
        "- **refundId**: Unique identifier of the refund (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Refund not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/refunds/{refundId}", Handle)
            .WithTags("Refund")
            .RequireAuthorization()
            .WithName("GetRefund")
            .WithDisplayName("Get Refund API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetRefundResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid refundId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetRefundQuery(refundId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetRefundResponse>.Ok(response));
    }
}
