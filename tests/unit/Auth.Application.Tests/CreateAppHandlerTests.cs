using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Features.Apps.Commands.CreateApp;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class CreateAppHandlerTests
{
    private static IUnitOfWork BuildUnitOfWork()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });
        return uow;
    }

    [Fact]
    public async Task Handle_CreatesApp_WhenCodeIsUnique()
    {
        var unitOfWork = BuildUnitOfWork();
        var readService = Substitute.For<IAppReadService>();
        var code = AppCode.Create("storefront_web");
        readService.ExistsByCodeAsync(code, Arg.Any<CancellationToken>()).Returns(false);
        var writeService = Substitute.For<IAppWriteService>();
        App? created = null;
        writeService.CreateAsync(Arg.Do<App>(a => created = a), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new CreateAppHandler(
            unitOfWork, readService, writeService, Substitute.For<IAppCollectionCache>());

        var id = await handler.Handle(new CreateAppCommand("storefront_web", "Storefront Web"));

        id.ShouldNotBe(Guid.Empty);
        created.ShouldNotBeNull();
        created!.Code.Value.ShouldBe("storefront_web");
        created.Name.ShouldBe("Storefront Web");
        created.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenCodeAlreadyExists()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var readService = Substitute.For<IAppReadService>();
        var code = AppCode.Create("storefront_web");
        readService.ExistsByCodeAsync(code, Arg.Any<CancellationToken>()).Returns(true);
        var writeService = Substitute.For<IAppWriteService>();
        var handler = new CreateAppHandler(
            unitOfWork, readService, writeService, Substitute.For<IAppCollectionCache>());

        await Should.ThrowAsync<ConflictException>(
            () => handler.Handle(new CreateAppCommand("storefront_web", "Storefront Web")));

        await writeService.DidNotReceive().CreateAsync(Arg.Any<App>(), Arg.Any<CancellationToken>());
    }
}
