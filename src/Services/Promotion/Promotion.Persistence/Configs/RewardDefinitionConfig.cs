using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RewardDefinitionConfig : IEntityTypeConfiguration<RewardDefinition>
{
    public void Configure(EntityTypeBuilder<RewardDefinition> builder)
    {
        // Table
        builder.ToTable("reward_definitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RewardType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.Definitions)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProgramId);
    }
}
