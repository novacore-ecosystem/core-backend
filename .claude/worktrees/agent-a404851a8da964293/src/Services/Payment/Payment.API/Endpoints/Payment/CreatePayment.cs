using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Infrastructure.Idempotency;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using NovaCore.Payment.Application.Features.Payments.Commands.CreatePayment;

namespace NovaCore.Payment.API.Endpoints.Payment;

public sealed record CreatePaymentRequest(
    ReferenceType ReferenceType,
    Guid ReferenceId,
    decimal Amount,
    string CurrencyCode,
    Guid GatewayId,
    Guid? PaymentIntentId,
    Guid? PaymentMethodId,
    DateTime? ExpiresAt,
    string? Metadata);

public sealed class CreatePaymentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Payment",
        "",
        "Creates a Payment against a business reference (ReferenceType + ReferenceId). This is the",
        "single entry point every consumer module (Order, Subscription, Wallet, Booking, ...) uses",
        "to charge money - PaymentService never depends on the referenced module's own data.",
        "",
        "### Request Body",
        "- **ReferenceType/ReferenceId**: The business module and its own identifier for what is being paid for (required)",
        "- **Amount/CurrencyCode**: Money amount and ISO-4217 currency code (required)",
        "- **GatewayId**: The PaymentGateway this payment will be processed through (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments", Handle)
            .WithTags("Payment")
            .RequireAuthorization()
            .Headers([
                new HeaderDefinition(HeaderKeyConstant.IdempotencyKey, true, "Ensures this payment is only created once, even if the request is retried")
            ])
            .RequireIdempotency()
            .WithName("CreatePayment")
            .WithDisplayName("Create Payment API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreatePaymentResponse>>(StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreatePaymentRequest request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var command = new CreatePaymentCommand(
            request.ReferenceType,
            request.ReferenceId,
            request.Amount,
            request.CurrencyCode,
            request.GatewayId,
            request.PaymentIntentId,
            request.PaymentMethodId,
            currentUser.GetIdempotencyKey(),
            request.ExpiresAt,
            request.Metadata);

        var response = await sender.Send(command, ct);

        return Results.Accepted($"/payments/{response.PaymentId}", ApiResponse<CreatePaymentResponse>.Ok(response));
    }
}
