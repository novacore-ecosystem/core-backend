using NovaCore.BuildingBlock.Persistence.Ef.Provisioning;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NovaCore.Auth.DbMigrator.Services;

/// <summary>
/// Provisions the isolated database backing Hangfire's background job storage if it doesn't already exist.
/// </summary>
public static class HangfireDbProvisioner
{
    public static async Task RunAsync(IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("[INFO] Checking Hangfire DB...");

        var hangfireConnectionString = configuration.GetConnectionString("Hangfire")
            ?? throw new InvalidOperationException("ConnectionStrings:Hangfire was not configured.");
        await DatabaseProvisioner.EnsureDatabaseExistsAsync(hangfireConnectionString, logger);

        logger.LogInformation("[SUCCESS] Hangfire DB ready");
    }
}
