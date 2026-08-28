using Microsoft.Extensions.Configuration;

using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace NovaCore.BuildingBlock.Web.Extensions;

/// <summary>
/// Bootstraps IConfiguration from HashiCorp Vault before the host is built, so every
/// other Add*(configuration) extension downstream (DB, JWT, Kafka, Redis, ...) reads
/// secrets transparently through the normal IConfiguration surface. Reads
/// VAULT_ADDR/VAULT_TOKEN/VAULT_PATHS the same way scripts/vault/fetch-and-run.sh does -
/// see docker-compose.override.yml's header comment for how those are wired per service.
/// </summary>
public static class VaultConfigurationExtensions
{
    private const string VaultAddrVariable = "VAULT_ADDR";
    private const string VaultTokenVariable = "VAULT_TOKEN";
    private const string VaultPathsVariable = "VAULT_PATHS";

    /// <summary>
    /// Fetches every path in VAULT_PATHS from Vault's KV-v2 engine, merges them (later
    /// path wins on key collisions - callers list shared infra paths first and their own
    /// service path last), and layers the result into configuration via
    /// AddInMemoryCollection. No-ops with a log line if VAULT_ADDR/TOKEN/PATHS aren't
    /// set, so `dotnet run` without Vault configured still starts against
    /// appsettings.json. A reachability/auth failure once Vault *is* configured throws -
    /// starting with partial secrets (missing JWT key, missing DB connection string)
    /// produces far more confusing errors later in the DI pipeline than failing here.
    /// </summary>
    public static async Task<IConfigurationBuilder> AddVaultSecretsAsync(this IConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        var vaultAddress = Environment.GetEnvironmentVariable(VaultAddrVariable);
        var vaultToken = Environment.GetEnvironmentVariable(VaultTokenVariable);
        var vaultPaths = Environment.GetEnvironmentVariable(VaultPathsVariable);

        if (string.IsNullOrWhiteSpace(vaultAddress) || string.IsNullOrWhiteSpace(vaultToken) || string.IsNullOrWhiteSpace(vaultPaths))
        {
            Console.WriteLine(
                $"[Vault] {VaultAddrVariable}/{VaultTokenVariable}/{VaultPathsVariable} not fully set - " +
                "skipping Vault bootstrap, falling back to appsettings.json/local environment.");
            return configurationBuilder;
        }

        try
        {
            var vaultClient = new VaultClient(new VaultClientSettings(vaultAddress, new TokenAuthMethodInfo(vaultToken)));

            var entries = vaultPaths
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseMountAndPath)
                .ToArray();

            var results = await Task.WhenAll(entries.Select(entry =>
                vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: entry.SecretPath, mountPoint: entry.MountPoint)));

            // Later paths win on key collisions - preserves the "shared infra first,
            // service-specific last" override order VAULT_PATHS entries are listed in.
            var merged = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var result in results)
            foreach (var (key, value) in result.Data.Data)
                merged[key] = value?.ToString();

            Console.WriteLine($"[Vault] Loaded {merged.Count} key(s) from {entries.Length} path(s) at {vaultAddress}.");
            configurationBuilder.AddInMemoryCollection(merged);
            return configurationBuilder;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Vault] Failed to load secrets from {vaultAddress}: {ex.Message}");
            throw new InvalidOperationException(
                $"Vault secret bootstrap failed (VAULT_ADDR={vaultAddress}, VAULT_PATHS={vaultPaths}). " +
                "See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Splits a "&lt;mount&gt;/&lt;secret-path&gt;" entry into its KV-v2 mount point and
    /// secret path, stripping a literal "data" segment if present - accepts both the
    /// short form ("kv/nova-core/dev/security") and the raw API-shaped form
    /// ("kv/data/nova-core/dev/security").
    /// </summary>
    private static (string MountPoint, string SecretPath) ParseMountAndPath(string entry)
    {
        var segments = entry.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            throw new FormatException($"Invalid VAULT_PATHS entry '{entry}' - expected '<mount>/<secret-path>'.");

        var mountPoint = segments[0];
        var pathSegments = segments[1] == "data" ? segments[2..] : segments[1..];

        if (pathSegments.Length == 0)
            throw new FormatException($"Invalid VAULT_PATHS entry '{entry}' - missing secret path after the mount.");

        return (mountPoint, string.Join('/', pathSegments));
    }
}
