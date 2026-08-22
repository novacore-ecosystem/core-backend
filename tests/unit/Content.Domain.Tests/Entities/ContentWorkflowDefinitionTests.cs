using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Content.Domain.Entities.Workflows;
using NovaCore.Content.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Content.Domain.Tests.Entities;

public class ContentWorkflowDefinitionTests
{
    private static ContentWorkflowDefinition CreateValidDefinition()
        => ContentWorkflowDefinition.Create(ContentKey.Create("editorial"), "Editorial Workflow", "");

    [Fact]
    public void AddState_SecondInitialState_ThrowsInvalidState()
    {
        var definition = CreateValidDefinition();
        definition.AddState(ContentKey.Create("draft"), "Draft", "", isInitial: true);

        Action act = () => definition.AddState(ContentKey.Create("in-review"), "In Review", "", isInitial: true);

        act.ShouldThrowDomainException<InvalidStateException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void AddTransition_BetweenKnownStates_Succeeds()
    {
        var definition = CreateValidDefinition();
        var draft = definition.AddState(ContentKey.Create("draft"), "Draft", "", isInitial: true);
        var published = definition.AddState(ContentKey.Create("published"), "Published", "", isFinal: true);

        definition.AddTransition(ContentKey.Create("publish"), "Publish", "", draft.Id, published.Id);

        definition.CanTransition(draft.Id, published.Id).ShouldBeTrue();
        definition.CanTransition(published.Id, draft.Id).ShouldBeFalse();
    }

    [Fact]
    public void AddTransition_UnknownFromState_ThrowsEntityNotFound()
    {
        var definition = CreateValidDefinition();
        var published = definition.AddState(ContentKey.Create("published"), "Published", "");

        Action act = () => definition.AddTransition(ContentKey.Create("publish"), "Publish", "", Guid.CreateVersion7(), published.Id);

        act.ShouldThrowDomainException<EntityNotFoundException>(MessageCode.NotFound);
    }

    [Fact]
    public void AddTransition_DuplicatePair_ThrowsDuplicate()
    {
        var definition = CreateValidDefinition();
        var draft = definition.AddState(ContentKey.Create("draft"), "Draft", "", isInitial: true);
        var published = definition.AddState(ContentKey.Create("published"), "Published", "", isFinal: true);
        definition.AddTransition(ContentKey.Create("publish"), "Publish", "", draft.Id, published.Id);

        Action act = () => definition.AddTransition(ContentKey.Create("publish-again"), "Publish Again", "", draft.Id, published.Id);

        act.ShouldThrowDomainException<BusinessRuleException>(MessageCode.BadRequest);
    }
}
