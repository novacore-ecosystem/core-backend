using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;
using NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Write;

public sealed class ConversationReasonSuggestionWriteService(
    IConversationReasonSuggestionRepository suggestionRepo,
    IUnitOfWork unitOfWork) : IConversationReasonSuggestionWriteService
{
    public async Task CreateAsync(ConversationReasonSuggestion suggestion, CancellationToken ct = default)
    {
        await suggestionRepo.AddAsync(suggestion, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task TranslateAsync(Guid id, LanguageCode languageCode, string text, CancellationToken ct = default)
    {
        await suggestionRepo.UpdateAsync(
            id,
            query => query.Include(s => s.Translations),
            s => s.Translate(languageCode, text),
            ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
