using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Infrastructure.Idempotency;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using NovaCore.Payment.Application.Features.Refunds.Commands.CreateRefund;

namespace NovaCore.Payment.API.Endpoints.Refund;

public sealed record CreateRefundRequest(
    Guid PaymentId,
    decimal Amount,
    string CurrencyCode,
    string Reason);

public sealed class CreateRefundEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Refund",
        "",
        "Creates a Refund against an existing Payment. A Payment may have multiple refunds",
        "(e.g. partial refunds).",
        "",
        "### Request Body",
        "- **PaymentId**: The Payment being refunded (required, must exist)",
        "- **Amount/CurrencyCode**: Refund amount and ISO-4217 currency code (required)",
        "- **Reason**: Free-text refund reason (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: Payment not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/refunds", Handle)
            .WithTags("Refund")
            .RequireAuthorization()
            .Headers([
                new HeaderDefinition(HeaderKeyConstant.IdempotencyKey, true, "Ensures this refund is only created once, even if the request is retried")
            ])
            .RequireIdempotency()
            .WithName("CreateRefund")
            .WithDisplayName("Create Refund API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateRefundResponse>>(StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateRefundRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateRefundCommand(request.PaymentId, request.Amount, request.CurrencyCode, request.Reason);

        var response = await sender.Send(command, ct);

        return Results.Accepted($"/refunds/{response.RefundId}", ApiResponse<CreateRefundResponse>.Ok(response));
    }
}
