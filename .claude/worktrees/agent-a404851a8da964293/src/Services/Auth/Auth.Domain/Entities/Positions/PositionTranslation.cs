using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Positions;

/// <summary>
/// Owned child of Position - a locale-specific override of the position's admin-facing
/// display copy. Id doubles as the owning Position's Id (one row per language, see
/// PositionTranslationConfig for the composite (Id, LanguageCode) key).
/// </summary>
public sealed class PositionTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Position Position { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PositionTranslation() { }

    public static PositionTranslation Create(
        Guid positionId,
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        ValidateDisplayName(displayName);

        return new PositionTranslation
        {
            Id = positionId,
            LanguageCode = languageCode,
            DisplayName = displayName,
            Description = description,
        };
    }

    public void UpdateContent(string displayName, string? description)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName;
        Description = description;
    }

    public static bool IsValidDisplayName(string? displayName) => displayName.IsNotNullOrWhiteSpace();

    private static void ValidateDisplayName(string displayName)
    {
        if (!IsValidDisplayName(displayName))
            throw ExceptionFactory.RequiredField("Translated display name cannot be empty.");
    }
}
