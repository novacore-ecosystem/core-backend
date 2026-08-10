using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.BuildingBlock.Domain.Tests.Exceptions;

public class ExceptionFactoryTests
{
    public static TheoryData<Func<string, DomainException>, Type> InvalidArgumentFactories => new()
    {
        { ExceptionFactory.InvalidEnumValue, typeof(InvalidArgumentException) },
        { ExceptionFactory.InvalidRange, typeof(InvalidArgumentException) },
        { ExceptionFactory.ValueTooSmall, typeof(InvalidArgumentException) },
        { ExceptionFactory.ValueTooLarge, typeof(InvalidArgumentException) },
        { ExceptionFactory.InvalidFormat, typeof(InvalidArgumentException) },
        { ExceptionFactory.RequiredField, typeof(InvalidArgumentException) },
        { ExceptionFactory.RequiredNotEmpty, typeof(InvalidArgumentException) },
        { ExceptionFactory.InvalidState, typeof(InvalidStateException) },
        { ExceptionFactory.InvalidStatus, typeof(InvalidStatusException) },
        { ExceptionFactory.EmptyCollection, typeof(EmptyCollectionException) },
        { ExceptionFactory.EmptyItems, typeof(EmptyItemsException) },
    };

    [Theory]
    [MemberData(nameof(InvalidArgumentFactories))]
    public void Factories_MappingToInvalidInput_ProduceCorrectTypeCodeAndMessage(
        Func<string, DomainException> factory, Type expectedType)
    {
        var exception = factory("system message");

        exception.ShouldBeOfType(expectedType);
        exception.MessageCode.ShouldBe(MessageCode.InvalidInput);
        exception.SystemMessage.ShouldBe("system message");
    }

    public static TheoryData<Func<string, DomainException>> InsufficientAmountFactories => new()
    {
        // Wrapped in a lambda, not passed as a bare method group: InsufficientStock now has an
        // optional trailing `detail` parameter (see docs/tasks/2026-07-27/Task20_...), and a
        // method group with extra optional parameters no longer implicitly converts to a
        // narrower Func<string, DomainException> delegate.
        s => ExceptionFactory.InsufficientStock(s),
        ExceptionFactory.InsufficientBalance,
        ExceptionFactory.InsufficientQuota,
    };

    [Theory]
    [MemberData(nameof(InsufficientAmountFactories))]
    public void Factories_InsufficientAmount_ProduceInsufficientAmountExceptionWithInsufficientStockCode(
        Func<string, DomainException> factory)
    {
        var exception = factory("not enough");

        exception.ShouldBeOfType<InsufficientAmountException>();
        exception.MessageCode.ShouldBe(MessageCode.InsufficientStock);
        exception.SystemMessage.ShouldBe("not enough");
    }

    public static TheoryData<Func<string, DomainException>> BusinessRuleFactories => new()
    {
        ExceptionFactory.Duplicate,
        ExceptionFactory.UniqueConstraintViolation,
    };

    [Theory]
    [MemberData(nameof(BusinessRuleFactories))]
    public void Factories_BusinessRule_ProduceBusinessRuleExceptionWithBadRequestCode(
        Func<string, DomainException> factory)
    {
        var exception = factory("rule broken");

        exception.ShouldBeOfType<BusinessRuleException>();
        exception.MessageCode.ShouldBe(MessageCode.BadRequest);
        exception.SystemMessage.ShouldBe("rule broken");
    }

    [Fact]
    public void EntityNotFound_WithMessage_ProducesNotFoundCode()
    {
        var exception = ExceptionFactory.EntityNotFound("missing");

        exception.MessageCode.ShouldBe(MessageCode.NotFound);
        exception.SystemMessage.ShouldBe("missing");
    }

    [Fact]
    public void EntityNotFound_ThrownDirectly_MatchesTestKitDomainExceptionHelper()
    {
        void Act() => throw ExceptionFactory.EntityNotFound("missing");

        ((Action)Act).ShouldThrowDomainException<EntityNotFoundException>(MessageCode.NotFound);
    }

    [Fact]
    public void EntityNotFound_Generic_BuildsMessageFromTypeAndId()
    {
        var id = Guid.NewGuid();

        var exception = ExceptionFactory.EntityNotFound<ExceptionFactoryTests>(id);

        exception.MessageCode.ShouldBe(MessageCode.NotFound);
        exception.SystemMessage.ShouldBe($"Related {nameof(ExceptionFactoryTests)} with id {id} not found");
    }
}
