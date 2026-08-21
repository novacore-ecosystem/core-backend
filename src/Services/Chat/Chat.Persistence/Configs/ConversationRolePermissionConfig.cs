using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationRolePermissionConfig : IEntityTypeConfiguration<ConversationRolePermission>
{
    public void Configure(EntityTypeBuilder<ConversationRolePermission> builder)
    {
        // Table
        builder.ToTable("conversation_role_permissions");

        // Properties
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        // Relationships
        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PermissionId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
