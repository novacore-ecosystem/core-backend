using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.Product.Domain.Entities.Brands;
using NovaCore.Product.Domain.Entities.Options;
using NovaCore.Product.Domain.Entities.Products.Data;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Aggregate root of the catalog. Holds identity, display copy, lifecycle/status, and ownership
/// of every product-scoped child (variants, options, specifications, media, translations, and
/// lookup memberships). Pricing, stock, promotions, and shipping-fee calculation are
/// intentionally absent - those belong to their own services.
/// </summary>
public sealed class Product : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string ShortDescription { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid? BrandId { get; private set; }
    public ProductBrand? Brand { get; private set; }
    public Guid? CategoryId { get; private set; }
    public ProductType ProductType { get; private set; }
    public ProductStatus ProductStatus { get; private set; }
    public Guid? SeoId { get; private set; }
    public ProductSeo? Seo { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public bool IsFeatured { get; private set; }
    public bool IsVisible { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    public ProductMetadata Metadata { get; private set; } = new();
    public ICollection<ProductOption> Options { get; private set; } = [];
    public ICollection<ProductSpecification> Specifications { get; private set; } = [];
    public ICollection<ProductMedia> Media { get; private set; } = [];
    public ICollection<ProductTranslation> Translations { get; private set; } = [];
    public ICollection<ProductVariant> Variants { get; private set; } = [];
    public ICollection<ProductCategoryMapping> CategoryMappings { get; private set; } = [];
    public ICollection<ProductTagMapping> TagMappings { get; private set; } = [];
    public ICollection<ProductCollectionMapping> CollectionMappings { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Product() { }

    /// <summary>
    /// Creates a Product together with its required variants in one call - aggregate
    /// initialization is naturally a bulk operation, so the caller just supplies the collection
    /// and every invariant (at least one variant, exactly one Default, DisplayOrder) is
    /// enforced and initialized here rather than split across the caller.
    /// </summary>
    public static Product Create(
        string name,
        string shortDescription,
        string description,
        Slug slug,
        ProductType productType,
        IEnumerable<CreateVariantData> variants,
        Guid? brandId = null,
        IEnumerable<Guid>? categoryIds = null,
        IEnumerable<Guid>? tagIds = null,
        ProductStatus status = ProductStatus.Draft,
        bool isFeatured = false,
        bool isVisible = true,
        int displayOrder = 0,
        ProductMetadata? metadata = null)
    {
        ValidateName(name);

        var product = new Product
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            ShortDescription = shortDescription,
            Description = description,
            Slug = slug,
            BrandId = brandId,
            ProductType = productType,
            ProductStatus = status,
            IsFeatured = isFeatured,
            IsVisible = isVisible,
            DisplayOrder = displayOrder,
            Metadata = metadata ?? new ProductMetadata(),
        };
        product.SetVariants(variants);
        product.SetTags(tagIds ?? []);
        product.SetCategories(categoryIds ?? []);

        return product;
    }

    // ============================================================================
    // Product Variants
    // Manages the owned Variant collection: add, remove, and switch which
    // variant is the Default. Enforces the "at least one, exactly one Default"
    // invariant across the collection.
    // ============================================================================

    #region Product Variants

    /// <summary>
    /// Removes a variant. The last remaining variant can never be removed. Removing the
    /// current Default auto-promotes the remaining variant with the lowest DisplayOrder, so
    /// "exactly one Default" holds at every method boundary without an extra caller round-trip.
    /// </summary>
    public void RemoveVariant(Guid variantId)
    {
        if (Variants.Count <= 1)
            throw ExceptionFactory.InvalidState("Cannot remove the last variant of a product.");

        var variant = Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw ExceptionFactory.EntityNotFound<ProductVariant>(variantId);

        var wasDefault = variant.IsDefault;
        Variants.Remove(variant);

        if (wasDefault)
        {
            var successor = Variants.OrderBy(v => v.DisplayOrder).First();
            successor.MarkAsDefault();
        }
    }

    public void SetVariants(IEnumerable<CreateVariantData> variants)
    {
        if (Variants.Count > 0)
            throw ExceptionFactory.InvalidState("Cannot set variants on a product that already has variants.");

        var variantList = variants.ToArray();
        if (variantList.Length == 0)
            throw ExceptionFactory.EmptyCollection("A product must have at least one variant.");

        if (variantList.Count(v => v.IsDefault) != 1)
            throw ExceptionFactory.InvalidState("Exactly one variant must be marked as default.");

        foreach (var variant in variantList)
            AddVariant(variant);
    }

    public void AddVariant(CreateVariantData variant)
    {
        if (Variants.Any(v => v.Id == variant.ProductId))
            throw ExceptionFactory.Duplicate("A variant with the same ID already exists.");

        if (variant.IsDefault && Variants.Any(v => v.IsDefault))
            throw ExceptionFactory.InvalidState("Cannot add a new default variant when one already exists.");

        var selectedOptionValueIds = (variant.SelectedOptionValueIds ?? []).ToArray();
        ValidateOptionValueSelection(selectedOptionValueIds);

        var variantEntity = ProductVariant.Create(
            productId: Id,
            sku: variant.Sku,
            name: variant.Name,
            displayOrder: variant.DisplayOrder,
            selectedOptionValueIds: selectedOptionValueIds,
            barcode: variant.Barcode,
            weight: variant.Weight,
            isActive: variant.IsActive,
            metadata: variant.Metadata,
            bundleItems: variant.BundleItems);
        Variants.Add(variantEntity);
    }

    /// <summary>
    /// Enforces the OptionValue selection rules for a Variant being added: every ID must belong
    /// to one of this Product's Options, the combination must be unique among existing Variants,
    /// a Variant needs at least one OptionValue once the Product has Options, and a Product
    /// without Options may only ever hold a single (default) Variant.
    /// </summary>
    private void ValidateOptionValueSelection(IReadOnlyCollection<Guid> selectedOptionValueIds)
    {
        if (Options.Count == 0)
        {
            if (selectedOptionValueIds.Count > 0)
                throw ExceptionFactory.InvalidState("Cannot select option values for a product with no options.");

            if (Variants.Count > 0)
                throw ExceptionFactory.InvalidState("A product with no options can only have a single variant.");

            return;
        }

        if (selectedOptionValueIds.Count == 0)
            throw ExceptionFactory.EmptyCollection("A variant must select at least one option value.");

        if (selectedOptionValueIds.Distinct().Count() != selectedOptionValueIds.Count)
            throw ExceptionFactory.Duplicate("A variant cannot select the same option value more than once.");

        var validOptionValueIds = Options.SelectMany(o => o.Values).Select(v => v.Id).ToHashSet();
        if (selectedOptionValueIds.Any(id => !validOptionValueIds.Contains(id)))
            throw ExceptionFactory.InvalidState("One or more selected option values do not belong to this product.");

        var combination = selectedOptionValueIds.ToHashSet();
        var isDuplicateCombination = Variants.Any(v =>
            v.SelectedOptionValues.Count == combination.Count
            && v.SelectedOptionValues.All(o => combination.Contains(o.ProductOptionValueId)));

        if (isDuplicateCombination)
            throw ExceptionFactory.Duplicate("A variant with the same option value combination already exists.");
    }

    /// <summary>Switches the Default variant. No-op if it already is the default.</summary>
    public void SetDefaultVariant(Guid variantId)
    {
        var target = Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw ExceptionFactory.EntityNotFound<ProductVariant>(variantId);

        if (target.IsDefault)
            return;

        foreach (var variant in Variants.Where(v => v.IsDefault))
            variant.UnmarkAsDefault();

        target.MarkAsDefault();
    }

    #endregion

    // ============================================================================
    // Options
    // Manages the ProductOption collection defining the configurable choices
    // (e.g. size, color) available for this product's variants.
    // ============================================================================

    #region Options

    public void AddOption(ProductOption option)
    {
        if (Options.Any(o => o.Id == option.Id))
            return;

        Options.Add(option);
    }

    public void RemoveOption(Guid optionId)
    {
        var option = Options.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            return;

        Options.Remove(option);
    }

    #endregion

    // ============================================================================
    // Specifications
    // Manages the technical specification entries (name/value pairs) shown
    // on the product detail page.
    // ============================================================================

    #region Specifications

    public void AddSpecification(ProductSpecification specification)
    {
        Specifications.Add(specification);
    }

    public void RemoveSpecification(Guid specificationId)
    {
        var specification = Specifications.FirstOrDefault(s => s.Id == specificationId);
        if (specification is null)
            return;

        Specifications.Remove(specification);
    }

    #endregion

    // ============================================================================
    // Media
    // Manages the product's image/video gallery entries.
    // ============================================================================

    #region Media

    public void AddMedia(ProductMedia media)
    {
        Media.Add(media);
    }

    public void RemoveMedia(Guid mediaId)
    {
        var media = Media.FirstOrDefault(m => m.Id == mediaId);
        if (media is null)
            return;

        Media.Remove(media);
    }

    #endregion

    // ============================================================================
    // Translations
    // Manages per-language copy overrides (name/description) keyed by
    // language code, one entry per locale.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string name,
        string shortDescription,
        string description)
    {
        ValidateName(name);

        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateContent(name, shortDescription, description);
            return;
        }

        var translation = ProductTranslation.Create(
            Id,
            languageCode,
            name,
            shortDescription,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Collections, Categories & Tags
    // Manages the product's many-to-many membership mappings for merchandising
    // collections, taxonomy categories, and free-form tags. Enforces
    // no-duplicate-membership invariants per mapping type.
    // ============================================================================

    #region Collections, Categories & Tags

    public void SetCollections(IEnumerable<Guid> collectionIds)
    {
        if (CollectionMappings.Count > 0)
            throw ExceptionFactory.InvalidState("Cannot set collections on a product that already has collections.");

        var collectionIdList = collectionIds.ToArray();
        if (collectionIdList.Distinct().Count() != collectionIdList.Length)
            throw ExceptionFactory.Duplicate("One or more collections are duplicated.");

        foreach (var collectionId in collectionIdList)
        {
            var collectionMapping = ProductCollectionMapping.Create(
                Id,
                collectionId);
            CollectionMappings.Add(collectionMapping);
        }
    }

    public void AssignCollection(Guid collectionId, int displayOrder = 0)
    {
        if (CollectionMappings.Any(m => m.CollectionId == collectionId))
            return;

        var collectionMapping = ProductCollectionMapping.Create(
            Id,
            collectionId,
            displayOrder);
        CollectionMappings.Add(collectionMapping);
    }

    public void RemoveCollection(Guid collectionId)
    {
        var mapping = CollectionMappings.FirstOrDefault(m => m.CollectionId == collectionId);
        if (mapping is null)
            return;

        CollectionMappings.Remove(mapping);
    }

    public void SetCategories(IEnumerable<Guid> categoryIds)
    {
        if (CategoryMappings.Count > 0)
            throw ExceptionFactory.InvalidState("Cannot set categories on a product that already has categories.");

        var categoryIdList = categoryIds.ToArray();
        if (categoryIdList.Distinct().Count() != categoryIdList.Length)
            throw ExceptionFactory.Duplicate("One or more categories are duplicated.");

        foreach (var categoryId in categoryIdList)
        {
            var categoryMapping = ProductCategoryMapping.Create(
                Id,
                categoryId);
            CategoryMappings.Add(categoryMapping);
        }
    }

    public void AssignCategory(Guid categoryId, int displayOrder = 0)
    {
        if (CategoryMappings.Any(m => m.CategoryId == categoryId))
            return;

        var categoryMapping = ProductCategoryMapping.Create(
            Id,
            categoryId,
            displayOrder);
        CategoryMappings.Add(categoryMapping);
    }

    public void RemoveCategory(Guid categoryId)
    {
        var mapping = CategoryMappings.FirstOrDefault(m => m.CategoryId == categoryId);
        if (mapping is null)
            return;

        CategoryMappings.Remove(mapping);
    }

    public void SetTags(IEnumerable<Guid> tagIds)
    {
        if (TagMappings.Count > 0)
            throw ExceptionFactory.InvalidState("Cannot set tags on a product that already has tags.");

        var tagIdList = tagIds.ToArray();
        if (tagIdList.Distinct().Count() != tagIdList.Length)
            throw ExceptionFactory.Duplicate("One or more tags are duplicated.");

        foreach (var tagId in tagIdList)
        {
            var tagMapping = ProductTagMapping.Create(
                Id,
                tagId);
            TagMappings.Add(tagMapping);
        }
    }

    public void AssignTag(Guid tagId, int displayOrder = 0)
    {
        if (TagMappings.Any(m => m.TagId == tagId))
            return;

        var tagMapping = ProductTagMapping.Create(
            Id,
            tagId,
            displayOrder);
        TagMappings.Add(tagMapping);
    }

    public void RemoveTag(Guid tagId)
    {
        var mapping = TagMappings.FirstOrDefault(m => m.TagId == tagId);
        if (mapping is null)
            return;

        TagMappings.Remove(mapping);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields (name, slug, brand, category, SEO), status
    // transitions (publish/hide/archive/submit), display flags, and the
    // shared name-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(string name, string shortDescription, string description)
    {
        ValidateName(name);

        Name = name;
        ShortDescription = shortDescription;
        Description = description;
    }

    public void ChangeSlug(Slug slug)
    {
        Slug = slug;
    }

    public void ChangeBrand(Guid? brandId)
    {
        BrandId = brandId;
    }

    public void ChangeCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }

    public void AssignSeo(Guid seoId)
    {
        SeoId = seoId;
    }

    public void ClearSeo()
    {
        SeoId = null;
    }

    public void Publish()
    {
        ProductStatus = ProductStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void Hide()
    {
        ProductStatus = ProductStatus.Hidden;
    }

    public void Archive()
    {
        ProductStatus = ProductStatus.Archived;
    }

    public void SubmitForApproval()
    {
        ProductStatus = ProductStatus.PendingApproval;
    }

    public void Feature()
    {
        IsFeatured = true;
    }

    public void Unfeature()
    {
        IsFeatured = false;
    }

    public void Show()
    {
        IsVisible = true;
    }

    public void ConcealFromCatalog()
    {
        IsVisible = false;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void UpdateMetadata(ProductMetadata metadata)
    {
        Metadata = metadata;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Product name cannot be empty.");
    }

    #endregion
}
