using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace backend_api_dotnet9.Infrastructure;

public static class ConnectionStringHelper
{
    public static string NormalizePostgresConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        if (userInfo.Length != 2)
        {
            throw new InvalidOperationException("Invalid PostgreSQL URL format. Expected username and password.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = Uri.UnescapeDataString(userInfo[1]),
            SslMode = SslMode.Require
        };

        var query = QueryHelpers.ParseQuery(uri.Query);
        var sslMode = query.TryGetValue("sslmode", out var sslModeValues)
            ? sslModeValues.ToString()
            : null;
        if (!string.IsNullOrWhiteSpace(sslMode) &&
            Enum.TryParse<SslMode>(sslMode, true, out var parsedSslMode))
        {
            builder.SslMode = parsedSslMode;
        }

        return builder.ConnectionString;
    }
}
