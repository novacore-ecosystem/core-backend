using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointExpirationConfig : IEntityTypeConfiguration<PointExpiration>
{
    public void Configure(EntityTypeBuilder<PointExpiration> builder)
    {
        // Table
        builder.ToTable("point_expirations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Points).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(x => x.Expirations)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.AccountId);
    }
}
