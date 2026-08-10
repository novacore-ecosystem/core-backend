using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class ShipmentConfig : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        // Table
        builder.ToTable("shipments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShipmentNumber)
            .HasConversion(x => x.Value, x => ShipmentNumber.Create(x))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ShipmentType).HasConversion<short>().IsRequired();
        builder.Property(x => x.SourceType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.SourceReferenceId).IsRequired();

        builder.Property(x => x.SenderName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ReceiverName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SenderPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.ReceiverPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.DeclaredValue)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);

        builder.OwnsShippingAddress(x => x.SenderAddress, "sender_address", required: true);
        builder.OwnsShippingAddress(x => x.ReceiverAddress, "receiver_address", required: true);

        builder.ConfigureCommonFields();

        // Relationships
        // Items/Events/Packages have no independent identity outside their Shipment but are normal
        // related entities (own table, own PK, FK back to Shipment) - not EF owned types, matching
        // Order's post-refactor convention. Nothing is auto-loaded: every read path Includes
        // explicitly.
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Events)
            .WithOne()
            .HasForeignKey(e => e.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Packages)
            .WithOne()
            .HasForeignKey(p => p.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ShipmentNumber).IsUnique();
        builder.HasIndex(x => new { x.SourceType, x.SourceReferenceId });
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => x.CreatedAt);
    }
}
