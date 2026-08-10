using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.Metadata;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Scopes;

/// <summary>
/// Business scope inside a Tenant (Branch, Agency, Dealer, Region, ...), organized as a
/// self-referencing hierarchy via ParentScopeId. Path/Level exist only to simplify hierarchy
/// management (rendering breadcrumbs, computing depth) - they are not query-filtering
/// mechanisms, and this aggregate never performs scope/tenant filtering itself (that is a
/// later phase's concern). Mirrors ProductCategory's shadow-navigation hierarchy shape - no
/// Domain-level Parent/Children navigation, since detecting a deeper ancestor cycle requires
/// querying the full scope tree, an Application-layer concern.
/// </summary>
public sealed class Scope : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid? ParentScopeId { get; private set; }
    public ScopeCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ScopeMetadata Metadata { get; private set; } = new();
    public string Path { get; private set; } = string.Empty;
    public int Level { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<ScopeTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = default!;

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Scope() { }

    public static Scope Create(
        Guid tenantId,
        ScopeCode code,
        string name,
        string path,
        int level,
        Guid? parentScopeId = null,
        string? description = null,
        int sortOrder = 0,
        ScopeMetadata? metadata = null)
    {
        ValidateName(name);

        var id = Guid.CreateVersion7();

        if (parentScopeId == id)
            throw ExceptionFactory.InvalidState("A scope cannot be its own parent.");

        var scope = new Scope
        {
            Id = id,
            ParentScopeId = parentScopeId,
            Code = code,
            Name = name,
            Description = description,
            Metadata = metadata ?? new ScopeMetadata(),
            Path = path,
            Level = level,
            SortOrder = sortOrder,
            IsActive = true,
        };
        scope.AssignTenant(tenantId);
        return scope;
    }

    // ============================================================================
    // Translations
    // Manages the per-language Name/Description override for this scope, upserting by
    // language code - standard business translation, unlike Tenant's bootstrap-only locales.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string name,
        string? description = null)
    {
        ValidateName(name);

        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateContent(name, description);
            return;
        }

        var translation = ScopeTranslation.Create(
            Id,
            languageCode,
            name,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields, hierarchy reassignment (Path/Level are recomputed by the
    // caller - a single Scope instance cannot resolve the full tree on its own), ordering,
    // and Active/Inactive status. Code has no change method - it is the stable identifier
    // scope-scoped data keys off of.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void UpdateMetadata(ScopeMetadata metadata)
    {
        Metadata = metadata;
    }

    /// <summary>Moves this scope under a new parent (or to root when null), with the
    /// caller-computed Path/Level for the new position. Only guards against direct
    /// self-parenting - see the class doc comment for why deeper cycle detection is
    /// out of scope here.</summary>
    public void ChangeParent(Guid? parentScopeId, string path, int level)
    {
        if (parentScopeId == Id)
            throw ExceptionFactory.InvalidState("A scope cannot be its own parent.");

        ParentScopeId = parentScopeId;
        Path = path;
        Level = level;
    }

    public void Reorder(int sortOrder)
    {
        SortOrder = sortOrder;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public static bool IsValidName(string? name) => name.IsNotNullOrWhiteSpace();

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Scope name cannot be empty.");
    }

    #endregion
}
