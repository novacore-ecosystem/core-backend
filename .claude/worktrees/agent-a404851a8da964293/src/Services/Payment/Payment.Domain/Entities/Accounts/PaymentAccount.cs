namespace NovaCore.Payment.Domain.Entities.Accounts;

/// <summary>
/// A user-linked payment account (card, bank, PayPal, wallet, Apple Pay, Google Pay, ...).
/// PaymentService owns this data - UserService should only keep a reference (mirrors how
/// Order.Domain.OrderPayment only keeps a reference into PaymentService). Never stores real
/// PAN/CVV - only tokens, masked numbers, holder names, expiration, issuer.
/// </summary>
public sealed class PaymentAccount : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid OwnerReferenceId { get; private set; }
    public PaymentAccountType AccountType { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string? MaskedNumber { get; private set; }
    public string? HolderName { get; private set; }
    public int? ExpirationMonth { get; private set; }
    public int? ExpirationYear { get; private set; }
    public string? Issuer { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsVerified { get; private set; }
    public string? Metadata { get; private set; }

    public ICollection<PaymentToken> Tokens { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PaymentAccount() { }

    public static PaymentAccount Create(
        Guid ownerReferenceId,
        PaymentAccountType accountType,
        string token,
        string? maskedNumber = null,
        string? holderName = null,
        int? expirationMonth = null,
        int? expirationYear = null,
        string? issuer = null,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw ExceptionFactory.RequiredField("Payment account token cannot be empty.");

        return new PaymentAccount
        {
            Id = Guid.CreateVersion7(),
            OwnerReferenceId = ownerReferenceId,
            AccountType = accountType,
            Token = token.Trim(),
            MaskedNumber = maskedNumber,
            HolderName = holderName,
            ExpirationMonth = expirationMonth,
            ExpirationYear = expirationYear,
            Issuer = issuer,
            IsDefault = false,
            IsVerified = false,
            Metadata = metadata,
        };
    }

    public void MarkAsDefault() => IsDefault = true;

    public void UnmarkAsDefault() => IsDefault = false;

    public void MarkVerified() => IsVerified = true;

    /// <summary>Only PaymentAccount may construct a PaymentToken (see PaymentToken.Create being internal).</summary>
    public PaymentToken AddToken(Guid gatewayId, string token, string tokenType, DateTime? expiresAt = null)
    {
        var paymentToken = PaymentToken.Create(Id, gatewayId, token, tokenType, expiresAt);
        Tokens.Add(paymentToken);
        return paymentToken;
    }
}
