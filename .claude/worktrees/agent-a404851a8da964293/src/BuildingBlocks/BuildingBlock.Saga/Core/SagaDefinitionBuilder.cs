using NovaCore.BuildingBlock.Saga.Abstractions;

namespace NovaCore.BuildingBlock.Saga.Core;

/// <summary>
/// Fluent builder for defining sagas using chain-of-responsibility pattern.
/// </summary>
public sealed class SagaDefinitionBuilder
{
    private readonly string _sagaName;
    private string _description = string.Empty;
    private readonly List<ISagaStep> _steps = [];

    public SagaDefinitionBuilder(string sagaName)
    {
        _sagaName = sagaName;
    }

    public SagaDefinitionBuilder Description(string description)
    {
        _description = description;
        return this;
    }

    public SagaDefinitionBuilder AddStep(ISagaStep step)
    {
        _steps.Add(step);
        return this;
    }

    public SagaDefinitionBuilder AddSteps(params ISagaStep[] steps)
    {
        _steps.AddRange(steps);
        return this;
    }

    public ISagaDefinition Build()
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException($"Saga '{_sagaName}' must have at least one step");

        return new SagaDefinition(_sagaName, _description, _steps);
    }

    private sealed class SagaDefinition : ISagaDefinition
    {
        private readonly Dictionary<string, ISagaStep> _stepMap = [];

        public string SagaName { get; }
        public string Description { get; }

        private readonly List<ISagaStep> _steps;

        public SagaDefinition(string sagaName, string description, List<ISagaStep> steps)
        {
            SagaName = sagaName;
            Description = description;
            _steps = steps;

            foreach (var step in _steps)
                _stepMap[step.StepName] = step;
        }

        public IReadOnlyList<ISagaStep> GetSteps() => _steps.AsReadOnly();

        public ISagaStep? GetStep(string stepName) =>
            _stepMap.TryGetValue(stepName, out var step) ? step : null;
    }
}
