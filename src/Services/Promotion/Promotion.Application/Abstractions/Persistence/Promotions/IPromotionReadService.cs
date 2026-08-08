namespace NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

public interface IPromotionReadService
{
    Task<PromotionEntity?> GetByIdAsync(Guid promotionId, CancellationToken ct = default);

    Task<(IReadOnlyList<PromotionEntity> Items, int TotalCount)> SearchAsync(
        PromotionStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Candidate Promotions for evaluation: enabled, Active, matching currency, Automatic
    /// execution mode, with Targets/Benefits/Constraints/StackingPolicy loaded. The caller still
    /// checks the StartTime/EndTime window against the evaluation time - see
    /// EvaluatePromotionsHandler.
    /// </summary>
    Task<IReadOnlyList<PromotionEntity>> GetEvaluationCandidatesAsync(
        string currency,
        CancellationToken ct = default);

    /// <summary>PromotionExclusion has no Promotion navigation collection by design - queried directly for the given candidate set.</summary>
    Task<IReadOnlyList<PromotionExclusion>> GetExclusionsAsync(
        IReadOnlyList<Guid> promotionIds,
        CancellationToken ct = default);
}
