using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationProofConfig : IEntityTypeConfiguration<TransportationProof>
{
    public void Configure(EntityTypeBuilder<TransportationProof> builder)
    {
        // Table
        builder.ToTable("transportation_proofs");

        // Properties
        // Shared-PK 1:1 with Transportation - see TransportationAssignmentConfig for the same shape.
        builder.HasKey(x => x.TransportationId);

        builder.Property(x => x.TransportationId).ValueGeneratedNever();
        builder.Property(x => x.ReceivedByName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SignatureUrl).HasMaxLength(1000);
        builder.Property(x => x.PhotoUrl).HasMaxLength(1000);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.CapturedAt).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<Transportation>()
            .WithOne(t => t.Proof)
            .HasForeignKey<TransportationProof>(x => x.TransportationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
