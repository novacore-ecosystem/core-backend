using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

namespace NovaCore.Promotion.Persistence.Engine;

public sealed class PromotionDbContext(DbContextOptions<PromotionDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    // Campaigns
    public DbSet<Campaign> Campaigns { get; set; } = null!;
    public DbSet<CampaignSchedule> CampaignSchedules { get; set; } = null!;
    public DbSet<CampaignAudience> CampaignAudiences { get; set; } = null!;
    public DbSet<CampaignChannel> CampaignChannels { get; set; } = null!;
    public DbSet<CampaignTag> CampaignTags { get; set; } = null!;
    public DbSet<CampaignAttachment> CampaignAttachments { get; set; } = null!;
    public DbSet<CampaignBudget> CampaignBudgets { get; set; } = null!;
    public DbSet<CampaignApproval> CampaignApprovals { get; set; } = null!;
    public DbSet<CampaignTranslation> CampaignTranslations { get; set; } = null!;

    // Promotions
    public DbSet<PromotionEntity> Promotions { get; set; } = null!;
    public DbSet<PromotionVersion> PromotionVersions { get; set; } = null!;
    public DbSet<PromotionRuleGroup> PromotionRuleGroups { get; set; } = null!;
    public DbSet<PromotionRule> PromotionRules { get; set; } = null!;
    public DbSet<PromotionCondition> PromotionConditions { get; set; } = null!;
    public DbSet<PromotionTarget> PromotionTargets { get; set; } = null!;
    public DbSet<PromotionBenefit> PromotionBenefits { get; set; } = null!;
    public DbSet<PromotionConstraint> PromotionConstraints { get; set; } = null!;
    public DbSet<PromotionUsageLimit> PromotionUsageLimits { get; set; } = null!;
    public DbSet<PromotionPriority> PromotionPriorities { get; set; } = null!;
    public DbSet<PromotionExclusion> PromotionExclusions { get; set; } = null!;
    public DbSet<PromotionExecutionPolicy> PromotionExecutionPolicies { get; set; } = null!;
    public DbSet<PromotionStackingPolicy> PromotionStackingPolicies { get; set; } = null!;
    public DbSet<PromotionTranslation> PromotionTranslations { get; set; } = null!;

    // Coupons
    public DbSet<Coupon> Coupons { get; set; } = null!;
    public DbSet<CouponCode> CouponCodes { get; set; } = null!;
    public DbSet<CouponBatch> CouponBatches { get; set; } = null!;
    public DbSet<CouponUsage> CouponUsages { get; set; } = null!;
    public DbSet<CouponReservation> CouponReservations { get; set; } = null!;
    public DbSet<CouponHistory> CouponHistories { get; set; } = null!;
    public DbSet<CouponVersion> CouponVersions { get; set; } = null!;
    public DbSet<CouponApproval> CouponApprovals { get; set; } = null!;
    public DbSet<CouponTranslation> CouponTranslations { get; set; } = null!;

    // Vouchers
    public DbSet<Voucher> Vouchers { get; set; } = null!;
    public DbSet<VoucherWallet> VoucherWallets { get; set; } = null!;
    public DbSet<VoucherBatch> VoucherBatches { get; set; } = null!;
    public DbSet<VoucherIssue> VoucherIssues { get; set; } = null!;
    public DbSet<VoucherReservation> VoucherReservations { get; set; } = null!;
    public DbSet<VoucherRedemption> VoucherRedemptions { get; set; } = null!;
    public DbSet<VoucherTransfer> VoucherTransfers { get; set; } = null!;
    public DbSet<VoucherHistory> VoucherHistories { get; set; } = null!;
    public DbSet<VoucherExpiration> VoucherExpirations { get; set; } = null!;
    public DbSet<VoucherFreeze> VoucherFreezes { get; set; } = null!;
    public DbSet<VoucherTranslation> VoucherTranslations { get; set; } = null!;

    // Loyalty
    public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; } = null!;
    public DbSet<PointAccount> PointAccounts { get; set; } = null!;
    public DbSet<PointTransaction> PointTransactions { get; set; } = null!;
    public DbSet<PointLedger> PointLedgers { get; set; } = null!;
    public DbSet<PointExpiration> PointExpirations { get; set; } = null!;
    public DbSet<PointAdjustment> PointAdjustments { get; set; } = null!;
    public DbSet<PointRule> PointRules { get; set; } = null!;
    public DbSet<PointPolicy> PointPolicies { get; set; } = null!;
    public DbSet<PointHistory> PointHistories { get; set; } = null!;
    public DbSet<LoyaltyProgramTranslation> LoyaltyProgramTranslations { get; set; } = null!;

    // Rewards
    public DbSet<RewardProgram> RewardPrograms { get; set; } = null!;
    public DbSet<RewardDefinition> RewardDefinitions { get; set; } = null!;
    public DbSet<RewardDistribution> RewardDistributions { get; set; } = null!;
    public DbSet<RewardExecution> RewardExecutions { get; set; } = null!;
    public DbSet<RewardReservation> RewardReservations { get; set; } = null!;
    public DbSet<RewardClaim> RewardClaims { get; set; } = null!;
    public DbSet<RewardHistory> RewardHistories { get; set; } = null!;
    public DbSet<RewardProgramTranslation> RewardProgramTranslations { get; set; } = null!;

    // Distributions
    public DbSet<DistributionJob> DistributionJobs { get; set; } = null!;
    public DbSet<DistributionBatch> DistributionBatches { get; set; } = null!;
    public DbSet<DistributionItem> DistributionItems { get; set; } = null!;
    public DbSet<DistributionExecution> DistributionExecutions { get; set; } = null!;
    public DbSet<DistributionRetry> DistributionRetries { get; set; } = null!;
    public DbSet<DistributionHistory> DistributionHistories { get; set; } = null!;

    // Recommendations
    public DbSet<RecommendationProgram> RecommendationPrograms { get; set; } = null!;
    public DbSet<RecommendationRule> RecommendationRules { get; set; } = null!;
    public DbSet<RecommendationProduct> RecommendationProducts { get; set; } = null!;
    public DbSet<RecommendationScore> RecommendationScores { get; set; } = null!;
    public DbSet<RecommendationHistory> RecommendationHistories { get; set; } = null!;
    public DbSet<RecommendationProgramTranslation> RecommendationProgramTranslations { get; set; } = null!;

    // Product Sets
    public DbSet<ProductSet> ProductSets { get; set; } = null!;
    public DbSet<ProductSetItem> ProductSetItems { get; set; } = null!;
    public DbSet<ProductBundle> ProductBundles { get; set; } = null!;
    public DbSet<BundlePrice> BundlePrices { get; set; } = null!;
    public DbSet<BundleRule> BundleRules { get; set; } = null!;
    public DbSet<BundleGift> BundleGifts { get; set; } = null!;
    public DbSet<ProductSetTranslation> ProductSetTranslations { get; set; } = null!;
    public DbSet<ProductBundleTranslation> ProductBundleTranslations { get; set; } = null!;

    // Gifts
    public DbSet<GiftProgram> GiftPrograms { get; set; } = null!;
    public DbSet<GiftItem> GiftItems { get; set; } = null!;
    public DbSet<GiftInventory> GiftInventories { get; set; } = null!;
    public DbSet<GiftReservation> GiftReservations { get; set; } = null!;
    public DbSet<GiftClaim> GiftClaims { get; set; } = null!;
    public DbSet<GiftUsage> GiftUsages { get; set; } = null!;
    public DbSet<GiftProgramTranslation> GiftProgramTranslations { get; set; } = null!;

    // Approvals
    public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; } = null!;
    public DbSet<ApprovalStep> ApprovalSteps { get; set; } = null!;
    public DbSet<ApprovalAssignment> ApprovalAssignments { get; set; } = null!;
    public DbSet<ApprovalDecision> ApprovalDecisions { get; set; } = null!;
    public DbSet<ApprovalComment> ApprovalComments { get; set; } = null!;
    public DbSet<ApprovalHistory> ApprovalHistories { get; set; } = null!;

    // Validations
    public DbSet<PromotionValidationPolicy> PromotionValidationPolicies { get; set; } = null!;
    public DbSet<PromotionValidationResult> PromotionValidationResults { get; set; } = null!;
    public DbSet<PromotionSimulation> PromotionSimulations { get; set; } = null!;
    public DbSet<PromotionSimulationScenario> PromotionSimulationScenarios { get; set; } = null!;
    public DbSet<PromotionSimulationResult> PromotionSimulationResults { get; set; } = null!;

    // Audits
    public DbSet<PromotionAudit> PromotionAudits { get; set; } = null!;
    public DbSet<RuleAudit> RuleAudits { get; set; } = null!;
    public DbSet<ApprovalAudit> ApprovalAudits { get; set; } = null!;
    public DbSet<ExecutionAudit> ExecutionAudits { get; set; } = null!;

    // Reliability
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
}
