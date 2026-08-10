namespace NovaCore.Product.Domain.Enums;

public enum ProductVariantType : byte
{
    /// <summary>The variant is a simple product variant.</summary>
    Simple = 0,

    /// <summary>The variant is a bundle product variant.</summary>
    Bundle = 1,
}
