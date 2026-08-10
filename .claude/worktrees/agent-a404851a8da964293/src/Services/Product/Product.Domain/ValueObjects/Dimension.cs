namespace NovaCore.Product.Domain.ValueObjects;

/// <summary>Physical shipping dimensions, expressed in a single unit shared by all three measurements.</summary>
public sealed class Dimension : ValueObject
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public DimensionUnit Unit { get; }

    private Dimension(decimal length, decimal width, decimal height, DimensionUnit unit)
    {
        Length = length;
        Width = width;
        Height = height;
        Unit = unit;
    }

    public static bool IsValid(decimal length, decimal width, decimal height) =>
        GetValidationError(length, width, height) is null;

    public static bool TryCreate(decimal length, decimal width, decimal height, DimensionUnit unit, out Dimension? dimension)
    {
        if (GetValidationError(length, width, height) is not null)
        {
            dimension = null;
            return false;
        }

        dimension = new Dimension(length, width, height, unit);
        return true;
    }

    public static Dimension Create(decimal length, decimal width, decimal height, DimensionUnit unit)
    {
        var error = GetValidationError(length, width, height);
        if (error is not null)
            throw error;

        return new Dimension(length, width, height, unit);
    }

    private static InvalidArgumentException? GetValidationError(decimal length, decimal width, decimal height)
    {
        if (length <= 0 || width <= 0 || height <= 0)
            return ExceptionFactory.InvalidRange("Dimensions must all be greater than zero.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Length;
        yield return Width;
        yield return Height;
        yield return Unit;
    }

    public override string ToString() => $"{Length}x{Width}x{Height} {Unit}";
}
