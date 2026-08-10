using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class IdempotencyRecordConfig : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        // Table
        builder.ToTable("idempotency_records");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RequestHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ResponseSnapshot).HasColumnType("jsonb");
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}
