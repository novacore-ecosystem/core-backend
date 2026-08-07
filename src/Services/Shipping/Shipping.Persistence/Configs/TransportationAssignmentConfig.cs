using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationAssignmentConfig : IEntityTypeConfiguration<TransportationAssignment>
{
    public void Configure(EntityTypeBuilder<TransportationAssignment> builder)
    {
        // Table
        builder.ToTable("transportation_assignments");

        // Properties
        // Shared-PK 1:1: the child's key IS the parent's id, no surrogate id and no extra unique
        // index needed to fake uniqueness (see domain-coding-conventions.md rule 5).
        builder.HasKey(x => x.TransportationId);

        builder.Property(x => x.TransportationId).ValueGeneratedNever();
        builder.Property(x => x.AssignedAt).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<Transportation>()
            .WithOne(t => t.Assignment)
            .HasForeignKey<TransportationAssignment>(x => x.TransportationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PersonId);
        builder.HasIndex(x => x.VehicleId);
    }
}
