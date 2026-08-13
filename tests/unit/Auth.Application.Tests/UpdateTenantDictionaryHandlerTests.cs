using System.Text.Json;

using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Contract.Events.Tenant;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.TestKit.Fakes;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class UpdateTenantDictionaryHandlerTests
{
    [Fact]
    public async Task Handle_MergesPayloadOntoExistingDictionary_PreservingUnspecifiedKeys_AndOtherLanguages()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        tenant.SetLocale(LanguageCode.Create("vi"), "{}", """{"welcome":"Xin chao","logout":"Tam biet"}""");
        tenant.SetLocale(LanguageCode.Create("en"), "{}", """{"welcome":"Hello"}""");

        var writeService = Substitute.For<ITenantWriteService>();
        writeService.UpdateWithLocalesAsync(tenant.Id, Arg.Any<Action<Tenant>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                ci.ArgAt<Action<Tenant>>(1)(tenant);
                return Task.CompletedTask;
            });

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new UpdateTenantDictionaryHandler(writeService, uow, Substitute.For<IOutboxStore>(), new FakeCurrentUserService());

        var payload = JsonDocument.Parse("""{"welcome":"Chao ban"}""").RootElement;
        await handler.Handle(new UpdateTenantDictionaryCommand(tenant.Id, "vi", payload));

        var viLocale = tenant.Locales.Single(l => l.LanguageCode?.Value == "vi");
        var merged = JsonDocument.Parse(viLocale.DictionaryJson).RootElement;
        merged.GetProperty("welcome").GetString().ShouldBe("Chao ban"); // updated
        merged.GetProperty("logout").GetString().ShouldBe("Tam biet");  // preserved

        // The other language's dictionary is a separate TenantLocale row - untouched.
        var enLocale = tenant.Locales.Single(l => l.LanguageCode?.Value == "en");
        JsonDocument.Parse(enLocale.DictionaryJson).RootElement.GetProperty("welcome").GetString().ShouldBe("Hello");
    }

    [Fact]
    public async Task Handle_RejectsNonObjectPayload()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        var writeService = Substitute.For<ITenantWriteService>();
        var uow = Substitute.For<IUnitOfWork>();
        var handler = new UpdateTenantDictionaryHandler(writeService, uow, Substitute.For<IOutboxStore>(), new FakeCurrentUserService());

        var payload = JsonDocument.Parse("[1,2,3]").RootElement;

        await Should.ThrowAsync<BadRequestException>(
            () => handler.Handle(new UpdateTenantDictionaryCommand(tenant.Id, "vi", payload)));
    }
}
