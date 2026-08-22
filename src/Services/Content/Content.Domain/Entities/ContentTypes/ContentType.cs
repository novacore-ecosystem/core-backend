namespace NovaCore.Content.Domain.Entities.ContentTypes;

/// <summary>
/// Aggregate root describing a schema/type of content (ARTICLE, NEWS, BULLETIN, ...). Owns its
/// field definitions. Content items reference a ContentType by id; the type itself carries no
/// knowledge of the content instances built against it.
/// </summary>
public sealed class ContentType : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ContentKey Key { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ContentTypeStatus Status { get; private set; }
    public int SchemaVersion { get; private set; } = 1;

    public ICollection<ContentFieldDefinition> FieldDefinitions { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentType() { }

    public static ContentType Create(
        ContentKey key,
        string name,
        string description,
        ContentTypeStatus status = ContentTypeStatus.Active)
    {
        ValidateName(name);

        return new ContentType
        {
            Id = Guid.CreateVersion7(),
            Key = key,
            Name = name,
            Description = description,
            Status = status,
        };
    }

    // ============================================================================
    // Field Definitions
    // Manages the owned ContentFieldDefinition collection defining this type's schema.
    // A structural change (add/remove/update a field) bumps SchemaVersion so existing
    // ContentVersions authored under an older schema remain distinguishable.
    // ============================================================================

    #region Field Definitions

    public ContentFieldDefinition AddFieldDefinition(
        ContentKey key,
        string name,
        string description,
        ContentFieldType fieldType,
        bool isRequired = false,
        bool isLocalized = false,
        bool isSearchable = false,
        bool isSortable = false,
        string? defaultValue = null,
        string? validationConfiguration = null,
        int displayOrder = 0)
    {
        if (FieldDefinitions.Any(f => f.Key == key))
            throw ExceptionFactory.Duplicate($"A field definition with key '{key}' already exists on this content type.");

        var field = ContentFieldDefinition.Create(
            Id,
            key,
            name,
            description,
            fieldType,
            isRequired,
            isLocalized,
            isSearchable,
            isSortable,
            defaultValue,
            validationConfiguration,
            displayOrder);

        FieldDefinitions.Add(field);
        SchemaVersion++;

        return field;
    }

    public void RemoveFieldDefinition(Guid fieldDefinitionId)
    {
        var field = FieldDefinitions.FirstOrDefault(f => f.Id == fieldDefinitionId)
            ?? throw ExceptionFactory.EntityNotFound<ContentFieldDefinition>(fieldDefinitionId);

        FieldDefinitions.Remove(field);
        SchemaVersion++;
    }

    public void UpdateFieldDefinition(
        Guid fieldDefinitionId,
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
        var field = FieldDefinitions.FirstOrDefault(f => f.Id == fieldDefinitionId)
            ?? throw ExceptionFactory.EntityNotFound<ContentFieldDefinition>(fieldDefinitionId);

        field.UpdateDetails(
            name,
            description,
            isRequired,
            isLocalized,
            isSearchable,
            isSortable,
            defaultValue,
            validationConfiguration,
            displayOrder);
        SchemaVersion++;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void Activate()
    {
        Status = ContentTypeStatus.Active;
    }

    public void Deactivate()
    {
        Status = ContentTypeStatus.Inactive;
    }

    public void Archive()
    {
        Status = ContentTypeStatus.Archived;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Content type name cannot be empty.");
    }

    #endregion
}
