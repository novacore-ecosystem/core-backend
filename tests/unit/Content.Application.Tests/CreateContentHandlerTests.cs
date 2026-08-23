using NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;
using NovaCore.Content.Application.Features.Contents.Commands.CreateContent;
using NovaCore.Content.Domain.ValueObjects;

namespace NovaCore.Content.Application.Tests;

public sealed class CreateContentHandlerTests
{
    private static (IContentReadService ReadService, IContentWriteService WriteService, IContentTypeReadService ContentTypeReadService, CreateContentHandler Handler, List<ContentEntity> Saved)
        BuildHandler(bool contentTypeExists = true, bool slugExists = false)
    {
        var readService = Substitute.For<IContentReadService>();
        readService.ExistsBySlugAsync(Arg.Any<ContentSlug>(), Arg.Any<CancellationToken>()).Returns(slugExists);

        var contentTypeReadService = Substitute.For<IContentTypeReadService>();
        contentTypeReadService.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(contentTypeExists);

        var saved = new List<ContentEntity>();
        var writeService = Substitute.For<IContentWriteService>();
        writeService.CreateAsync(Arg.Do<ContentEntity>(saved.Add), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new CreateContentHandler(readService, writeService, contentTypeReadService, uow, Substitute.For<IOutboxStore>());
        return (readService, writeService, contentTypeReadService, handler, saved);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesContentWithRequestedLanguage()
    {
        var (_, _, _, handler, saved) = BuildHandler();

        var response = await handler.Handle(new CreateContentCommand(
            Guid.CreateVersion7(), "first-article", "vi", "Title", "Summary", "{}", Guid.CreateVersion7()));

        saved.ShouldHaveSingleItem();
        var content = saved.Single();
        content.Id.ShouldBe(response.ContentId);
        content.CurrentVersionId.ShouldBe(response.VersionId);
        content.Versions.Single().Localizations.Single().Culture.Value.ShouldBe("vi");
    }

    [Fact]
    public async Task Handle_BlankLanguage_FallsBackToServiceDefault()
    {
        var (_, _, _, handler, saved) = BuildHandler();

        await handler.Handle(new CreateContentCommand(
            Guid.CreateVersion7(), "first-article", "", "Title", "Summary", "{}", Guid.CreateVersion7()));

        saved.Single().Versions.Single().Localizations.Single().Culture.Value.ShouldBe("en");
    }

    [Fact]
    public async Task Handle_UnknownContentType_ThrowsNotFound()
    {
        var (_, writeService, _, handler, _) = BuildHandler(contentTypeExists: false);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new CreateContentCommand(
            Guid.CreateVersion7(), "first-article", "en", "Title", "Summary", "{}", Guid.CreateVersion7())));

        await writeService.DidNotReceive().CreateAsync(Arg.Any<ContentEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ThrowsConflict()
    {
        var (_, writeService, _, handler, _) = BuildHandler(slugExists: true);

        await Should.ThrowAsync<ConflictException>(() => handler.Handle(new CreateContentCommand(
            Guid.CreateVersion7(), "first-article", "en", "Title", "Summary", "{}", Guid.CreateVersion7())));

        await writeService.DidNotReceive().CreateAsync(Arg.Any<ContentEntity>(), Arg.Any<CancellationToken>());
    }
}
