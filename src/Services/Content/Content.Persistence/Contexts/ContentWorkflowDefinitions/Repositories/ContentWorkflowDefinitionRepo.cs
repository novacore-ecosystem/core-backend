using NovaCore.Content.Persistence.Contexts;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Repositories;

public sealed class ContentWorkflowDefinitionRepo(ContentDbContext dbContext)
    : ContentBaseRepository<ContentWorkflowDefinition, Guid>(dbContext), IContentWorkflowDefinitionRepository
{
}
