using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Repositories;

public interface IContentWorkflowDefinitionRepository : IRepository<ContentWorkflowDefinition, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
