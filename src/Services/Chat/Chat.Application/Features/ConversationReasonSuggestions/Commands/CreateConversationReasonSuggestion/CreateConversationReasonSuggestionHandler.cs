using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;

namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.CreateConversationReasonSuggestion;

public sealed class CreateConversationReasonSuggestionHandler(
    IConversationReasonSuggestionReadService suggestionReadService,
    IConversationReasonSuggestionWriteService suggestionWriteService)
    : ICommandHandler<CreateConversationReasonSuggestionCommand, CreateConversationReasonSuggestionResponse>
{
    public async Task<CreateConversationReasonSuggestionResponse> Handle(
        CreateConversationReasonSuggestionCommand request,
        CancellationToken ct = default)
    {
        if (await suggestionReadService.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"A conversation reason suggestion with code '{request.Code}' already exists.");

        var suggestion = ConversationReasonSuggestion.Create(EntityCode.Create(request.Code), request.SortOrder);

        await suggestionWriteService.CreateAsync(suggestion, ct);

        return new CreateConversationReasonSuggestionResponse(suggestion.Id);
    }
}
