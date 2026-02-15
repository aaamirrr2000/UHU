using Microsoft.JSInterop;
using Newtonsoft.Json;
using MicroERP.Shared.Models;

namespace MicroERP.Shared.Helper;

/// <summary>Persists and restores auth/session in localStorage so Globals survive page refresh.</summary>
public static class SessionStorageHelper
{
    private const string AuthTokenKey = "authToken";
    private const string UserKey = "user";
    private const string OrganizationKey = "organization";
    // Use sessionStorage (survives refresh in same tab; often allowed when Tracking Prevention blocks localStorage)
    private const string StorageName = "sessionStorage";

    public static async Task ClearSessionAsync(IJSRuntime js)
    {
        try
        {
            await js.InvokeVoidAsync("eval", StorageName + ".removeItem('" + AuthTokenKey + "'); " + StorageName + ".removeItem('" + UserKey + "'); " + StorageName + ".removeItem('" + OrganizationKey + "');");
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
            var token = await js.InvokeAsync<string?>("eval", StorageName + ".getItem('" + AuthTokenKey + "')");
            if (string.IsNullOrWhiteSpace(token))
                return false;

            globals.Token = token;

            var userJson = await js.InvokeAsync<string?>("eval", StorageName + ".getItem('" + UserKey + "')");
            if (!string.IsNullOrWhiteSpace(userJson))
            {
                var user = DeserializeUser(userJson);
                if (user != null)
                    globals.User = user;
            }

            var orgJson = await js.InvokeAsync<string?>("eval", StorageName + ".getItem('" + OrganizationKey + "')");
            if (!string.IsNullOrWhiteSpace(orgJson))
            {
                var org = DeserializeOrganization(orgJson);
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

    private static UsersModel? DeserializeUser(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var user = JsonConvert.DeserializeObject<UsersModel>(json);
            if (user != null) return user;
            var inner = JsonConvert.DeserializeObject<string>(json);
            return string.IsNullOrWhiteSpace(inner) ? null : JsonConvert.DeserializeObject<UsersModel>(inner);
        }
        catch
        {
            return null;
        }
    }

    private static OrganizationsModel? DeserializeOrganization(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var org = JsonConvert.DeserializeObject<OrganizationsModel>(json);
            if (org != null) return org;
            var inner = JsonConvert.DeserializeObject<string>(json);
            return string.IsNullOrWhiteSpace(inner) ? null : JsonConvert.DeserializeObject<OrganizationsModel>(inner);
        }
        catch
        {
            return null;
        }
    }

    public static async Task SaveSessionAsync(IJSRuntime js, string token, UsersModel? user, OrganizationsModel? organization)
    {
        try
        {
            await js.InvokeVoidAsync("eval", StorageName + ".setItem('" + AuthTokenKey + "', " + JsonConvert.SerializeObject(token ?? "") + ")");
            if (user != null)
                await js.InvokeVoidAsync("eval", StorageName + ".setItem('" + UserKey + "', " + JsonConvert.SerializeObject(JsonConvert.SerializeObject(user)) + ")");
            if (organization != null)
                await js.InvokeVoidAsync("eval", StorageName + ".setItem('" + OrganizationKey + "', " + JsonConvert.SerializeObject(JsonConvert.SerializeObject(organization)) + ")");
        }
        catch
        {
            // Ignore if running in non-browser context
        }
    }
}

