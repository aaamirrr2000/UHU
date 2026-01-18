

using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Services;
using NG.MicroERP.App.SwiftServe.Helper;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

using Serilog;

namespace NG.MicroERP.App.SwiftServe.Components.MauiPages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        InitializeServices();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Try to initialize if not already done
        InitializeServices();

        string savedUsername = Preferences.Get("Username", "");
        string savedPassword = Preferences.Get("Password", "");

        if (!string.IsNullOrWhiteSpace(savedUsername) && !string.IsNullOrWhiteSpace(savedPassword))
        {
            //btnSettings.IsVisible = false;
            //await Login(UsernameEntry.Text, PasswordEntry.Text);
        }

        if (MyGlobals.BaseURI == null || MyGlobals.BaseURI.Trim() == string.Empty)
            lblURL.Text = MyGlobals.BaseURI;
    }

    private void InitializeServices()
    {
        string savedUrl = Preferences.Get("BaseURI", "").Trim();
        if (!string.IsNullOrEmpty(savedUrl))
        {
            MyGlobals.BaseURI = savedUrl;
        }
        else
        {
            MyGlobals.BaseURI = "https://localhost:7019/api/";
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text!.Trim();
        string password = PasswordEntry.Text!.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Login", $"Please enter both username and password.", "Okay");
            return;
        }

        await Login(username, password);
    }

    private async Task Login(string userName, string password)
    {
        // Ensure services are initialized
        InitializeServices();

        LoginSpinner.IsVisible = true;
        LoginSpinner.IsRunning = true;

        try
        {
            if (MyGlobals.BaseURI == null || MyGlobals.BaseURI.Trim() == string.Empty)
                lblURL.Text = MyGlobals.BaseURI;

            var res = await MyFunctions.GetAsync<UsersModel>($"api/Login/Login/{userName}/{password}", false);

            if (res != null)
            {
                Preferences.Set("Username", userName);
                Preferences.Set("Password", password);

                // Store in static MyGlobals (accessible from both MAUI and Blazor)
                MyGlobals.User = res;
                MyGlobals.Token = res.Token ?? string.Empty;
                
                // Log token storage for debugging
                Serilog.Log.Information($"Token stored in static MyGlobals - Length: {MyGlobals.Token?.Length ?? 0}, IsEmpty: {string.IsNullOrEmpty(MyGlobals.Token)}, Token preview: {(string.IsNullOrEmpty(MyGlobals.Token) ? "EMPTY" : MyGlobals.Token.Substring(0, Math.Min(20, MyGlobals.Token.Length)) + "...")}");

                var org = await MyFunctions.GetAsync<List<OrganizationsModel>>($"api/Organizations/Search", true) ?? new List<OrganizationsModel>();
                if (org != null && org.Any())
                {
                    MyGlobals.Organization = org.FirstOrDefault()!;
                }

                if (MyGlobals.Organization.Expiry <= DateTime.Now)
                {
                    await DisplayAlert("Login Failed", "License Expired.", "OK");

                }
                else
                {
                    if (MyGlobals.User.UserType!.ToUpper() == "WAITER")
                    {
                        // Hide current page to prevent flicker
                        this.IsVisible = false;
                        
                        // Create and navigate to MainPage
                        var mainPage = new MainPage();
                        await Navigation.PushAsync(mainPage);
                        
                        // Remove LoginPage from navigation stack to prevent back navigation
                        var navigationStack = Navigation.NavigationStack.ToList();
                        if (navigationStack.Count > 1)
                        {
                            Navigation.RemovePage(navigationStack[0]);
                        }
                    }
                }

            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid credentials.", "OK");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Login Exception");
            await DisplayAlert("Error", $"An error occurred during login. {ex.Message}", "Okay");
        }
        finally
        {
            LoginSpinner.IsRunning = false;
            LoginSpinner.IsVisible = false;
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        // Ensure services are initialized
        InitializeServices();

        string savedUrl = Preferences.Get("BaseURI", "").Trim();

        string result = await DisplayPromptAsync(
            "Set Server URL",
            "Enter the API base URL:",
            initialValue: !string.IsNullOrEmpty(savedUrl) ? savedUrl : null,
            maxLength: 200,
            keyboard: Keyboard.Url,
            placeholder: "http://10.0.2.2:5000/"
        );

        if (!string.IsNullOrWhiteSpace(result))
        {
            Preferences.Set("BaseURI", result.Trim());
            MyGlobals.BaseURI = result.Trim();
            await DisplayAlert("URL", $"API URL saved successfully.", "Okay");
        }
    }


}