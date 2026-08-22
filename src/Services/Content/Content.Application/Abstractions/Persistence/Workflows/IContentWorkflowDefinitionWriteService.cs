namespace NovaCore.Content.Application.Abstractions.Persistence.Workflows;

public interface IContentWorkflowDefinitionWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - a brand-new definition is a single-aggregate write.</summary>
    Task CreateAsync(ContentWorkflowDefinition definition, CancellationToken ct = default);
}
