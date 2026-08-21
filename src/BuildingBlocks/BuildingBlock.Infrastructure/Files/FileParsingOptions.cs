namespace NovaCore.BuildingBlock.Infrastructure.Files;

public sealed class FileParsingOptions
{
    public const string Section = "FileParsing";

    /// <summary>Safety ceiling - files larger than this are never opened for content inspection, regardless of any business AllowedSizeBytes rule. Default: 100 MB.</summary>
    public long MaxParseSizeBytes { get; set; } = 100 * 1024 * 1024;
}
