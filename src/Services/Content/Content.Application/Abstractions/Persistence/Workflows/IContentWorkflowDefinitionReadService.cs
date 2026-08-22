namespace NovaCore.Content.Application.Abstractions.Persistence.Workflows;

public interface IContentWorkflowDefinitionReadService
{
    Task<ContentWorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
