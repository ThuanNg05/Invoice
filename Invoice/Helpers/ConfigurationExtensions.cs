using Invoice.Core.Helpers;
using Microsoft.Extensions.Configuration;

namespace Invoice.Helpers;

public static class ConfigurationExtensions
{
    public static string? GetDecrypted(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return value == null ? null : SecurityHelper.Decrypt(value);
    }

    public static string GetRequiredDecrypted(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Configuration value for key '{key}' is missing.");
        }
        return SecurityHelper.Decrypt(value);
    }
}
