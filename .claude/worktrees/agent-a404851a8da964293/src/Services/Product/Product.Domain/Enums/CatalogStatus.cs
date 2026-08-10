namespace NovaCore.Product.Domain.Enums;

/// <summary>Shared Active/Inactive lifecycle status reused by every catalog lookup entity (options, attributes, brands, collections, warranties, recommendations).</summary>
public enum CatalogStatus : short
{
    Active = 1,
    Inactive = 2,
}
