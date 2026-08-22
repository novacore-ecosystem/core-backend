using NovaCore.Content.Application.Abstractions.Persistence.Workflows;
using NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Repositories;

namespace NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Read;

public sealed class ContentWorkflowDefinitionReadService(IContentWorkflowDefinitionRepository definitionRepo) : IContentWorkflowDefinitionReadService
{
    public async Task<ContentWorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await definitionRepo.GetByIdAsync(
            id,
            query => query.Include(d => d.States).Include(d => d.Transitions),
            ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await definitionRepo.ExistsByIdAsync(id, ct);
    }
}
