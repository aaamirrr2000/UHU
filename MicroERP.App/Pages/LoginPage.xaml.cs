

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using Controls.UserDialogs.Maui;

using MudBlazor;

using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;

using Serilog;

namespace MicroERP.App.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        string savedUsername = Preferences.Get("Username", "");
        string savedPassword = Preferences.Get("Password", "");

        if (!string.IsNullOrWhiteSpace(savedUsername) && !string.IsNullOrWhiteSpace(savedPassword))
        {
            //btnSettings.IsVisible = false;
            //await Login(UsernameEntry.Text, PasswordEntry.Text);
        }

    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text!.Trim();
        string password = PasswordEntry.Text!.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await Toast.Make($"Please enter both username and password.", ToastDuration.Short).Show();
            return;
        }

        await Login(username, password);
    }

    private async Task Login(string userName, string password)
    {
        LoginSpinner.IsVisible = true;
        LoginSpinner.IsRunning = true;

        try
        {
            UserDialogs.Instance.ShowLoading("Logging in...");
            var res = await Functions.GetAsync<UsersModel>($"Login/Login/{userName}/{password}", false);
            UserDialogs.Instance.HideHud();
            if (res != null)
            {
                Log.Information($"User {userName} logged in.");

                Preferences.Set("Username", userName);
                Preferences.Set("Password", password);

                // Save user data
                Globals.User = res;
                Globals.Token = res.Token;

                var org = await Functions.GetAsync<List<OrganizationsModel>>($"Organizations/Search", true) ?? new List<OrganizationsModel>();
                if (org != null)
                    Globals.Organization = org.FirstOrDefault()!;

                if (Globals.Organization.Expiry <= DateTime.Now)
                {
                    await DisplayAlert("Login Failed", "License Expired.", "OK");
                   
                }
                else
                {
                    if (Globals.User.UserType.ToUpper() == "WAITER")
                        await Navigation.PushAsync(new MainMenuPage());
                    else if (Globals.User.UserType.ToUpper() == "KITCHEN")
                        await Navigation.PushAsync(new DineinOrdersPage());
                    //else if (Globals.User.UserType.ToUpper() == "CUSTOMER")
                    //    await Navigation.PushAsync(new ClientPage());
                }

            }
            else
            {
                Log.Information($"{userName} login failed.");
                await DisplayAlert("Login Failed", "Invalid credentials.", "OK");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Login Exception");
            await Toast.Make($"An error occurred during login. {ex.Message}", ToastDuration.Short).Show();
        }
        finally
        {
            LoginSpinner.IsRunning = false;
            LoginSpinner.IsVisible = false;
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        string savedUrl = Preferences.Get("BaseURI", "").Trim();

        string result = await DisplayPromptAsync(
            "Set Server URL",
            "Enter the API base URL:",
            initialValue: !string.IsNullOrEmpty(savedUrl) ? savedUrl : null,
            maxLength: 200,
            keyboard: Keyboard.Url,
            placeholder: "https://192.168.0.1:8080/"
        );

        if (!string.IsNullOrWhiteSpace(result))
        {
            Preferences.Set("BaseURI", result.Trim());
            Globals.BaseURI = result.Trim();
            await Toast.Make($"API URL saved successfully.", ToastDuration.Short).Show();
        }
    }
}
