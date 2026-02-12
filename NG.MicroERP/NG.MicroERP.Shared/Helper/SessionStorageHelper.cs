using Microsoft.JSInterop;
using Newtonsoft.Json;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.Shared.Helper;

public static class SessionStorageHelper
{
    private const string AuthTokenKey = "authToken";
    private const string UserKey = "user";
    private const string OrganizationKey = "organization";

    public static async Task ClearSessionAsync(IJSRuntime js)
    {
        try
        {
            await js.InvokeVoidAsync("eval", "sessionStorage.removeItem('" + AuthTokenKey + "'); sessionStorage.removeItem('" + UserKey + "'); sessionStorage.removeItem('" + OrganizationKey + "');");
        }
        catch
        {
            // Ignore if running in non-browser context
        }
    }

    public static async Task<bool> RestoreSessionAsync(IJSRuntime js, Globals globals)
    {
        try
        {
            var token = await js.InvokeAsync<string?>("eval", "sessionStorage.getItem('" + AuthTokenKey + "')");
            if (string.IsNullOrWhiteSpace(token))
                return false;

            globals.Token = token;

            var userJson = await js.InvokeAsync<string?>("eval", "sessionStorage.getItem('" + UserKey + "')");
            if (!string.IsNullOrWhiteSpace(userJson))
            {
                var user = JsonConvert.DeserializeObject<UsersModel>(userJson);
                if (user != null)
                    globals.User = user;
            }

            var orgJson = await js.InvokeAsync<string?>("eval", "sessionStorage.getItem('" + OrganizationKey + "')");
            if (!string.IsNullOrWhiteSpace(orgJson))
            {
                var org = JsonConvert.DeserializeObject<OrganizationsModel>(orgJson);
                if (org != null)
                    globals.Organization = org;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task SaveSessionAsync(IJSRuntime js, string token, UsersModel? user, OrganizationsModel? organization)
    {
        try
        {
            await js.InvokeVoidAsync("eval", "sessionStorage.setItem('" + AuthTokenKey + "', " + JsonConvert.SerializeObject(token ?? "") + ")");
            if (user != null)
                await js.InvokeVoidAsync("eval", "sessionStorage.setItem('" + UserKey + "', " + JsonConvert.SerializeObject(JsonConvert.SerializeObject(user)) + ")");
            if (organization != null)
                await js.InvokeVoidAsync("eval", "sessionStorage.setItem('" + OrganizationKey + "', " + JsonConvert.SerializeObject(JsonConvert.SerializeObject(organization)) + ")");
        }
        catch
        {
            // Ignore if running in non-browser context
        }
    }
}
