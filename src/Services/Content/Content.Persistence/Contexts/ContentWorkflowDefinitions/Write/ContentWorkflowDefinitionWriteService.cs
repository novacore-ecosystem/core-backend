using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Content.Application.Abstractions.Persistence.Workflows;
using NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Repositories;

namespace NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Write;

public sealed class ContentWorkflowDefinitionWriteService(
    IContentWorkflowDefinitionRepository definitionRepo,
    IUnitOfWork unitOfWork) : IContentWorkflowDefinitionWriteService
{
    public async Task CreateAsync(ContentWorkflowDefinition definition, CancellationToken ct = default)
    {
        await definitionRepo.AddAsync(definition, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
