namespace NovaCore.Chat.Domain.Enums;

/// <summary>short (not byte) - spec section 38 lists many future message kinds (structured cards, order/shipment, location, contact share, GIF/video, link preview, ...), so this enum is expected to grow well past byte's headroom.</summary>
public enum MessageType : short
{
    Text = 1,
    Image = 2,
    File = 3,
    Sticker = 4,
    System = 5,
    Poll = 6,
    Task = 7,
    Schedule = 8,
    Structured = 9,
}
