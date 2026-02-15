using MicroERP.API.Services.Services;
using MicroERP.Shared.Models;

namespace MicroERP.API.Helper;

public static class SystemConfigurationHelper
{
    private static SystemConfigurationService _service = new SystemConfigurationService();
    private static Dictionary<string, Dictionary<string, string>> _cache = new Dictionary<string, Dictionary<string, string>>();
    private static DateTime _lastCacheUpdate = DateTime.MinValue;
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5); // Cache for 5 minutes

    /// <summary>
    /// Gets a configuration value from the database by category and key
    /// </summary>
    public static async Task<string?> GetConfigValueAsync(string category, string configKey, int organizationId = 1)
    {
        try
        {
            // Check cache first
            if (ShouldRefreshCache())
            {
                await RefreshCacheAsync(organizationId);
            }

            string cacheKey = $"{organizationId}_{category}";
            if (_cache.ContainsKey(cacheKey) && _cache[cacheKey].ContainsKey(configKey))
            {
                return _cache[cacheKey][configKey];
            }

            // If not in cache, fetch from database
            var result = await _service.GetByCategory(category, organizationId);
            if (result.Item1 && result.Item2 != null)
            {
                var config = result.Item2.FirstOrDefault(c => c.ConfigKey?.Equals(configKey, StringComparison.OrdinalIgnoreCase) == true && c.IsActive == 1);
                return config?.ConfigValue;
            }

            return null;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error getting configuration value for {category}:{configKey}");
            return null;
        }
    }

    /// <summary>
    /// Gets all configuration values for a category
    /// </summary>
    public static async Task<Dictionary<string, string>> GetCategoryConfigAsync(string category, int organizationId = 1)
    {
        try
        {
            // Check cache first
            if (ShouldRefreshCache())
            {
                await RefreshCacheAsync(organizationId);
            }

            string cacheKey = $"{organizationId}_{category}";
            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }

            // If not in cache, fetch from database
            var result = await _service.GetByCategory(category, organizationId);
            if (result.Item1 && result.Item2 != null)
            {
                var configDict = result.Item2
                    .Where(c => c.IsActive == 1)
                    .ToDictionary(c => c.ConfigKey ?? "", c => c.ConfigValue ?? "");
                
                _cache[cacheKey] = configDict;
                return configDict;
            }

            return new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error getting category configuration for {category}");
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Clears the configuration cache (call this after updating configurations)
    /// </summary>
    public static void ClearCache()
    {
        _cache.Clear();
        _lastCacheUpdate = DateTime.MinValue;
    }

    private static bool ShouldRefreshCache()
    {
        return DateTime.Now - _lastCacheUpdate > CacheExpiry || _cache.Count == 0;
    }

    private static async Task RefreshCacheAsync(int organizationId)
    {
        try
        {
            var categories = new[] { "Backup", "Images", "Email", "FTP" };
            foreach (var category in categories)
            {
                var result = await _service.GetByCategory(category, organizationId);
                if (result.Item1 && result.Item2 != null)
                {
                    string cacheKey = $"{organizationId}_{category}";
                    _cache[cacheKey] = result.Item2
                        .Where(c => c.IsActive == 1)
                        .ToDictionary(c => c.ConfigKey ?? "", c => c.ConfigValue ?? "");
                }
            }
            _lastCacheUpdate = DateTime.Now;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error refreshing configuration cache");
        }
    }
}

