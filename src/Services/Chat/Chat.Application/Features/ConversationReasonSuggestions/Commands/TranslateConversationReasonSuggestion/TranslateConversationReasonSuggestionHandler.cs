using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;

namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.TranslateConversationReasonSuggestion;

public sealed class TranslateConversationReasonSuggestionHandler(
    IConversationReasonSuggestionReadService suggestionReadService,
    IConversationReasonSuggestionWriteService suggestionWriteService)
    : ICommandHandler<TranslateConversationReasonSuggestionCommand>
{
    public async Task Handle(TranslateConversationReasonSuggestionCommand request, CancellationToken ct = default)
    {
        _ = await suggestionReadService.GetByIdAsync(request.ConversationReasonSuggestionId, ct)
            ?? throw new NotFoundException(nameof(ConversationReasonSuggestion), request.ConversationReasonSuggestionId);

        await suggestionWriteService.TranslateAsync(
            request.ConversationReasonSuggestionId,
            LanguageCode.Create(request.LanguageCode),
            request.Text,
            ct);
    }
}
