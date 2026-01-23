using Microsoft.JSInterop;
using System.Text.Json;

namespace NG.ControlCenter.WebSite.Services
{
    /// <summary>
    /// Service to persist component state to browser storage (localStorage/sessionStorage)
    /// Ensures form data survives page refresh
    /// </summary>
    public interface IStateService
    {
        Task SaveStateAsync<T>(string key, T value);
        Task<T?> LoadStateAsync<T>(string key);
        Task RemoveStateAsync(string key);
        Task ClearAllStateAsync();
    }

    public class StateService : IStateService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string StorageKey = "app_state_";

        public StateService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Save state to localStorage (persists across browser sessions)
        /// </summary>
        public async Task SaveStateAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey + key, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving state: {ex.Message}");
            }
        }

        /// <summary>
        /// Load state from localStorage
        /// </summary>
        public async Task<T?> LoadStateAsync<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey + key);
                return json == null ? default : JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading state: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Remove specific state from localStorage
        /// </summary>
        public async Task RemoveStateAsync(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey + key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing state: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear all saved state from localStorage
        /// </summary>
        public async Task ClearAllStateAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "");
                if (json != null)
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.clear");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing state: {ex.Message}");
            }
        }
    }
}
