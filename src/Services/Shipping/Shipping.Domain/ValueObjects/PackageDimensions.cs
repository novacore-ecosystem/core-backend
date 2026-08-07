namespace NovaCore.Shipping.Domain.ValueObjects;

/// <summary>Outer Length/Width/Height of a Package in centimetres - the input to dimensional (volumetric) weight pricing.</summary>
public sealed class PackageDimensions : ValueObject
{
    public decimal LengthCm { get; }
    public decimal WidthCm { get; }
    public decimal HeightCm { get; }

    private PackageDimensions(decimal lengthCm, decimal widthCm, decimal heightCm)
    {
        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
    }

    public static PackageDimensions Create(decimal lengthCm, decimal widthCm, decimal heightCm)
    {
        if (!IsValid(lengthCm, widthCm, heightCm))
            throw ExceptionFactory.InvalidRange("Package dimensions must all be greater than zero.");

        return new PackageDimensions(lengthCm, widthCm, heightCm);
    }

    public static bool IsValid(decimal lengthCm, decimal widthCm, decimal heightCm)
        => lengthCm > 0 && widthCm > 0 && heightCm > 0;

    public decimal VolumeCm3 => LengthCm * WidthCm * HeightCm;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return LengthCm;
        yield return WidthCm;
        yield return HeightCm;
    }

    public override string ToString() => $"{LengthCm}x{WidthCm}x{HeightCm}cm";
}
