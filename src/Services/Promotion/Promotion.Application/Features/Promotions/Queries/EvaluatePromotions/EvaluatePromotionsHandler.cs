using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Queries.EvaluatePromotions;

/// <summary>
/// A single deterministic evaluation pass: load candidates -> filter by time window/constraints/
/// targeting -> compute each candidate's discount -> resolve a stackable subset by
/// Priority/StackingPolicy/PromotionExclusion -> return in apply order. No randomness, no
/// database-order dependence (candidates are explicitly sorted), no external service calls. This
/// is a baseline greedy pass, not the full stacking optimization algorithm - Section 21 explicitly
/// defers that to the next Promotion Engine phase.
/// </summary>
public sealed class EvaluatePromotionsHandler(IPromotionReadService promotionReadService)
    : IQueryHandler<EvaluatePromotionsQuery, EvaluatePromotionsResponse>
{
    public async Task<EvaluatePromotionsResponse> Handle(EvaluatePromotionsQuery request, CancellationToken ct = default)
    {
        var asOf = DateTime.UtcNow;

        var candidates = await promotionReadService.GetEvaluationCandidatesAsync(request.Currency, ct);

        var timeEligible = candidates
            .Where(p => asOf >= p.StartTime && asOf < p.EndTime)
            .ToList();

        var evaluated = new List<EvaluationCandidate>();
        foreach (var promotion in timeEligible)
        {
            if (TryEvaluate(promotion, request, out var candidate))
                evaluated.Add(candidate!);
        }

        if (evaluated.Count == 0)
            return new EvaluatePromotionsResponse(false, 0m, []);

        var exclusions = await promotionReadService.GetExclusionsAsync(
            [.. evaluated.Select(e => e.Promotion.Id)], ct);

        var ordered = evaluated
            .OrderByDescending(e => e.Promotion.Priority)
            .ThenBy(e => e.Promotion.CreatedAt)
            .ToList();

        var applied = ResolveStackableSet(ordered, exclusions);

        var totalDiscount = Math.Min(applied.Sum(a => a.DiscountAmount), Math.Max(request.OrderAmount, 0m));

        var results = applied
            .Select((a, index) => new AppliedPromotionResult(
                a.Promotion.Id,
                a.Promotion.Code.Value,
                a.Promotion.Name,
                a.Promotion.Priority,
                index,
                a.Promotion.Type,
                a.BenefitType,
                a.DiscountAmount,
                [.. a.MatchedItems.Where(i => i.ProductId is not null).Select(i => i.ProductId!.Value).Distinct()],
                [.. a.MatchedItems.Where(i => i.VariantId is not null).Select(i => i.VariantId!.Value).Distinct()],
                a.Reason))
            .ToList();

        return new EvaluatePromotionsResponse(results.Count > 0, totalDiscount, results);
    }

    private static bool TryEvaluate(PromotionEntity promotion, EvaluatePromotionsQuery request, out EvaluationCandidate? candidate)
    {
        candidate = null;

        if (!TryMatchTargets(promotion.Targets, request.Items, request.UserId, out var matchedItems, out var isItemTargeted))
            return false;

        if (!TryEvaluateConstraints(promotion.Constraints, request.OrderAmount, matchedItems, out var maxDiscountCap))
            return false;

        var baseAmount = isItemTargeted
            ? matchedItems.Sum(i => i.UnitPrice * i.Quantity)
            : request.OrderAmount;

        if (baseAmount <= 0 || promotion.Benefits.Count == 0)
            return false;

        var benefitType = promotion.Benefits.First().BenefitType;
        var rawDiscount = promotion.Benefits.Sum(b => ComputeBenefitDiscount(b, baseAmount));
        var cappedDiscount = maxDiscountCap is { } cap ? Math.Min(rawDiscount, cap) : rawDiscount;
        var discount = Math.Min(Math.Max(cappedDiscount, 0m), baseAmount);

        if (discount <= 0)
            return false;

        candidate = new EvaluationCandidate(
            promotion, matchedItems, benefitType, discount, $"Matched {promotion.Type} promotion \"{promotion.Name}\".");

        return true;
    }

    /// <summary>
    /// Customer target rows are an order-level gate (must match request.UserId, if present).
    /// Product/Sku/Category target rows narrow the promotion to matching items; if none of those
    /// types are configured (only Cart/Order/Customer, or no targets at all), the promotion
    /// applies to the entire order.
    /// </summary>
    private static bool TryMatchTargets(
        ICollection<PromotionTarget> targets,
        IReadOnlyList<EvaluationItemRequest> items,
        Guid? userId,
        out IReadOnlyList<EvaluationItemRequest> matchedItems,
        out bool isItemTargeted)
    {
        var customerTargets = targets.Where(t => t.TargetType == PromotionTargetType.Customer).ToList();
        if (customerTargets.Count > 0 &&
            !customerTargets.Any(t => userId is not null && t.TargetKey == userId.Value.ToString()))
        {
            matchedItems = [];
            isItemTargeted = false;
            return false;
        }

        var itemTargets = targets
            .Where(t => t.TargetType is PromotionTargetType.Product or PromotionTargetType.Sku or PromotionTargetType.Category)
            .ToList();

        if (itemTargets.Count == 0)
        {
            matchedItems = items;
            isItemTargeted = false;
            return items.Count > 0;
        }

        matchedItems = items.Where(item => itemTargets.Any(t => t.TargetType switch
        {
            PromotionTargetType.Product => item.ProductId is not null && t.TargetKey == item.ProductId.Value.ToString(),
            PromotionTargetType.Sku => item.VariantId is not null && t.TargetKey == item.VariantId.Value.ToString(),
            PromotionTargetType.Category => item.CategoryId is not null && t.TargetKey == item.CategoryId.Value.ToString(),
            _ => false,
        })).ToList();

        isItemTargeted = true;
        return matchedItems.Count > 0;
    }

    /// <summary>
    /// Only constraint types backed by data in EvaluatePromotionsQuery are enforced -
    /// CustomerSegment/PaymentMethod are configurable on a Promotion but silently not evaluated
    /// this phase (the evaluation context carries neither field, per Section 14's minimal request
    /// shape) - a documented gap, not a bug. MaximumDiscountAmount is not a gate; it's returned
    /// via maxDiscountCap for the caller to apply to the computed discount (Section 16).
    /// </summary>
    private static bool TryEvaluateConstraints(
        ICollection<PromotionConstraint> constraints,
        decimal orderAmount,
        IReadOnlyList<EvaluationItemRequest> matchedItems,
        out decimal? maxDiscountCap)
    {
        maxDiscountCap = null;
        var quantity = matchedItems.Sum(i => i.Quantity);

        foreach (var constraint in constraints)
        {
            switch (constraint.ConstraintType)
            {
                case PromotionConstraintType.MinimumOrderAmount:
                    if (!decimal.TryParse(constraint.Value, out var minOrder) || orderAmount < minOrder)
                        return false;
                    break;

                case PromotionConstraintType.MaximumOrderAmount:
                    if (decimal.TryParse(constraint.Value, out var maxOrder) && orderAmount > maxOrder)
                        return false;
                    break;

                case PromotionConstraintType.MinimumQuantity:
                    if (!int.TryParse(constraint.Value, out var minQty) || quantity < minQty)
                        return false;
                    break;

                case PromotionConstraintType.MaximumQuantity:
                    if (int.TryParse(constraint.Value, out var maxQty) && quantity > maxQty)
                        return false;
                    break;

                case PromotionConstraintType.ProductCategory:
                    if (!matchedItems.Any(i => i.CategoryId is not null && i.CategoryId.Value.ToString() == constraint.Value))
                        return false;
                    break;

                case PromotionConstraintType.MaximumDiscountAmount:
                    if (decimal.TryParse(constraint.Value, out var cap))
                        maxDiscountCap = cap;
                    break;

                case PromotionConstraintType.CustomerSegment:
                case PromotionConstraintType.PaymentMethod:
                    break;
            }
        }

        return true;
    }

    private static decimal ComputeBenefitDiscount(PromotionBenefit benefit, decimal baseAmount) => benefit.BenefitType switch
    {
        PromotionBenefitType.PercentageOff => Math.Round(baseAmount * (benefit.Value / 100m), 2, MidpointRounding.AwayFromZero),
        PromotionBenefitType.FixedAmountOff => Math.Min(benefit.Value, baseAmount),
        // No shipping-cost/gift-value data in this evaluation context - see Section 13/15 scope notes.
        PromotionBenefitType.FreeShipping or PromotionBenefitType.FreeGift => 0m,
        _ => 0m,
    };

    /// <summary>
    /// Greedy pass over Priority-then-CreatedAt-ordered candidates: a NotStackable candidate (or
    /// one arriving after a NotStackable promotion is already committed) stops there;
    /// StackWithSameType additionally requires every already-committed promotion to share its
    /// Type; an explicit PromotionExclusion pairing always blocks, regardless of StackingMode.
    /// </summary>
    private static List<EvaluationCandidate> ResolveStackableSet(
        List<EvaluationCandidate> ordered,
        IReadOnlyList<PromotionExclusion> exclusions)
    {
        var committed = new List<EvaluationCandidate>();

        foreach (var candidate in ordered)
        {
            if (committed.Count > 0)
            {
                var mode = candidate.Promotion.StackingPolicy?.Mode ?? PromotionStackingMode.NotStackable;

                if (mode == PromotionStackingMode.NotStackable)
                    continue;

                if (committed.Any(c => (c.Promotion.StackingPolicy?.Mode ?? PromotionStackingMode.NotStackable) == PromotionStackingMode.NotStackable))
                    continue;

                if (mode == PromotionStackingMode.StackWithSameType &&
                    !committed.All(c => c.Promotion.Type == candidate.Promotion.Type))
                    continue;

                var isExcluded = exclusions.Any(e =>
                    (e.PromotionId == candidate.Promotion.Id && committed.Any(c => c.Promotion.Id == e.ExcludedPromotionId)) ||
                    (e.ExcludedPromotionId == candidate.Promotion.Id && committed.Any(c => c.Promotion.Id == e.PromotionId)));

                if (isExcluded)
                    continue;
            }

            committed.Add(candidate);
        }

        return committed;
    }

    private sealed record EvaluationCandidate(
        PromotionEntity Promotion,
        IReadOnlyList<EvaluationItemRequest> MatchedItems,
        PromotionBenefitType BenefitType,
        decimal DiscountAmount,
        string Reason);
}
