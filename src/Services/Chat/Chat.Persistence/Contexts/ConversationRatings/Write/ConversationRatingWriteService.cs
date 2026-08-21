using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationRatings;
using NovaCore.Chat.Persistence.Contexts.ConversationRatings.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRatings.Write;

public sealed class ConversationRatingWriteService(
    IConversationRatingRepository ratingRepo,
    IUnitOfWork unitOfWork) : IConversationRatingWriteService
{
    public async Task CreateAsync(ConversationRating rating, CancellationToken ct = default)
    {
        await ratingRepo.AddAsync(rating, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
