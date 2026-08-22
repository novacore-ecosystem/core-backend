namespace NovaCore.Content.Domain.Entities.ContentTypes;

/// <summary>Defines a single field available on ContentVersions of the owning ContentType.</summary>
public sealed class ContentFieldDefinition : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentTypeId { get; private set; }
    public ContentType ContentType { get; private set; } = default!;
    public ContentKey Key { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ContentFieldType FieldType { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsLocalized { get; private set; }
    public bool IsSearchable { get; private set; }
    public bool IsSortable { get; private set; }
    public string? DefaultValue { get; private set; }
    public string? ValidationConfiguration { get; private set; }
    public int DisplayOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentFieldDefinition() { }

    internal static ContentFieldDefinition Create(
        Guid contentTypeId,
        ContentKey key,
        string name,
        string description,
        ContentFieldType fieldType,
        bool isRequired,
        bool isLocalized,
        bool isSearchable,
        bool isSortable,
        string? defaultValue,
        string? validationConfiguration,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Field definition name cannot be empty.");

        return new ContentFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            ContentTypeId = contentTypeId,
            Key = key,
            Name = name,
            Description = description,
            FieldType = fieldType,
            IsRequired = isRequired,
            IsLocalized = isLocalized,
            IsSearchable = isSearchable,
            IsSortable = isSortable,
            DefaultValue = defaultValue,
            ValidationConfiguration = validationConfiguration,
            DisplayOrder = displayOrder,
        };
    }

    internal void UpdateDetails(
        string name,
        string description,
        bool isRequired,
        bool isLocalized,
        bool isSearchable,
        bool isSortable,
        string? defaultValue,
        string? validationConfiguration,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Field definition name cannot be empty.");

        Name = name;
        Description = description;
        IsRequired = isRequired;
        IsLocalized = isLocalized;
        IsSearchable = isSearchable;
        IsSortable = isSortable;
        DefaultValue = defaultValue;
        ValidationConfiguration = validationConfiguration;
        DisplayOrder = displayOrder;
    }
}
