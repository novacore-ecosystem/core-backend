using NovaCore.BuildingBlock.Domain.Exceptions;

using NovaCore.Content.Application.Features.Contents.Commands.RestoreContent;

namespace NovaCore.Content.Application.Tests;

public sealed class RestoreContentHandlerTests
{
    private static ContentEntity BuildDeletedContent()
    {
        var content = ContentEntity.Create(
            Guid.CreateVersion7(), ContentSlug.Create("first-article"), LanguageCode.Create("en"),
            "Title", "Summary", "{}", Guid.CreateVersion7());
        content.Delete();
        return content;
    }

    private static (IContentWriteService WriteService, RestoreContentHandler Handler) BuildHandler(ContentEntity? content)
    {
        var readService = Substitute.For<IContentReadService>();
        readService.GetByIdIncludingDeletedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(content);

        var writeService = Substitute.For<IContentWriteService>();
        writeService.RestoreAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new RestoreContentHandler(readService, writeService, uow);
        return (writeService, handler);
    }

    [Fact]
    public async Task Handle_DeletedContent_CallsWriteServiceRestoreAsync()
    {
        var content = BuildDeletedContent();
        var (writeService, handler) = BuildHandler(content);

        var response = await handler.Handle(new RestoreContentCommand(content.Id));

        response.ContentId.ShouldBe(content.Id);
        await writeService.Received(1).RestoreAsync(content.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotDeletedContent_ThrowsInvalidState()
    {
        var content = ContentEntity.Create(
            Guid.CreateVersion7(), ContentSlug.Create("first-article"), LanguageCode.Create("en"),
            "Title", "Summary", "{}", Guid.CreateVersion7());
        var (writeService, handler) = BuildHandler(content);

        await Should.ThrowAsync<InvalidStateException>(() => handler.Handle(new RestoreContentCommand(content.Id)));

        await writeService.DidNotReceive().RestoreAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownContent_ThrowsNotFound()
    {
        var (_, handler) = BuildHandler(null);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new RestoreContentCommand(Guid.CreateVersion7())));
    }
}
