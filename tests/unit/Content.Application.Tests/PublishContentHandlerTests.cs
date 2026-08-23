using NovaCore.Content.Application.Features.Contents.Commands.PublishContent;
using NovaCore.Content.Domain.ValueObjects;

namespace NovaCore.Content.Application.Tests;

public sealed class PublishContentHandlerTests
{
    private static ContentEntity BuildContent(out Guid versionId)
    {
        var content = ContentEntity.Create(
            Guid.CreateVersion7(), ContentSlug.Create("first-article"), LanguageCode.Create("en"),
            "Title", "Summary", "{}", Guid.CreateVersion7());
        versionId = content.CurrentVersionId!.Value;
        return content;
    }

    private static (IContentReadService ReadService, IContentWriteService WriteService, PublishContentHandler Handler) BuildHandler(ContentEntity? content)
    {
        var readService = Substitute.For<IContentReadService>();
        readService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(content);

        var writeService = Substitute.For<IContentWriteService>();
        writeService.PublishAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new PublishContentHandler(readService, writeService, uow, Substitute.For<IOutboxStore>());
        return (readService, writeService, handler);
    }

    [Fact]
    public async Task Handle_ValidVersion_CallsWriteServicePublishAsync()
    {
        var content = BuildContent(out var versionId);
        var (_, writeService, handler) = BuildHandler(content);

        var response = await handler.Handle(new PublishContentCommand(content.Id, versionId));

        response.VersionId.ShouldBe(versionId);
        await writeService.Received(1).PublishAsync(content.Id, versionId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownContent_ThrowsNotFound()
    {
        var (_, _, handler) = BuildHandler(null);

        await Should.ThrowAsync<NotFoundException>(
            () => handler.Handle(new PublishContentCommand(Guid.CreateVersion7(), Guid.CreateVersion7())));
    }

    [Fact]
    public async Task Handle_UnknownVersion_ThrowsNotFound()
    {
        var content = BuildContent(out _);
        var (_, writeService, handler) = BuildHandler(content);

        await Should.ThrowAsync<NotFoundException>(
            () => handler.Handle(new PublishContentCommand(content.Id, Guid.CreateVersion7())));

        await writeService.DidNotReceive().PublishAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }
}
