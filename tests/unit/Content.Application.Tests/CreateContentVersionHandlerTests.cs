using NovaCore.Content.Application.Features.Contents.Commands.CreateContentVersion;

namespace NovaCore.Content.Application.Tests;

public sealed class CreateContentVersionHandlerTests
{
    private static (IContentWriteService WriteService, CreateContentVersionHandler Handler) BuildHandler(
        bool exists, (Guid Id, int VersionNumber) result = default)
    {
        var readService = Substitute.For<IContentReadService>();
        readService.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(exists);

        var writeService = Substitute.For<IContentWriteService>();
        writeService.CreateDraftVersionAsync(
            Arg.Any<Guid>(), Arg.Any<LanguageCode>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Guid>(), Arg.Any<ContentMetadata?>(), Arg.Any<CancellationToken>())
            .Returns(result);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new CreateContentVersionHandler(readService, writeService, uow);
        return (writeService, handler);
    }

    [Fact]
    public async Task Handle_ExistingContent_ReturnsNewVersionIdAndNumber()
    {
        var newVersionId = Guid.CreateVersion7();
        var (_, handler) = BuildHandler(exists: true, result: (newVersionId, 2));
        var contentId = Guid.CreateVersion7();

        var response = await handler.Handle(new CreateContentVersionCommand(contentId, "en", "Title", "Summary", "{}", Guid.CreateVersion7()));

        response.VersionId.ShouldBe(newVersionId);
        response.VersionNumber.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_UnknownContent_ThrowsNotFound()
    {
        var (writeService, handler) = BuildHandler(exists: false);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new CreateContentVersionCommand(
            Guid.CreateVersion7(), "en", "Title", "Summary", "{}", Guid.CreateVersion7())));

        await writeService.DidNotReceive().CreateDraftVersionAsync(
            Arg.Any<Guid>(), Arg.Any<LanguageCode>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Guid>(), Arg.Any<ContentMetadata?>(), Arg.Any<CancellationToken>());
    }
}
