using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Auth.Domain.Entities.Scopes;
using NovaCore.Auth.Domain.Metadata;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class ScopeConfig : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        // Table
        builder.ToTable("scopes");

        // Properties
        builder.HasKey(x => x.Id);

        // TenantId itself is mapped/indexed/query-filtered automatically by the Entity
        // Convention (see ModelBuilderExtensions.ApplyEntityConventions) - only the FK
        // relationship to Tenant and the (TenantId, Code) uniqueness are Scope-specific.
        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => ScopeCode.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<ScopeMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Path)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Level)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing hierarchy - no Domain-level Parent/Children navigation (see
        // Scope class doc comment), so this is a shadow-navigation FK only, same shape as
        // ProductCategory's ParentCategoryId.
        builder.HasOne<Scope>()
            .WithMany()
            .HasForeignKey(x => x.ParentScopeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Scope)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();
        builder.HasIndex(x => x.ParentScopeId);
        builder.HasIndex(x => x.IsActive);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
