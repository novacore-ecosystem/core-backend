using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

namespace NovaCore.Content.Persistence.Engine;

public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    // Aggregate Roots
    public DbSet<ContentEntity> Contents { get; set; } = null!;
    public DbSet<ContentType> ContentTypes { get; set; } = null!;
    public DbSet<ContentWorkflowDefinition> ContentWorkflowDefinitions { get; set; } = null!;
    public DbSet<ContentTaxonomy> ContentTaxonomies { get; set; } = null!;

    // Owned & child entities
    public DbSet<ContentVersion> ContentVersions { get; set; } = null!;
    public DbSet<ContentPublication> ContentPublications { get; set; } = null!;
    public DbSet<ContentWorkflowInstance> ContentWorkflowInstances { get; set; } = null!;
    public DbSet<ContentRelationship> ContentRelationships { get; set; } = null!;
    public DbSet<ContentLocalization> ContentLocalizations { get; set; } = null!;
    public DbSet<ContentTaxonomyAssignment> ContentTaxonomyAssignments { get; set; } = null!;
    public DbSet<ContentAudience> ContentAudiences { get; set; } = null!;
    public DbSet<ContentContributor> ContentContributors { get; set; } = null!;
    public DbSet<ContentFieldDefinition> ContentFieldDefinitions { get; set; } = null!;
    public DbSet<ContentWorkflowState> ContentWorkflowStates { get; set; } = null!;
    public DbSet<ContentWorkflowTransition> ContentWorkflowTransitions { get; set; } = null!;

    // System Tables
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
}
