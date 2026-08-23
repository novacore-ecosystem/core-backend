using NovaCore.Content.Application.Features.Contents.Commands.TranslateContentVersion;

namespace NovaCore.Content.Application.Tests;

public sealed class TranslateContentVersionHandlerTests
{
    private static ContentEntity BuildContent(out Guid versionId)
    {
        var content = ContentEntity.Create(
            Guid.CreateVersion7(), ContentSlug.Create("first-article"), LanguageCode.Create("en"),
            "Title", "Summary", "{}", Guid.CreateVersion7());
        versionId = content.CurrentVersionId!.Value;
        return content;
    }

    private static (IContentWriteService WriteService, TranslateContentVersionHandler Handler) BuildHandler(ContentEntity? content)
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

        var handler = new TranslateContentVersionHandler(readService, writeService, uow);
        return (writeService, handler);
    }

    [Fact]
    public async Task Handle_NewLanguage_ReportsWasExistingLanguageFalse()
    {
        var content = BuildContent(out var versionId);
        var (_, handler) = BuildHandler(content);

        var response = await handler.Handle(new TranslateContentVersionCommand(
            content.Id, versionId, "vi", "Tieu De", "Tom Tat", "{}", Guid.CreateVersion7()));

        response.WasExistingLanguage.ShouldBeFalse();
        response.TargetLanguage.ShouldBe("vi");
    }

    [Fact]
    public async Task Handle_AlreadyTranslatedLanguage_ReportsWasExistingLanguageTrue()
    {
        var content = BuildContent(out var versionId);
        var (_, handler) = BuildHandler(content);

        var response = await handler.Handle(new TranslateContentVersionCommand(
            content.Id, versionId, "en", "Updated Title", "Updated Summary", "{}", Guid.CreateVersion7()));

        response.WasExistingLanguage.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_UnknownVersion_ThrowsNotFound()
    {
        var content = BuildContent(out _);
        var (writeService, handler) = BuildHandler(content);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new TranslateContentVersionCommand(
            content.Id, Guid.CreateVersion7(), "vi", "Title", "Summary", "{}", Guid.CreateVersion7())));

        await writeService.DidNotReceive().UpsertLocalizationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<LanguageCode>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ContentMetadata?>(), Arg.Any<CancellationToken>());
    }
}
