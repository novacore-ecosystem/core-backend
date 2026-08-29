using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Npgsql;

namespace NovaCore.BuildingBlock.Persistence.Ef.Provisioning;

public static class DatabaseProvisioner
{
    /// <summary>
    /// Checks whether the database named in <paramref name="connectionString"/> exists and creates
    /// it if not, connecting through the server's always-present 'postgres' maintenance database
    /// (CREATE DATABASE can't run against the database it's creating).
    /// </summary>
    public static async Task EnsureDatabaseExistsAsync(string connectionString, ILogger logger)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = targetBuilder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("The connection string does not specify a Database name.");
        }

        // CREATE DATABASE can't be parameterized, so the identifier is validated here and quoted
        // below to guard against injection from a malformed connection string.
        if (!Regex.IsMatch(databaseName, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
        {
            throw new InvalidOperationException($"Database name '{databaseName}' is not a safe identifier.");
        }

        var maintenanceBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres"
        };

        await using var connection = new NpgsqlConnection(maintenanceBuilder.ConnectionString);
        await connection.OpenAsync();

        await using (var existsCommand = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @name", connection))
        {
            existsCommand.Parameters.AddWithValue("name", databaseName);

            if (await existsCommand.ExecuteScalarAsync() is not null)
            {
                logger.LogInformation("Database '{Database}' already exists", databaseName);
                return;
            }
        }

        logger.LogInformation("Database '{Database}' not found, creating...", databaseName);

        await using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
        await createCommand.ExecuteNonQueryAsync();

        logger.LogInformation("Database '{Database}' created", databaseName);
    }
}
