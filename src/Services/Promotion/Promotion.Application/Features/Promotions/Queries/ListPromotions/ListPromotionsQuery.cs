using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Promotion.Application.Features.Promotions.Queries.ListPromotions;

public sealed record ListPromotionsQuery(
    PromotionStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<PromotionSummaryResponse>>;

public sealed record PromotionSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    PromotionStatus Status,
    PromotionType Type,
    int Priority,
    DateTime StartTime,
    DateTime EndTime,
    bool IsEnabled,
    bool IsApproved);
