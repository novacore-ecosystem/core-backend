namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>
/// Structural record of a Campaign's approval request/decision. No approval-status enum/workflow
/// yet - Phase 2.5 ("Approval + Validation + Audit") owns the real workflow; this is a minimal
/// placeholder shape so the entity exists structurally without anticipating that phase's design.
/// </summary>
public sealed class CampaignApproval : BaseEntity<Guid>, IAuditable
{
    public Guid CampaignId { get; private set; }
    public Guid? RequestedById { get; private set; }
    public DateTime? RequestedAt { get; private set; }
    public Guid? DecidedById { get; private set; }
    public DateTime? DecidedAt { get; private set; }
    public string? Comment { get; private set; }

    public Campaign Campaign { get; private set; } = default!;

    private CampaignApproval() { }

    public static CampaignApproval Create(Guid campaignId)
    {
        return new CampaignApproval
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
        };
    }

    // TODO (Phase 2.5): replace these structural stand-ins with the real approval workflow
    // (status enum, validation rules, audit trail) once that phase's design is available.
    public void RequestApproval(Guid requestedById)
    {
        RequestedById = requestedById;
        RequestedAt = DateTime.UtcNow;
    }

    public void RecordDecision(Guid decidedById, string? comment)
    {
        DecidedById = decidedById;
        DecidedAt = DateTime.UtcNow;
        Comment = comment;
    }
}
