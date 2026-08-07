namespace NovaCore.Promotion.Domain.Enums;

/// <summary>
/// Shared lifecycle status for the four "program" aggregate roots that used an identical
/// Draft/Active/Paused/Expired/Archived value set (LoyaltyProgram, RewardProgram, GiftProgram,
/// RecommendationProgram). Consolidated from 4 duplicate enums during the Phase 2.5 Domain
/// Standardization Review - see docs/promotion-service/enums/README.md.
/// </summary>
public enum ProgramStatus : byte
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Expired = 3,
    Archived = 4,
}
