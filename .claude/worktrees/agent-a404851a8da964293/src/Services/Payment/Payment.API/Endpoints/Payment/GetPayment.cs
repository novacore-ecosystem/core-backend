using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Payment.Application.Features.Payments.Queries.GetPayment;

namespace NovaCore.Payment.API.Endpoints.Payment;

public sealed class GetPaymentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Payment Details",
        "",
        "Retrieves a Payment (including its breakdown items and attempts) by id.",
        "",
        "### Route Parameters",
        "- **paymentId**: Unique identifier of the payment (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Payment not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/payments/{paymentId}", Handle)
            .WithTags("Payment")
            .RequireAuthorization()
            .WithName("GetPayment")
            .WithDisplayName("Get Payment API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetPaymentResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid paymentId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetPaymentQuery(paymentId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetPaymentResponse>.Ok(response));
    }
}
