namespace NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

public interface IPromotionWriteService
{
    /// <summary>Commits via bare SaveChangesAsync, matching CreateCoupon's precedent shape.</summary>
    Task CreateAsync(PromotionEntity promotion, CancellationToken ct = default);

    Task UpdateDetailsAsync(
        Guid promotionId,
        string name,
        string? description,
        DateTime startTime,
        DateTime endTime,
        string timeZone,
        int priority,
        CancellationToken ct = default);

    Task EnableAsync(Guid promotionId, CancellationToken ct = default);

    Task DisableAsync(Guid promotionId, CancellationToken ct = default);

    /// <summary>"Delete" maps to Promotion.Cancel() - no physical delete, same Domain-retains-history precedent as Coupon.Disable.</summary>
    Task CancelAsync(Guid promotionId, CancellationToken ct = default);

    Task SubmitForApprovalAsync(Guid promotionId, Guid approvalWorkflowId, CancellationToken ct = default);

    /// <summary>Approve also Activates - PromotionStatus has no separate Scheduled state, so Approve is the only path to Active (Activate() itself guards on IsApproved). See ApprovePromotionHandler.</summary>
    Task ApproveAsync(Guid promotionId, CancellationToken ct = default);

    Task RejectAsync(Guid promotionId, CancellationToken ct = default);

    Task AddExclusionAsync(Guid promotionId, Guid excludedPromotionId, CancellationToken ct = default);
}
