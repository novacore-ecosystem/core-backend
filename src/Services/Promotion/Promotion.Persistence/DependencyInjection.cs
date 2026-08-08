using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;
using NovaCore.BuildingBlock.Search.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.Promotion.Application.Abstractions.Persistence.Campaigns;
using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;
using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Application.Abstractions.Persistence.Vouchers;
using NovaCore.Promotion.Application.Abstractions.Persistence.Loyalty;
using NovaCore.Promotion.Application.Abstractions.Persistence.Rewards;
using NovaCore.Promotion.Application.Abstractions.Persistence.Distributions;
using NovaCore.Promotion.Application.Abstractions.Persistence.Recommendations;
using NovaCore.Promotion.Application.Abstractions.Persistence.ProductSets;
using NovaCore.Promotion.Application.Abstractions.Persistence.Gifts;
using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Persistence.Contexts.Campaigns.Read;
using NovaCore.Promotion.Persistence.Contexts.Campaigns.Write;
using NovaCore.Promotion.Persistence.Contexts.Promotions.Read;
using NovaCore.Promotion.Persistence.Contexts.Promotions.Write;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Read;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Write;
using NovaCore.Promotion.Persistence.Contexts.Vouchers.Read;
using NovaCore.Promotion.Persistence.Contexts.Vouchers.Write;
using NovaCore.Promotion.Persistence.Contexts.Loyalty.Read;
using NovaCore.Promotion.Persistence.Contexts.Loyalty.Write;
using NovaCore.Promotion.Persistence.Contexts.Rewards.Read;
using NovaCore.Promotion.Persistence.Contexts.Rewards.Write;
using NovaCore.Promotion.Persistence.Contexts.Distributions.Read;
using NovaCore.Promotion.Persistence.Contexts.Distributions.Write;
using NovaCore.Promotion.Persistence.Contexts.Recommendations.Read;
using NovaCore.Promotion.Persistence.Contexts.Recommendations.Write;
using NovaCore.Promotion.Persistence.Contexts.ProductSets.Read;
using NovaCore.Promotion.Persistence.Contexts.ProductSets.Write;
using NovaCore.Promotion.Persistence.Contexts.Gifts.Read;
using NovaCore.Promotion.Persistence.Contexts.Gifts.Write;
using NovaCore.Promotion.Persistence.Contexts.Approvals.Read;
using NovaCore.Promotion.Persistence.Contexts.Approvals.Write;
using NovaCore.Promotion.Application.Abstractions.Search;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Search.Indexers;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Search.Repositories;
using NovaCore.Promotion.Persistence.Engine;
using NovaCore.Promotion.Persistence.Engine.UnitOfWork;
using NovaCore.Promotion.Persistence.Reliability.Inbox;
using NovaCore.Promotion.Persistence.Reliability.Outbox;

namespace NovaCore.Promotion.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
        => builder.AddNpgsql();

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddApplicationServices()
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddAuditHierarchy()
            .AddPromotionSearchServices(configuration);

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<PromotionDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(PromotionDbContext));
        return services;
    }

    // {Root}Repo for each of the 11 true aggregate roots implements the generic IRepository<T> -
    // Scrutor's AsImplementedInterfaces() registers each concrete class against every interface it
    // implements, so this one scan call covers all eleven repos, plus the generic IRepository<T>
    // binding every other entity can be resolved through directly. Read/Write services are
    // registered explicitly since they're one-per-aggregate-root. Only the 11 true aggregate roots
    // (Campaign, Promotion, Coupon, Voucher, LoyaltyProgram, RewardProgram, DistributionJob,
    // RecommendationProgram, ProductSet, GiftProgram, ApprovalWorkflow) have a dedicated
    // repository/Read/Write service - every other entity (owned children reached through a root's
    // own Include, or flat entities with no independent query need yet) has an EF configuration
    // and DbSet but no entity-specific repository until a real feature needs one, per
    // docs/promotion-service/persistence/persistence-strategy.md.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(PromotionDbContext));

        services.AddScoped<ICampaignReadService, CampaignReadService>();
        services.AddScoped<ICampaignWriteService, CampaignWriteService>();

        services.AddScoped<IPromotionReadService, PromotionReadService>();
        services.AddScoped<IPromotionWriteService, PromotionWriteService>();

        services.AddScoped<ICouponReadService, CouponReadService>();
        services.AddScoped<ICouponWriteService, CouponWriteService>();

        services.AddScoped<IVoucherReadService, VoucherReadService>();
        services.AddScoped<IVoucherWriteService, VoucherWriteService>();

        services.AddScoped<ILoyaltyProgramReadService, LoyaltyProgramReadService>();
        services.AddScoped<ILoyaltyProgramWriteService, LoyaltyProgramWriteService>();

        services.AddScoped<IRewardProgramReadService, RewardProgramReadService>();
        services.AddScoped<IRewardProgramWriteService, RewardProgramWriteService>();

        services.AddScoped<IDistributionJobReadService, DistributionJobReadService>();
        services.AddScoped<IDistributionJobWriteService, DistributionJobWriteService>();

        services.AddScoped<IRecommendationProgramReadService, RecommendationProgramReadService>();
        services.AddScoped<IRecommendationProgramWriteService, RecommendationProgramWriteService>();

        services.AddScoped<IProductSetReadService, ProductSetReadService>();
        services.AddScoped<IProductSetWriteService, ProductSetWriteService>();

        services.AddScoped<IGiftProgramReadService, GiftProgramReadService>();
        services.AddScoped<IGiftProgramWriteService, GiftProgramWriteService>();

        services.AddScoped<IApprovalWorkflowReadService, ApprovalWorkflowReadService>();
        services.AddScoped<IApprovalWorkflowWriteService, ApprovalWorkflowWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddEfOutboxStore<PromotionDbContext>()
            .AddEfInboxStore<PromotionDbContext>()
            .AddEfDeadLetterQueryService<PromotionDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    // Every one of the 103 Promotion.Domain entities is registered here (all are IAuditable),
    // same exhaustive precedent Payment Service follows - the AuditInterceptor only groups a
    // changed entity under its root if both the entity implements IAuditable AND is registered
    // here, so leaving one out silently drops it from the audit trail rather than failing loudly.
    // IsRoot = every entity NOT reached through another entity's owned navigation collection
    // (true aggregate roots, plus the flat/unowned entities related by id only, per
    // docs/promotion-service/aggregates/*.md); BelongsTo<TParent> = every entity that IS owned via
    // an ICollection<T> on its parent.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            // Campaigns
            builder.Entity<Campaign>().IsRoot(x => x.Id);
            builder.Entity<CampaignBudget>().IsRoot(x => x.Id);
            builder.Entity<CampaignApproval>().IsRoot(x => x.Id);
            builder.Entity<CampaignSchedule>().BelongsTo<Campaign>(x => x.CampaignId);
            builder.Entity<CampaignAudience>().BelongsTo<Campaign>(x => x.CampaignId);
            builder.Entity<CampaignChannel>().BelongsTo<Campaign>(x => x.CampaignId);
            builder.Entity<CampaignTag>().BelongsTo<Campaign>(x => x.CampaignId);
            builder.Entity<CampaignAttachment>().BelongsTo<Campaign>(x => x.CampaignId);
            builder.Entity<CampaignTranslation>().BelongsTo<Campaign>(x => x.CampaignId);

            // Promotions
            builder.Entity<PromotionEntity>().IsRoot(x => x.Id);
            builder.Entity<PromotionRuleGroup>().IsRoot(x => x.Id);
            builder.Entity<PromotionPriority>().IsRoot(x => x.Id);
            builder.Entity<PromotionExclusion>().IsRoot(x => new { x.PromotionId, x.ExcludedPromotionId });
            builder.Entity<PromotionVersion>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionRule>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionTarget>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionBenefit>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionConstraint>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionUsageLimit>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionExecutionPolicy>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionStackingPolicy>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionTranslation>().BelongsTo<PromotionEntity>(x => x.PromotionId);
            builder.Entity<PromotionCondition>().BelongsTo<PromotionRule>(x => x.RuleId);

            // Coupons
            builder.Entity<Coupon>().IsRoot(x => x.Id);
            builder.Entity<CouponBatch>().IsRoot(x => x.Id);
            builder.Entity<CouponCode>().IsRoot(x => x.Id);
            builder.Entity<CouponApproval>().IsRoot(x => x.Id);
            builder.Entity<CouponReservation>().BelongsTo<Coupon>(x => x.CouponId);
            builder.Entity<CouponUsage>().BelongsTo<Coupon>(x => x.CouponId);
            builder.Entity<CouponHistory>().BelongsTo<Coupon>(x => x.CouponId);
            builder.Entity<CouponVersion>().BelongsTo<Coupon>(x => x.CouponId);
            builder.Entity<CouponTranslation>().BelongsTo<Coupon>(x => x.CouponId);

            // Vouchers
            builder.Entity<Voucher>().IsRoot(x => x.Id);
            builder.Entity<VoucherWallet>().IsRoot(x => x.Id);
            builder.Entity<VoucherBatch>().IsRoot(x => x.Id);
            builder.Entity<VoucherExpiration>().IsRoot(x => x.Id);
            builder.Entity<VoucherFreeze>().IsRoot(x => x.Id);
            builder.Entity<VoucherIssue>().BelongsTo<Voucher>(x => x.VoucherId);
            builder.Entity<VoucherReservation>().BelongsTo<Voucher>(x => x.VoucherId);
            builder.Entity<VoucherRedemption>().BelongsTo<Voucher>(x => x.VoucherId);
            builder.Entity<VoucherTransfer>().BelongsTo<Voucher>(x => x.VoucherId);
            builder.Entity<VoucherHistory>().BelongsTo<Voucher>(x => x.VoucherId);
            builder.Entity<VoucherTranslation>().BelongsTo<Voucher>(x => x.VoucherId);

            // Loyalty
            builder.Entity<LoyaltyProgram>().IsRoot(x => x.Id);
            builder.Entity<PointTransaction>().IsRoot(x => x.Id);
            builder.Entity<PointLedger>().IsRoot(x => x.Id);
            builder.Entity<PointExpiration>().IsRoot(x => x.Id);
            builder.Entity<PointAdjustment>().IsRoot(x => x.Id);
            builder.Entity<PointHistory>().IsRoot(x => x.Id);
            builder.Entity<PointRule>().BelongsTo<LoyaltyProgram>(x => x.ProgramId);
            builder.Entity<PointPolicy>().BelongsTo<LoyaltyProgram>(x => x.ProgramId);
            builder.Entity<PointAccount>().BelongsTo<LoyaltyProgram>(x => x.ProgramId);
            builder.Entity<LoyaltyProgramTranslation>().BelongsTo<LoyaltyProgram>(x => x.ProgramId);

            // Rewards
            builder.Entity<RewardProgram>().IsRoot(x => x.Id);
            builder.Entity<RewardExecution>().IsRoot(x => x.Id);
            builder.Entity<RewardReservation>().IsRoot(x => x.Id);
            builder.Entity<RewardClaim>().IsRoot(x => x.Id);
            builder.Entity<RewardHistory>().IsRoot(x => x.Id);
            builder.Entity<RewardDefinition>().BelongsTo<RewardProgram>(x => x.ProgramId);
            builder.Entity<RewardDistribution>().BelongsTo<RewardProgram>(x => x.ProgramId);
            builder.Entity<RewardProgramTranslation>().BelongsTo<RewardProgram>(x => x.ProgramId);

            // Distributions
            builder.Entity<DistributionJob>().IsRoot(x => x.Id);
            builder.Entity<DistributionItem>().IsRoot(x => x.Id);
            builder.Entity<DistributionExecution>().IsRoot(x => x.Id);
            builder.Entity<DistributionRetry>().IsRoot(x => x.Id);
            builder.Entity<DistributionHistory>().IsRoot(x => x.Id);
            builder.Entity<DistributionBatch>().BelongsTo<DistributionJob>(x => x.JobId);

            // Recommendations
            builder.Entity<RecommendationProgram>().IsRoot(x => x.Id);
            builder.Entity<RecommendationScore>().IsRoot(x => x.Id);
            builder.Entity<RecommendationHistory>().IsRoot(x => x.Id);
            builder.Entity<RecommendationRule>().BelongsTo<RecommendationProgram>(x => x.ProgramId);
            builder.Entity<RecommendationProduct>().BelongsTo<RecommendationProgram>(x => x.ProgramId);
            builder.Entity<RecommendationProgramTranslation>().BelongsTo<RecommendationProgram>(x => x.ProgramId);

            // Product Sets
            builder.Entity<ProductSet>().IsRoot(x => x.Id);
            builder.Entity<BundlePrice>().IsRoot(x => x.Id);
            builder.Entity<BundleRule>().IsRoot(x => x.Id);
            builder.Entity<BundleGift>().IsRoot(x => x.Id);
            builder.Entity<ProductSetItem>().BelongsTo<ProductSet>(x => x.ProductSetId);
            builder.Entity<ProductBundle>().BelongsTo<ProductSet>(x => x.ProductSetId);
            builder.Entity<ProductSetTranslation>().BelongsTo<ProductSet>(x => x.ProductSetId);
            builder.Entity<ProductBundleTranslation>().BelongsTo<ProductBundle>(x => x.BundleId);

            // Gifts
            builder.Entity<GiftProgram>().IsRoot(x => x.Id);
            builder.Entity<GiftInventory>().IsRoot(x => x.Id);
            builder.Entity<GiftReservation>().IsRoot(x => x.Id);
            builder.Entity<GiftClaim>().IsRoot(x => x.Id);
            builder.Entity<GiftUsage>().IsRoot(x => x.Id);
            builder.Entity<GiftItem>().BelongsTo<GiftProgram>(x => x.ProgramId);
            builder.Entity<GiftProgramTranslation>().BelongsTo<GiftProgram>(x => x.ProgramId);

            // Approvals
            builder.Entity<ApprovalWorkflow>().IsRoot(x => x.Id);
            builder.Entity<ApprovalAssignment>().IsRoot(x => x.Id);
            builder.Entity<ApprovalDecision>().IsRoot(x => x.Id);
            builder.Entity<ApprovalComment>().IsRoot(x => x.Id);
            builder.Entity<ApprovalHistory>().IsRoot(x => x.Id);
            builder.Entity<ApprovalStep>().BelongsTo<ApprovalWorkflow>(x => x.WorkflowId);

            // Validations - no aggregate root (Phase 2 brief gave none); each stays its own root
            builder.Entity<PromotionValidationPolicy>().IsRoot(x => x.Id);
            builder.Entity<PromotionValidationResult>().IsRoot(x => x.Id);
            builder.Entity<PromotionSimulation>().IsRoot(x => x.Id);
            builder.Entity<PromotionSimulationScenario>().IsRoot(x => x.Id);
            builder.Entity<PromotionSimulationResult>().IsRoot(x => x.Id);

            // Audits - flat, platform-level audit-log entities, no aggregate root
            builder.Entity<PromotionAudit>().IsRoot(x => x.Id);
            builder.Entity<RuleAudit>().IsRoot(x => x.Id);
            builder.Entity<ApprovalAudit>().IsRoot(x => x.Id);
            builder.Entity<ExecutionAudit>().IsRoot(x => x.Id);
        });

        return services;
    }

    // First searchable resource is public Coupon discovery (Phase 3.4) - the same
    // AddElasticsearchClient + one indexer + one repository shape ProductSearch/UserSearch use.
    // Future searchable Promotion resources register the same way, one more scoped pair each.
    private static IServiceCollection AddPromotionSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddElasticsearchClient(configuration);
        services.AddScoped<ICouponSearchIndexer, CouponSearchIndexer>();
        services.AddScoped<ICouponSearchRepository, CouponSearchRepository>();

        return services;
    }
}
