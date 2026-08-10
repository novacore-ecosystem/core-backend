using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Infrastructure.Idempotency;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using NovaCore.Payment.Application.Features.PaymentIntents.Commands.CreatePaymentIntent;

namespace NovaCore.Payment.API.Endpoints.PaymentIntent;

public sealed record CreatePaymentIntentRequest(
    ReferenceType ReferenceType,
    Guid ReferenceId,
    decimal RequestedAmount,
    string CurrencyCode,
    DateTime? ExpiresAt,
    string? Metadata);

public sealed class CreatePaymentIntentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Payment Intent",
        "",
        "Stripe-style entry point for the checkout/redirect flow: PaymentIntent -> Payment ->",
        "PaymentAttempt. Records what the caller wants to happen before any gateway-facing Payment",
        "exists.",
        "",
        "### Request Body",
        "- **ReferenceType/ReferenceId**: The business module and its own identifier for what is being paid for (required)",
        "- **RequestedAmount/CurrencyCode**: Money amount and ISO-4217 currency code (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payment-intents", Handle)
            .WithTags("PaymentIntent")
            .RequireAuthorization()
            .Headers([
                new HeaderDefinition(HeaderKeyConstant.IdempotencyKey, true, "Ensures this payment intent is only created once, even if the request is retried")
            ])
            .RequireIdempotency()
            .WithName("CreatePaymentIntent")
            .WithDisplayName("Create Payment Intent API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreatePaymentIntentResponse>>(StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreatePaymentIntentRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreatePaymentIntentCommand(
            request.ReferenceType,
            request.ReferenceId,
            request.RequestedAmount,
            request.CurrencyCode,
            request.ExpiresAt,
            request.Metadata);

        var response = await sender.Send(command, ct);

        return Results.Accepted($"/payment-intents/{response.PaymentIntentId}", ApiResponse<CreatePaymentIntentResponse>.Ok(response));
    }
}
