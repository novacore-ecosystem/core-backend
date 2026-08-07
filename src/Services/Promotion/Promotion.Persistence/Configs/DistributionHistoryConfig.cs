using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class DistributionHistoryConfig : IEntityTypeConfiguration<DistributionHistory>
{
    public void Configure(EntityTypeBuilder<DistributionHistory> builder)
    {
        // Table
        builder.ToTable("distribution_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Job)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.JobId);
    }
}
