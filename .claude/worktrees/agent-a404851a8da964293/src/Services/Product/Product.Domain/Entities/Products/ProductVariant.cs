using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.Product.Domain.Entities.Options;
using NovaCore.Product.Domain.Entities.Products.Data;
using NovaCore.Product.Domain.Entities.Warranties;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product,
/// referenced back via ProductVariant.ProductId - the full set of variants for a product.
/// A Variant is identified only by its selected ProductOptionValues (via
/// <see cref="SelectedOptionValues"/>); it never owns or duplicates option data itself.
/// </summary>
public sealed class ProductVariant : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public ProductVariantType VariantType { get; private set; } = ProductVariantType.Simple;
    public Guid IdentifierId { get; private set; }
    public ProductIdentifier Identifier { get; private set; } = default!;
    public Weight? Weight { get; private set; }
    public Guid? ProductShippingId { get; private set; }
    public ProductShipping? Shipping { get; private set; } = default!;
    public Guid? WarrantyPolicyId { get; private set; }
    public WarrantyPolicy? WarrantyPolicy { get; private set; } = default!;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    public VariantMetadata Metadata { get; private set; } = new();
    public ICollection<VariantOptionValue> SelectedOptionValues { get; private set; } = [];
    public ICollection<ProductBundleItem> BundleItems { get; private set; } = [];
    public ICollection<ProductVariantTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductVariant() { }

    /// <summary>
    /// Creates a Variant from the Product-validated set of selected OptionValue IDs - the caller
    /// (Product) is responsible for verifying every ID belongs to the product and that the
    /// resulting combination is not a duplicate; this factory only builds the mappings.
    /// </summary>
    internal static ProductVariant Create(
        Guid productId,
        Sku sku,
        string name,
        int displayOrder,
        IEnumerable<Guid> selectedOptionValueIds,
        Barcode? barcode = null,
        Weight? weight = null,
        bool isActive = true,
        VariantMetadata? metadata = null,
        IEnumerable<CreateBundleItemData>? bundleItems = null)
    {
        ValidateName(name);

        var identifier = ProductIdentifier.Create(
            productId,
            sku,
            barcode);

        var variant = new ProductVariant
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            Name = name,
            Identifier = identifier,
            Weight = weight,
            IsActive = isActive,
            DisplayOrder = displayOrder,
            Metadata = metadata ?? new VariantMetadata(),
            VariantType = bundleItems != null && bundleItems.Any()
                ? ProductVariantType.Bundle
                : ProductVariantType.Simple,
        };
        variant.AssignOptionValues(selectedOptionValueIds);
        variant.SetBundleItems(bundleItems);

        return variant;
    }

    // ============================================================================
    // Translations
    // Owns the locale-specific ProductVariantTranslation collection for this variant:
    // creation, updates, and SEO overrides. ProductVariantTranslation is never constructed
    // outside this entity.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string name,
        string shortDescription,
        string description,
        string? seoTitle = null,
        string? seoDescription = null)
    {
        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateContent(name, shortDescription, description);
            existingTranslation.UpdateSeo(seoTitle, seoDescription);
            return;
        }

        var variantTranslation = ProductVariantTranslation.Create(
            Id,
            languageCode,
            name,
            shortDescription,
            description,
            seoTitle,
            seoDescription);
        Translations.Add(variantTranslation);
    }

    #endregion

    // ============================================================================
    // Option Values
    // Builds the SelectedOptionValues mapping collection this Variant is
    // identified by. Assigned once at creation from Product-validated
    // OptionValue IDs; never mutated afterward or used to create/modify
    // ProductOptionValues themselves.
    // ============================================================================

    #region Option Values

    private void AssignOptionValues(IEnumerable<Guid> optionValueIds)
    {
        var orderedIds = optionValueIds.ToArray();

        for (var i = 0; i < orderedIds.Length; i++)
            SelectedOptionValues.Add(VariantOptionValue.Create(Id, orderedIds[i], i));
    }

    #endregion

    // ============================================================================
    // Bundle Items
    // Manages the child variants and quantities that make up a Bundle-type
    // variant. Enforces that a Bundle variant always has at least one item.
    // ============================================================================

    #region Bundle Items

    public void SetBundleItems(IEnumerable<CreateBundleItemData>? bundleItems)
    {
        if (BundleItems.Count > 0 && (bundleItems is null || !bundleItems.Any()))
            throw ExceptionFactory.InvalidState("Cannot clear bundle items for a bundle variant.");

        if (VariantType == ProductVariantType.Bundle && (bundleItems is null || !bundleItems.Any()))
            throw ExceptionFactory.InvalidState("A bundle variant must have at least one bundle item.");

        foreach (var item in bundleItems ?? [])
        {
            if (item.VariantId != Id)
                throw ExceptionFactory.InvalidState("Bundle item variant ID does not match the parent bundle variant ID.");

            var bundleItem = ProductBundleItem.Create(
                item.BundleVariantId,
                item.VariantId,
                item.ProductId,
                item.Quantity);
            BundleItems.Add(bundleItem);
        }
    }

    #endregion

    // ============================================================================
    // Shipping & Warranty
    // Assigns or clears the optional Shipping profile and Warranty policy
    // this variant is linked to.
    // ============================================================================

    #region Shipping & Warranty

    public void AssignShipping(Guid productShippingId)
    {
        ProductShippingId = productShippingId;
    }

    public void ClearShipping()
    {
        ProductShippingId = null;
    }

    public void AssignWarranty(Guid warrantyPolicyId)
    {
        WarrantyPolicyId = warrantyPolicyId;
    }

    public void ClearWarranty()
    {
        WarrantyPolicyId = null;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields (name, identifiers, weight, display order),
    // Default/Active status transitions, metadata updates, and the shared
    // name-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    public void UpdateIdentifiers(Sku sku, Barcode? barcode)
    {
        Identifier.UpdateCoreIdentifiers(sku, barcode);
    }

    public void UpdateWeight(Weight? weight)
    {
        Weight = weight;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void MarkAsDefault()
    {
        IsDefault = true;
    }

    public void UnmarkAsDefault()
    {
        IsDefault = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateMetadata(VariantMetadata metadata)
    {
        Metadata = metadata;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Variant name cannot be empty.");
    }

    #endregion
}
