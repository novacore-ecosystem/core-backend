using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserTagMappingConfig : IEntityTypeConfiguration<UserTagMapping>
{
    public void Configure(EntityTypeBuilder<UserTagMapping> builder)
    {
        // Table
        builder.ToTable("user_tag_mappings");

        // Properties
        // Pure mapping entity - the pairing itself is the identity, no surrogate Id.
        builder.HasKey(x => new { x.UserId, x.TagId });

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.TagMappings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade - a UserTag still assigned to Users should not be deletable.
        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.TagId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
