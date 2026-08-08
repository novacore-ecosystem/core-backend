using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;
using NovaCore.Promotion.Persistence.Contexts.Promotions.Repositories;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Promotions.Write;

/// <summary>
/// CreateAsync/EnableAsync/DisableAsync/CancelAsync commit via bare SaveChangesAsync (single,
/// simple mutations, matching CouponWriteService's split). UpdateDetailsAsync and
/// Submit/Approve/RejectAsync stage only - their Handlers own ExecuteTransactionAsync (the latter
/// three because they always run alongside a matching IApprovalWorkflowWriteService call spanning
/// two independent aggregate roots - see SubmitPromotionHandler/ApprovePromotionHandler/
/// RejectPromotionHandler). AddExclusionAsync injects PromotionDbContext directly (not
/// IPromotionRepository) because PromotionExclusion is a pure-mapping, composite-key entity with
/// no Promotion navigation collection and no internal Create gate (see PromotionExclusion's own
/// doc comment) - it cannot be reached through the generic IRepository&lt;Promotion, Guid&gt; binding.
/// </summary>
public sealed class PromotionWriteService(
    IPromotionRepository promotionRepo,
    PromotionDbContext dbContext,
    IUnitOfWork unitOfWork) : IPromotionWriteService
{
    public async Task CreateAsync(PromotionEntity promotion, CancellationToken ct = default)
    {
        await promotionRepo.AddAsync(promotion, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateDetailsAsync(
        Guid promotionId,
        string name,
        string? description,
        DateTime startTime,
        DateTime endTime,
        string timeZone,
        int priority,
        CancellationToken ct = default)
    {
        await promotionRepo.UpdateAsync(promotionId, promotion =>
        {
            promotion.UpdateDetails(name, description);
            promotion.Reschedule(startTime, endTime, timeZone);
            promotion.ChangePriority(priority);
        }, ct);
    }

    public async Task EnableAsync(Guid promotionId, CancellationToken ct = default)
    {
        await promotionRepo.UpdateAsync(promotionId, promotion => promotion.Enable(), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DisableAsync(Guid promotionId, CancellationToken ct = default)
    {
        await promotionRepo.UpdateAsync(promotionId, promotion => promotion.Disable(), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Guid promotionId, CancellationToken ct = default)
    {
        await promotionRepo.UpdateAsync(promotionId, promotion => promotion.Cancel(), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public Task SubmitForApprovalAsync(Guid promotionId, Guid approvalWorkflowId, CancellationToken ct = default) =>
        promotionRepo.UpdateAsync(promotionId, promotion => promotion.SubmitForApproval(approvalWorkflowId), ct);

    public Task ApproveAsync(Guid promotionId, CancellationToken ct = default) =>
        promotionRepo.UpdateAsync(promotionId, promotion =>
        {
            promotion.MarkApproved();
            promotion.Activate();
        }, ct);

    public Task RejectAsync(Guid promotionId, CancellationToken ct = default) =>
        promotionRepo.UpdateAsync(promotionId, promotion => promotion.MarkRejected(), ct);

    public async Task AddExclusionAsync(Guid promotionId, Guid excludedPromotionId, CancellationToken ct = default)
    {
        dbContext.PromotionExclusions.Add(PromotionExclusion.Create(promotionId, excludedPromotionId));
        await unitOfWork.SaveChangesAsync(ct);
    }
}
