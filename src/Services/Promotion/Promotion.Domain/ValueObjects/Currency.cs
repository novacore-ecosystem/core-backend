using System.Text.RegularExpressions;

namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>
/// Local 3-letter ISO-4217 currency code, paired with the currency-less Money Value Object
/// wherever an amount needs a currency (Promotion, CampaignBudget, BundlePrice, Voucher) - same
/// split Payment Service's own Money/Currency documents. Local to this project since
/// Payment.Domain's Currency lives in a different microservice and cannot be referenced across
/// service boundaries.
/// </summary>
public sealed partial class Currency : StringValueObject
{
    private Currency(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out Currency? currency)
    {
        if (GetValidationError(value) is not null)
        {
            currency = null;
            return false;
        }

        currency = new Currency(Normalize(value!));
        return true;
    }

    public static Currency Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new Currency(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Currency cannot be empty.");

        if (!CurrencyFormat().IsMatch(Normalize(value)))
            return ExceptionFactory.InvalidFormat("Currency must be a 3-letter ISO-4217 code.");

        return null;
    }

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyFormat();
}
