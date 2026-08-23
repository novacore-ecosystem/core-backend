using NovaCore.Content.Application.Features.Contents.Commands.UpdateContentDraft;
using NovaCore.Content.Domain.ValueObjects;

namespace NovaCore.Content.Application.Tests;

public sealed class UpdateContentDraftHandlerTests
{
    private static ContentEntity BuildContent(out Guid versionId)
    {
        var content = ContentEntity.Create(
            Guid.CreateVersion7(), ContentSlug.Create("first-article"), LanguageCode.Create("en"),
            "Title", "Summary", "{}", Guid.CreateVersion7());
        versionId = content.CurrentVersionId!.Value;
        return content;
    }

    private static (IContentWriteService WriteService, UpdateContentDraftHandler Handler) BuildHandler(ContentEntity? content)
    {
        var readService = Substitute.For<IContentReadService>();
        readService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(content);

        var writeService = Substitute.For<IContentWriteService>();
        writeService.UpsertLocalizationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<LanguageCode>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ContentMetadata?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new UpdateContentDraftHandler(readService, writeService, uow);
        return (writeService, handler);
    }

    [Fact]
    public async Task Handle_ExistingVersion_CallsUpsertLocalization()
    {
        var content = BuildContent(out var versionId);
        var (writeService, handler) = BuildHandler(content);

        var response = await handler.Handle(new UpdateContentDraftCommand(
            content.Id, versionId, "en", "New Title", "New Summary", "{}", Guid.CreateVersion7()));

        response.Language.ShouldBe("en");
        await writeService.Received(1).UpsertLocalizationAsync(
            content.Id, versionId, Arg.Is<LanguageCode>(l => l.Value == "en"), "New Title", "New Summary", "{}",
            Arg.Any<Guid>(), (ContentMetadata?)null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownVersion_ThrowsNotFound()
    {
        var content = BuildContent(out _);
        var (writeService, handler) = BuildHandler(content);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new UpdateContentDraftCommand(
            content.Id, Guid.CreateVersion7(), "en", "Title", "Summary", "{}", Guid.CreateVersion7())));

        await writeService.DidNotReceive().UpsertLocalizationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<LanguageCode>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ContentMetadata?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownContent_ThrowsNotFound()
    {
        var (_, handler) = BuildHandler(null);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new UpdateContentDraftCommand(
            Guid.CreateVersion7(), Guid.CreateVersion7(), "en", "Title", "Summary", "{}", Guid.CreateVersion7())));
    }
}
