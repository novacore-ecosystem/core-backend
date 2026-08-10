namespace NovaCore.Payment.Domain.Entities.Accounts;

/// <summary>Gateway tokenization record for a PaymentAccount - a given account may be tokenized separately per gateway. Only PaymentAccount may construct one.</summary>
public sealed class PaymentToken : BaseEntity<Guid>, IAuditable
{
    public Guid PaymentAccountId { get; private set; }
    public Guid GatewayId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string TokenType { get; private set; } = string.Empty;
    public DateTime? ExpiresAt { get; private set; }

    private PaymentToken() { }

    internal static PaymentToken Create(Guid paymentAccountId, Guid gatewayId, string token, string tokenType, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw ExceptionFactory.RequiredField("Gateway token cannot be empty.");

        if (string.IsNullOrWhiteSpace(tokenType))
            throw ExceptionFactory.RequiredField("Gateway token type cannot be empty.");

        return new PaymentToken
        {
            Id = Guid.CreateVersion7(),
            PaymentAccountId = paymentAccountId,
            GatewayId = gatewayId,
            Token = token.Trim(),
            TokenType = tokenType.Trim(),
            ExpiresAt = expiresAt,
        };
    }
}
