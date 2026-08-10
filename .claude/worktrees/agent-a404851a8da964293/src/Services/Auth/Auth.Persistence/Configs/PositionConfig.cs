using NovaCore.Auth.Domain.Entities.Positions;
using NovaCore.Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class PositionConfig : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        // Table
        builder.ToTable("positions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => PositionCode.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsSystemPosition)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(x => x.Roles)
            .WithOne(r => r.Position)
            .HasForeignKey(r => r.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Position)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
