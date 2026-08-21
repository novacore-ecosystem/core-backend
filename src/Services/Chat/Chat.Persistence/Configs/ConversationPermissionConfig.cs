using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationPermissionConfig : IEntityTypeConfiguration<ConversationPermission>
{
    public void Configure(EntityTypeBuilder<ConversationPermission> builder)
    {
        // Table
        builder.ToTable("conversation_permissions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.IsSystem).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
