namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

public enum FileUploadValidationState
{
    /// <summary>Within the business AllowedSizeBytes limit (and, if content was inspected, parsed without issue).</summary>
    Valid = 1,

    /// <summary>Exceeds the business AllowedSizeBytes limit but is within MaxParseSizeBytes - still safe to inspect/use, the caller decides whether to accept it.</summary>
    Abnormal = 2,

    /// <summary>Exceeds MaxParseSizeBytes (never opened for content inspection), or content inspection found the file corrupt/unreadable.</summary>
    Invalid = 3,
}
