using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationParticipantRoleConfig : IEntityTypeConfiguration<ConversationParticipantRole>
{
    public void Configure(EntityTypeBuilder<ConversationParticipantRole> builder)
    {
        // Table
        builder.ToTable("conversation_participant_roles");

        // Properties
        builder.HasKey(x => new { x.ConversationId, x.UserId, x.RoleId });

        // Relationships
        // ConversationId+UserId points at ConversationParticipant's own composite key - kept as
        // plain columns (no navigation) to avoid a three-way composite-FK-to-composite-key nav.
        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => new { x.ConversationId, x.UserId });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
