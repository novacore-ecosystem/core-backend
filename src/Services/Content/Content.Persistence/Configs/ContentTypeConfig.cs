using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentTypeConfig : IEntityTypeConfiguration<ContentType>
{
    public void Configure(EntityTypeBuilder<ContentType> builder)
    {
        // Table
        builder.ToTable("content_types");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => ContentKey.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.SchemaVersion)
            .IsRequired()
            .HasDefaultValue(1);

        // Relationships
        // FieldDefinitions is configured from ContentFieldDefinitionConfig (single source).

        // Indexes
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
