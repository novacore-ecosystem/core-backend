namespace NovaCore.Chat.Domain.Enums;

/// <summary>Spec section 25 lists ReactionType as a MessageReaction property but never defines its values - filled in with a standard reaction set.</summary>
public enum ReactionType : byte
{
    Like = 1,
    Love = 2,
    Care = 3,
    Haha = 4,
    Wow = 5,
    Sad = 6,
    Angry = 7,
}
