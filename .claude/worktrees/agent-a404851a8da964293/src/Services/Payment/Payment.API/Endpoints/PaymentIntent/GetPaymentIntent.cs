using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Payment.Application.Features.PaymentIntents.Queries.GetPaymentIntent;

namespace NovaCore.Payment.API.Endpoints.PaymentIntent;

public sealed class GetPaymentIntentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Payment Intent Details",
        "",
        "Retrieves a PaymentIntent by id.",
        "",
        "### Route Parameters",
        "- **paymentIntentId**: Unique identifier of the payment intent (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Payment intent not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/payment-intents/{paymentIntentId}", Handle)
            .WithTags("PaymentIntent")
            .RequireAuthorization()
            .WithName("GetPaymentIntent")
            .WithDisplayName("Get Payment Intent API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetPaymentIntentResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid paymentIntentId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetPaymentIntentQuery(paymentIntentId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetPaymentIntentResponse>.Ok(response));
    }
}
