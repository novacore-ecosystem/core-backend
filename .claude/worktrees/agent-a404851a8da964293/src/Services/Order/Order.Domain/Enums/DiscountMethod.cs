namespace NovaCore.Order.Domain.Enums;

public enum DiscountMethod : short
{
    /// <summary>Discount is a fixed amount, e.g. $10 off.</summary>
    FixedAmount = 1,

    /// <summary>Discount is a percentage of the original price, e.g. 10% off.</summary>
    Percentage = 2,

    /// <summary>Discount is a fixed price for the item, e.g. $5 for this product.</summary>
    FixedPrice = 3,

    /// <summary>Discount is for free shipping.</summary>
    FreeShipping = 4,

    /// <summary>Discount is for a "buy X get Y" offer.</summary>
    BuyXGetY = 5,

    /// <summary>Discount is for a product bundle.</summary>
    Bundle = 6,

    /// <summary>Discount is for a gift item.</summary>
    Gift = 7,
}
