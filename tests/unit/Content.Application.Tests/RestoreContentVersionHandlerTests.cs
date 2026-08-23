using NovaCore.Content.Application.Features.Contents.Commands.RestoreContentVersion;

namespace NovaCore.Content.Application.Tests;

public sealed class RestoreContentVersionHandlerTests
{
    private static ContentEntity BuildContent(out Guid versionId)
    {
        var content = ContentEntity.Create(
            Guid.CreateVersion7(), ContentSlug.Create("first-article"), LanguageCode.Create("en"),
            "Title", "Summary", "{}", Guid.CreateVersion7());
        versionId = content.CurrentVersionId!.Value;
        return content;
    }

    private static (IContentWriteService WriteService, RestoreContentVersionHandler Handler) BuildHandler(
        ContentEntity? content, (Guid Id, int VersionNumber) result = default)
    {
        var readService = Substitute.For<IContentReadService>();
        readService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(content);

        var writeService = Substitute.For<IContentWriteService>();
        writeService.RestoreVersionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(result);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new RestoreContentVersionHandler(readService, writeService, uow);
        return (writeService, handler);
    }

    [Fact]
    public async Task Handle_ExistingVersion_ReturnsRestoredVersionIdAndNumber()
    {
        var content = BuildContent(out var versionId);
        var restoredId = Guid.CreateVersion7();
        var (_, handler) = BuildHandler(content, (restoredId, 2));

        var response = await handler.Handle(new RestoreContentVersionCommand(content.Id, versionId, Guid.CreateVersion7()));

        response.VersionId.ShouldBe(restoredId);
        response.VersionNumber.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_UnknownVersion_ThrowsNotFound()
    {
        var content = BuildContent(out _);
        var (writeService, handler) = BuildHandler(content);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new RestoreContentVersionCommand(
            content.Id, Guid.CreateVersion7(), Guid.CreateVersion7())));

        await writeService.DidNotReceive().RestoreVersionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
