using NovaCore.BuildingBlock.SharedKernel.RegexPatterns;
using NovaCore.BuildingBlock.SharedKernel.Utilities;

namespace NovaCore.Product.Domain.ValueObjects;

/// <summary>EAN/UPC/GTIN barcode - 8 to 14 numeric digits.</summary>
public sealed class Barcode : StringValueObject
{
    private Barcode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out Barcode? barcode)
    {
        if (GetValidationError(value) is not null)
        {
            barcode = null;
            return false;
        }

        barcode = new Barcode(value!.Trim());
        return true;
    }

    public static Barcode Create()
    {
        var newCode = NativeBarcodeGenerator.GenerateEan13();
        return Create(newCode);
    }
    public static Barcode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new Barcode(value.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Barcode cannot be empty.");

        if (!RegexPatterns.BarcodeFormat().IsMatch(value.Trim()))
            return ExceptionFactory.InvalidFormat("Barcode must be 8-14 numeric digits (EAN/UPC/GTIN).");

        return null;
    }
}
