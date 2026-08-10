namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product, optionally scoped to a single Variant. Stores a raw Url today;
/// migrating to a dedicated Media service later only requires swapping Url for a MediaId.
/// </summary>
public sealed class ProductMedia : BaseEntity<Guid>, ITenantEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid? VariantId { get; private set; }
    public ProductVariant? Variant { get; private set; }
    public MediaType MediaType { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? Thumbnail { get; private set; }
    public string? Alt { get; private set; }
    public string? Title { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductMedia() { }

    public static ProductMedia Create(
        Guid productId,
        MediaType mediaType,
        string url,
        int displayOrder,
        Guid? variantId = null,
        string? thumbnail = null,
        string? alt = null,
        string? title = null,
        bool isPrimary = false,
        CatalogStatus status = CatalogStatus.Active)
    {
        ValidateUrl(url);

        return new ProductMedia
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            VariantId = variantId,
            MediaType = mediaType,
            Url = url,
            Thumbnail = thumbnail,
            Alt = alt,
            Title = title,
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary,
            Status = status,
        };
    }

    public void UpdateUrl(string url, string? thumbnail)
    {
        ValidateUrl(url);

        Url = url;
        Thumbnail = thumbnail;
    }

    public void UpdateDisplayText(string? alt, string? title)
    {
        Alt = alt;
        Title = title;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void MarkAsPrimary()
    {
        IsPrimary = true;
    }

    public void UnmarkAsPrimary()
    {
        IsPrimary = false;
    }

    public void Activate()
    {
        Status = CatalogStatus.Active;
    }

    public void Deactivate()
    {
        Status = CatalogStatus.Inactive;
    }

    public static bool IsValidUrl(string? url) => !string.IsNullOrWhiteSpace(url);

    private static void ValidateUrl(string url)
    {
        if (!IsValidUrl(url))
            throw ExceptionFactory.RequiredField("Media URL cannot be empty.");
    }
}
