namespace NovaCore.Promotion.Domain.Entities.Recommendations;

/// <summary>Per-language override of a RecommendationProgram's Name/Description. Identity is ProgramId + LanguageCode (Phase 3.1 correction) - no surrogate Id.</summary>
public sealed class RecommendationProgramTranslation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid ProgramId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public RecommendationProgram Program { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private RecommendationProgramTranslation() { }

    /// <summary>Only RecommendationProgram may construct a RecommendationProgramTranslation - see RecommendationProgram.Translate.</summary>
    internal static RecommendationProgramTranslation Create(Guid programId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new RecommendationProgramTranslation
        {
            ProgramId = programId,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    internal void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Translated recommendation program name cannot be empty.");
    }
}
