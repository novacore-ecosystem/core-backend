using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointHistoryConfig : IEntityTypeConfiguration<PointHistory>
{
    public void Configure(EntityTypeBuilder<PointHistory> builder)
    {
        // Table
        builder.ToTable("point_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.AccountId);
    }
}
