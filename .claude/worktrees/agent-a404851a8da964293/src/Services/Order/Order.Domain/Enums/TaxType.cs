namespace NovaCore.Order.Domain.Enums;

public enum TaxType : byte
{
    /// <summary>Value-added tax.</summary>
    Vat = 1,

    /// <summary>Point-of-sale tax charged as a percentage of the sale.</summary>
    SalesTax = 2,

    /// <summary>Goods and services tax.</summary>
    Gst = 3,

    /// <summary>Any jurisdiction-specific tax not covered by the above.</summary>
    Custom = 4
}
