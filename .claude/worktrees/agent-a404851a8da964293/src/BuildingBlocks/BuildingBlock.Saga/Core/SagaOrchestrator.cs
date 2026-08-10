using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Saga.Abstractions;

namespace NovaCore.BuildingBlock.Saga.Core;

/// <summary>
/// Orchestrates saga execution with automatic compensation on failure.
/// Executes steps in order and rolls back completed steps if any step fails.
/// </summary>
public sealed class SagaOrchestrator : ISagaOrchestrator
{
    private readonly IAppLogger<SagaOrchestrator> _logger;
    private readonly ISagaStore? _sagaStore;

    public SagaOrchestrator(IAppLogger<SagaOrchestrator> logger, ISagaStore? sagaStore = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sagaStore = sagaStore;
    }

    public async Task<ISagaContext> ExecuteAsync(
        ISagaDefinition sagaDefinition,
        ISagaContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sagaDefinition);
        ArgumentNullException.ThrowIfNull(context);

        _logger.Information(
            "Starting saga '{SagaName}' with ID '{SagaId}' and correlation ID '{CorrelationId}'",
            sagaDefinition.SagaName,
            context.SagaId,
            context.CorrelationId);

        context.State = SagaExecutionState.Running;

        try
        {
            var steps = sagaDefinition.GetSteps();

            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logger.Information(
                        "Executing step '{StepName}' in saga '{SagaName}'",
                        step.StepName,
                        sagaDefinition.SagaName);

                    await step.ExecuteAsync(context, cancellationToken);

                    context.MarkStepCompleted(step.StepName);

                    _logger.Information(
                        "Step '{StepName}' completed successfully",
                        step.StepName);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        ex,
                        "Step '{StepName}' failed in saga '{SagaName}'",
                        step.StepName,
                        sagaDefinition.SagaName);

                    context.State = SagaExecutionState.Failed;
                    await CompensateAsync(sagaDefinition, context, cancellationToken);

                    var sagaException = new SagaExecutionException(
                        sagaDefinition.SagaName,
                        step.StepName,
                        context.SagaId,
                        $"Saga '{sagaDefinition.SagaName}' failed at step '{step.StepName}': {ex.Message}",
                        ex);

                    await SaveSagaAsync(sagaDefinition, context, sagaException.Message, cancellationToken);

                    throw sagaException;
                }
            }

            context.State = SagaExecutionState.Succeeded;
            await SaveSagaAsync(sagaDefinition, context, null, cancellationToken);

            _logger.Information(
                "Saga '{SagaName}' with ID '{SagaId}' completed successfully",
                sagaDefinition.SagaName,
                context.SagaId);

            return context;
        }
        catch (OperationCanceledException)
        {
            _logger.Warning(
                "Saga '{SagaName}' with ID '{SagaId}' was cancelled",
                sagaDefinition.SagaName,
                context.SagaId);
            throw;
        }
    }

    private async Task CompensateAsync(
        ISagaDefinition sagaDefinition,
        ISagaContext context,
        CancellationToken cancellationToken)
    {
        _logger.Information(
            "Starting compensation for saga '{SagaName}' with ID '{SagaId}'",
            sagaDefinition.SagaName,
            context.SagaId);

        context.State = SagaExecutionState.Compensating;
        var completedSteps = context.GetCompletedSteps();

        for (int i = completedSteps.Count - 1; i >= 0; i--)
        {
            var stepName = completedSteps[i];
            var step = sagaDefinition.GetStep(stepName);

            if (step == null)
            {
                _logger.Warning(
                    "Could not find step '{StepName}' for compensation",
                    stepName);
                continue;
            }

            try
            {
                _logger.Information(
                    "Compensating step '{StepName}' in saga '{SagaName}'",
                    stepName,
                    sagaDefinition.SagaName);

                await step.CompensateAsync(context, cancellationToken);

                _logger.Information(
                    "Step '{StepName}' compensated successfully",
                    stepName);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Compensation failed for step '{StepName}' in saga '{SagaName}'",
                    stepName,
                    sagaDefinition.SagaName);
            }
        }

        context.State = SagaExecutionState.Compensated;

        _logger.Information(
            "Compensation completed for saga '{SagaName}' with ID '{SagaId}'",
            sagaDefinition.SagaName,
            context.SagaId);
    }

    private async Task SaveSagaAsync(
        ISagaDefinition sagaDefinition,
        ISagaContext context,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        if (_sagaStore == null)
            return;

        try
        {
            var record = new SagaExecutionRecord
            {
                SagaId = context.SagaId,
                SagaName = sagaDefinition.SagaName,
                State = context.State,
                StartedAt = context.Metadata.StartedAt,
                CompletedAt = DateTime.UtcNow,
                FailureReason = failureReason,
                CompletedSteps = [..context.GetCompletedSteps()],
                ContextData = new Dictionary<string, object?>(context.GetAllData()),
                CorrelationId = context.CorrelationId,
                UserId = context.Metadata.UserId
            };

            await _sagaStore.SaveAsync(record, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Failed to save saga execution record for '{SagaName}'",
                sagaDefinition.SagaName);
        }
    }
}
