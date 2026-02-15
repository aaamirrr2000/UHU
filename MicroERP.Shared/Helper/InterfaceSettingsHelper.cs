using MicroERP.Shared.Models;

namespace MicroERP.Shared.Helper;

/// <summary>
/// Helper class to manage and cache Interface Settings
/// Provides easy access to interface settings with caching to avoid repeated API calls
/// </summary>
public class InterfaceSettingsHelper
{
    private static Dictionary<string, InterfaceSettingsModel>? _settingsCache;
    private static DateTime _cacheLastUpdated = DateTime.MinValue;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5); // Cache for 5 minutes
    private static readonly object _lockObject = new object();

    private readonly Functions _functions;
    private readonly Globals _globals;

    public InterfaceSettingsHelper(Functions functions, Globals globals)
    {
        _functions = functions;
        _globals = globals;
    }

    /// <summary>
    /// Get all settings for a specific category
    /// </summary>
    public async Task<List<InterfaceSettingsModel>> GetSettingsByCategoryAsync(string category)
    {
        await EnsureCacheLoadedAsync();
        
        lock (_lockObject)
        {
            return _settingsCache?
                .Values
                .Where(s => s.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true 
                         && s.IsActive == 1 
                         && s.IsSoftDeleted == 0)
                .ToList() ?? new List<InterfaceSettingsModel>();
        }
    }

    /// <summary>
    /// Get a specific setting value by category and key
    /// </summary>
    public async Task<string?> GetSettingValueAsync(string category, string settingKey)
    {
        await EnsureCacheLoadedAsync();
        
        lock (_lockObject)
        {
            var key = $"{category}|{settingKey}";
            var setting = _settingsCache?.GetValueOrDefault(key);
            
            if (setting != null && setting.IsActive == 1 && setting.IsSoftDeleted == 0)
            {
                return setting.SettingValue;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Get a boolean setting value (returns false if not found or invalid)
    /// </summary>
    public async Task<bool> GetBooleanSettingAsync(string category, string settingKey, bool defaultValue = false)
    {
        var value = await GetSettingValueAsync(category, settingKey);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        
        return value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get a numeric setting value (returns 0 if not found or invalid)
    /// </summary>
    public async Task<double> GetNumericSettingAsync(string category, string settingKey, double defaultValue = 0)
    {
        var value = await GetSettingValueAsync(category, settingKey);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        
        if (double.TryParse(value, out double result))
            return result;
        
        return defaultValue;
    }

    /// <summary>
    /// Get a string setting value (returns default if not found)
    /// </summary>
    public async Task<string> GetStringSettingAsync(string category, string settingKey, string defaultValue = "")
    {
        var value = await GetSettingValueAsync(category, settingKey);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    /// <summary>
    /// Clear the cache (useful when settings are updated)
    /// </summary>
    public static void ClearCache()
    {
        lock (_lockObject)
        {
            _settingsCache = null;
            _cacheLastUpdated = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Ensure cache is loaded and not expired
    /// </summary>
    private async Task EnsureCacheLoadedAsync()
    {
        lock (_lockObject)
        {
            // Check if cache is valid
            if (_settingsCache != null && 
                DateTime.Now - _cacheLastUpdated < CacheExpiration)
            {
                return; // Cache is still valid
            }
        }

        // Load settings from API
        try
        {
            var settings = await _functions.GetAsync<List<InterfaceSettingsModel>>(
                $"InterfaceSettings/Search/OrganizationId={_globals.User.OrganizationId}", true)
                ?? new List<InterfaceSettingsModel>();

            lock (_lockObject)
            {
                _settingsCache = new Dictionary<string, InterfaceSettingsModel>();
                
                foreach (var setting in settings.Where(s => s.IsActive == 1 && s.IsSoftDeleted == 0))
                {
                    var key = $"{setting.Category}|{setting.SettingKey}";
                    _settingsCache[key] = setting;
                }
                
                _cacheLastUpdated = DateTime.Now;
            }
        }
        catch
        {
            // If API call fails, use empty cache
            lock (_lockObject)
            {
                _settingsCache ??= new Dictionary<string, InterfaceSettingsModel>();
            }
        }
    }
}

