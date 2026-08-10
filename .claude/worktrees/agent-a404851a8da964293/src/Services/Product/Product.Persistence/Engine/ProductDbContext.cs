using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

namespace NovaCore.Product.Persistence.Engine;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    // Aggregate Roots
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
    public DbSet<ProductTag> ProductTags { get; set; } = null!;
    public DbSet<ProductBrand> ProductBrands { get; set; } = null!;
    public DbSet<ProductCollection> ProductCollections { get; set; } = null!;
    public DbSet<SpecificationGroup> SpecificationGroups { get; set; } = null!;
    public DbSet<SpecificationDefinition> SpecificationDefinitions { get; set; } = null!;
    public DbSet<WarrantyPolicy> WarrantyPolicies { get; set; } = null!;
    public DbSet<ProductOptionDefinition> ProductOptionDefinitions { get; set; } = null!;

    // Child Entities
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<ProductIdentifier> ProductIdentifiers { get; set; } = null!;
    public DbSet<ProductShipping> ProductShippings { get; set; } = null!;
    public DbSet<ProductOption> ProductOptions { get; set; } = null!;
    public DbSet<ProductOptionValue> ProductOptionValues { get; set; } = null!;
    public DbSet<ProductOptionValueDefinition> ProductOptionValueDefinitions { get; set; } = null!;
    public DbSet<ProductOptionDefinitionTranslation> ProductOptionDefinitionTranslations { get; set; } = null!;
    public DbSet<ProductOptionValueDefinitionTranslation> ProductOptionValueDefinitionTranslations { get; set; } = null!;
    public DbSet<VariantOptionValue> VariantOptionValues { get; set; } = null!;
    public DbSet<ProductBundleItem> ProductBundleItems { get; set; } = null!;
    public DbSet<ProductMedia> ProductMedia { get; set; } = null!;
    public DbSet<ProductSpecification> ProductSpecifications { get; set; } = null!;
    public DbSet<ProductRelation> ProductRelations { get; set; } = null!;
    public DbSet<ProductSeo> ProductSeos { get; set; } = null!;
    public DbSet<ProductCategoryMapping> ProductCategoryMappings { get; set; } = null!;
    public DbSet<ProductTagMapping> ProductTagMappings { get; set; } = null!;
    public DbSet<ProductCollectionMapping> ProductCollectionMappings { get; set; } = null!;
    public DbSet<SpecificationGroupTranslation> SpecificationGroupTranslations { get; set; } = null!;
    public DbSet<SpecificationDefinitionTranslation> SpecificationDefinitionTranslations { get; set; } = null!;

    // Translations
    public DbSet<ProductTranslation> ProductTranslations { get; set; } = null!;
    public DbSet<ProductVariantTranslation> ProductVariantTranslations { get; set; } = null!;
    public DbSet<ProductSeoTranslation> ProductSeoTranslations { get; set; } = null!;
    public DbSet<ProductCategoryTranslation> ProductCategoryTranslations { get; set; } = null!;
    public DbSet<ProductBrandTranslation> ProductBrandTranslations { get; set; } = null!;
    public DbSet<ProductCollectionTranslation> ProductCollectionTranslations { get; set; } = null!;
    public DbSet<ProductTagTranslation> ProductTagTranslations { get; set; } = null!;

    // System Tables
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        base.ConfigureModel(modelBuilder);
    }
}
