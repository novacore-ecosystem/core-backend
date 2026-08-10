using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Positions;

/// <summary>
/// Organizational responsibility (e.g. "Office Manager", "Accounting Staff") - the primary unit
/// administrators assign to Accounts. A Position bundles the Roles that responsibility carries
/// (PositionRole), so a personnel change (Employee A replaced by Employee B) is "assign the same
/// Position" instead of recreating dozens of individual Role/permission assignments. A Position
/// never grants PermissionDefinitions directly - only through the Roles it aggregates, keeping
/// Role as the single reusable permission-bundle concept shared across many Positions.
/// </summary>
public sealed class Position : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public PositionCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemPosition { get; private set; }

    public ICollection<PositionRole> Roles { get; private set; } = [];
    public ICollection<PositionTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Position() { }

    public static Position Create(
        string name,
        PositionCode code,
        string? description = null,
        bool isSystemPosition = false)
    {
        ValidateName(name);

        return new Position
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            IsSystemPosition = isSystemPosition,
        };
    }

    // ============================================================================
    // Roles
    // Manages the PositionRole join collection - which Roles this Position carries.
    // Assignment changes here must be followed by an
    // Account.RefreshPermissionSnapshot() call for every Account holding this Position.
    // ============================================================================

    #region Roles

    public void AssignRole(Role role)
    {
        if (Roles.Any(r => r.RoleId == role.Id))
            return;

        Roles.Add(PositionRole.Create(Id, role.Id));
    }

    public void RemoveRole(Guid roleId)
    {
        var positionRole = Roles.FirstOrDefault(r => r.RoleId == roleId);
        if (positionRole is null)
            return;

        Roles.Remove(positionRole);
    }

    #endregion

    // ============================================================================
    // Translations
    // Manages per-language DisplayName/Description overrides, upserting by
    // language code. Code itself is never translated.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateContent(displayName, description);
            return;
        }

        var translation = PositionTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Display-name renaming and system-position protection. Code has no change
    // method - it is the stable identifier assignments key off of.
    // ============================================================================

    #region Details & lifecycle

    public void Rename(string name)
    {
        if (IsSystemPosition)
            throw ExceptionFactory.InvalidState("Cannot rename a system position.");

        ValidateName(name);
        Name = name;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public static bool IsValidName(string? name) => name.IsNotNullOrWhiteSpace();

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Position name cannot be empty.");
    }

    #endregion
}
