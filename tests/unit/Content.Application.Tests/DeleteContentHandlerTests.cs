using NovaCore.Content.Application.Features.Contents.Commands.DeleteContent;

namespace NovaCore.Content.Application.Tests;

public sealed class DeleteContentHandlerTests
{
    private static (IContentReadService ReadService, IContentWriteService WriteService, DeleteContentHandler Handler) BuildHandler(bool exists)
    {
        var readService = Substitute.For<IContentReadService>();
        readService.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(exists);

        var writeService = Substitute.For<IContentWriteService>();
        writeService.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new DeleteContentHandler(readService, writeService, uow, Substitute.For<IOutboxStore>());
        return (readService, writeService, handler);
    }

    [Fact]
    public async Task Handle_ExistingContent_CallsWriteServiceDeleteAsync()
    {
        var (_, writeService, handler) = BuildHandler(exists: true);
        var contentId = Guid.CreateVersion7();

        var response = await handler.Handle(new DeleteContentCommand(contentId));

        response.ContentId.ShouldBe(contentId);
        await writeService.Received(1).DeleteAsync(contentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownContent_ThrowsNotFound()
    {
        var (_, writeService, handler) = BuildHandler(exists: false);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new DeleteContentCommand(Guid.CreateVersion7())));

        await writeService.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
