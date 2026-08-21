namespace NovaCore.Chat.Domain.Constants;

/// <summary>Recommended permission codes from the Chat Domain spec (section 35) - seed data for ConversationPermission, avoids magic strings.</summary>
public static class ConversationPermissionCodes
{
    public const string ViewConversation = "ViewConversation";
    public const string SendMessage = "SendMessage";
    public const string EditOwnMessage = "EditOwnMessage";
    public const string RecallOwnMessage = "RecallOwnMessage";
    public const string ReactMessage = "ReactMessage";
    public const string PinMessage = "PinMessage";
    public const string ManageNotes = "ManageNotes";
    public const string CreateTask = "CreateTask";
    public const string AssignTask = "AssignTask";
    public const string CreatePoll = "CreatePoll";
    public const string ManagePoll = "ManagePoll";
    public const string CreateSchedule = "CreateSchedule";
    public const string ManageParticipants = "ManageParticipants";
    public const string InviteParticipant = "InviteParticipant";
    public const string RemoveParticipant = "RemoveParticipant";
    public const string TransferConversation = "TransferConversation";
    public const string CloseConversation = "CloseConversation";
    public const string ReopenConversation = "ReopenConversation";
    public const string ManageRoles = "ManageRoles";
    public const string ManagePermissions = "ManagePermissions";
}
