using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using Shouldly;

namespace NovaCore.TestKit.ShouldlyExtensions;

/// <summary>
/// Asserts both the exception type and the <see cref="MessageCode"/> in one call, so a domain
/// rule violation is verified by its actual client-facing contract, not just "some exception".
/// </summary>
public static class DomainExceptionShouldlyExtensions
{
    public static TException ShouldThrowDomainException<TException>(this Action action, MessageCode expectedMessageCode)
        where TException : DomainException
    {
        var exception = Should.Throw<TException>(action);
        exception.MessageCode.ShouldBe(expectedMessageCode);
        return exception;
    }
}
