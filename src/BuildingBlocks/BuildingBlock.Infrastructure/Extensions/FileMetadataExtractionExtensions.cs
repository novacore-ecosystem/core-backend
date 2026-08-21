using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Infrastructure.Files;
using NovaCore.BuildingBlock.Infrastructure.Files.MetadataExtraction;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Infrastructure.Extensions;

/// <summary>
/// File Metadata Extraction Extensions
/// </summary>
public static class FileMetadataExtractionExtensions
{
    /// <summary>Add file metadata extraction (IFileMetadataExtractor) from appsettings.json "FileParsing" section. Not wired into any service yet - opt in from a service's own AddInfrastructure() when a consuming feature needs it.</summary>
    public static IServiceCollection AddFileMetadataExtraction(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<FileParsingOptions>(configuration.GetSection(FileParsingOptions.Section));

        services.AddSingleton<ImageFileMetadataExtractor>();
        services.AddSingleton<MediaFileMetadataExtractor>();
        services.AddSingleton<DocumentFileMetadataExtractor>();
        services.AddSingleton<IFileMetadataExtractor, CompositeFileMetadataExtractor>();

        return services;
    }
}
