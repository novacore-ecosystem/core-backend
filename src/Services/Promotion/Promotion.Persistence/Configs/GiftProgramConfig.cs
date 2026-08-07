using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class GiftProgramConfig : IEntityTypeConfiguration<GiftProgram>
{
    public void Configure(EntityTypeBuilder<GiftProgram> builder)
    {
        // Table
        builder.ToTable("gift_programs");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        // Items is configured from GiftItemConfig's side (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
