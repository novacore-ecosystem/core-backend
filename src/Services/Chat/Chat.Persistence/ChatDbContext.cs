using NovaCore.BuildingBlock.Persistence.Ef.DbContext;

namespace NovaCore.Chat.Persistence;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContextBase(options)
{
    // Aggregate Roots
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Contact> Contacts { get; set; } = null!;
    public DbSet<ConversationQueue> ConversationQueues { get; set; } = null!;
    public DbSet<ConversationAssignment> ConversationAssignments { get; set; } = null!;
    public DbSet<ConversationTransferRequest> ConversationTransferRequests { get; set; } = null!;
    public DbSet<ConversationResponsibilityHistory> ConversationResponsibilityHistories { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Poll> Polls { get; set; } = null!;
    public DbSet<ConversationTask> ConversationTasks { get; set; } = null!;
    public DbSet<ConversationSchedule> ConversationSchedules { get; set; } = null!;
    public DbSet<ConversationRole> ConversationRoles { get; set; } = null!;
    public DbSet<ConversationPermission> ConversationPermissions { get; set; } = null!;
    public DbSet<Sticker> Stickers { get; set; } = null!;

    // Owned & mapping entities
    public DbSet<ConversationContact> ConversationContacts { get; set; } = null!;
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;
    public DbSet<ConversationQueueItem> ConversationQueueItems { get; set; } = null!;
    public DbSet<ConversationTag> ConversationTags { get; set; } = null!;
    public DbSet<MessageAttachment> MessageAttachments { get; set; } = null!;
    public DbSet<MessageReference> MessageReferences { get; set; } = null!;
    public DbSet<MessageReaction> MessageReactions { get; set; } = null!;
    public DbSet<MessageMention> MessageMentions { get; set; } = null!;
    public DbSet<MessageDelivery> MessageDeliveries { get; set; } = null!;
    public DbSet<ConversationReadState> ConversationReadStates { get; set; } = null!;
    public DbSet<ConversationPinnedMessage> ConversationPinnedMessages { get; set; } = null!;
    public DbSet<ConversationNote> ConversationNotes { get; set; } = null!;
    public DbSet<PollOption> PollOptions { get; set; } = null!;
    public DbSet<PollVote> PollVotes { get; set; } = null!;
    public DbSet<ConversationParticipantRole> ConversationParticipantRoles { get; set; } = null!;
    public DbSet<ConversationRolePermission> ConversationRolePermissions { get; set; } = null!;
    public DbSet<ConversationNotificationPreference> ConversationNotificationPreferences { get; set; } = null!;

    // Translations
    public DbSet<TagTranslation> TagTranslations { get; set; } = null!;
    public DbSet<ConversationRoleTranslation> ConversationRoleTranslations { get; set; } = null!;
    public DbSet<ConversationPermissionTranslation> ConversationPermissionTranslations { get; set; } = null!;
    public DbSet<StickerTranslation> StickerTranslations { get; set; } = null!;
}
