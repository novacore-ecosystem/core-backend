using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class DistributionJobConfig : IEntityTypeConfiguration<DistributionJob>
{
    public void Configure(EntityTypeBuilder<DistributionJob> builder)
    {
        // Table
        builder.ToTable("distribution_jobs");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.Strategy).HasConversion<short>().IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        // Batches/History are configured from the child entity's own config (single source per
        // relationship).

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
