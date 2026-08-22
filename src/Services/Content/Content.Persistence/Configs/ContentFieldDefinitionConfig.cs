using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentFieldDefinitionConfig : IEntityTypeConfiguration<ContentFieldDefinition>
{
    public void Configure(EntityTypeBuilder<ContentFieldDefinition> builder)
    {
        // Table
        builder.ToTable("content_field_definitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentTypeId).IsRequired();

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => ContentKey.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.FieldType)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.IsRequired).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsLocalized).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsSearchable).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsSortable).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DefaultValue).HasMaxLength(2000);
        builder.Property(x => x.ValidationConfiguration).HasColumnType("jsonb");
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.ContentType)
            .WithMany(t => t.FieldDefinitions)
            .HasForeignKey(x => x.ContentTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.ContentTypeId, x.Key }).IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
