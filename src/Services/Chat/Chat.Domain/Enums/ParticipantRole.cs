namespace NovaCore.Chat.Domain.Enums;

/// <summary>Named ParticipantRole (not ConversationParticipantRole) to avoid colliding with the ConversationParticipantRole mapping entity (Entities/Roles/ConversationParticipantRole.cs).</summary>
public enum ParticipantRole : byte
{
    Owner = 1,
    Admin = 2,
    Member = 3,
}
