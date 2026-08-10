namespace NovaCore.Payment.Domain.Entities.Catalogs;

/// <summary>
/// Catalog of supported payment methods (Visa, MasterCard, VNPay, MoMo, PayPal, Apple Pay,
/// Google Pay, ...). Reference/lookup data seeded via migration, not a business workflow entity.
/// This is NOT UserPaymentMethod (a user's tokenized account) - see PaymentAccount for that.
/// </summary>
public sealed class PaymentMethod : AggregateRoot<Guid>, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public PaymentMethodType MethodType { get; private set; }
    public string? IconUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    private PaymentMethod() { }

    public static PaymentMethod Create(Guid id, string code, string name, PaymentMethodType methodType, string? iconUrl = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw ExceptionFactory.RequiredField("Payment method code cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Payment method name cannot be empty.");

        return new PaymentMethod
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            MethodType = methodType,
            IconUrl = iconUrl,
            IsActive = true,
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
