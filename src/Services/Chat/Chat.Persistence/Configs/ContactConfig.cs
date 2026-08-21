using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ContactConfig : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        // Table
        builder.ToTable("contacts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId);
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();

        builder.Property(x => x.Email)
            .HasConversion(
                x => x == null ? null : x.Value,
                x => x == null ? null : Email.Create(x))
            .HasMaxLength(100);

        builder.Property(x => x.Phone)
            .HasConversion(
                x => x == null ? null : x.Value,
                x => x == null ? null : PhoneNumber.Create(x))
            .HasMaxLength(30);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.UserId);
    }
}
