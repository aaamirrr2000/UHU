

using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Services;

using Serilog;

namespace NG.MicroERP.App.SwiftServe.Components.MauiPage;

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
            await DisplayAlert("Login", $"Please enter both username and password.", "Okay");
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
            var res = await Functions.GetAsync<UsersModel>($"Login/Login/{userName}/{password}", false);
            if (res != null)
            {
                Log.Information($"User {userName} logged in.");

                Preferences.Set("Username", userName);
                Preferences.Set("Password", password);

                // Save user data
                Globals.User = res;
                Globals.Token = res.Token;

                //Get Service Charges
                ServiceChargeCalculationService _serviceChargeService = new();
                await _serviceChargeService.InitializeAsync();
                Globals.ServiceCharge!.ServiceChargeType = _serviceChargeService.ServiceChargeType;
                Globals.ServiceCharge.ServiceCharge = _serviceChargeService.ServiceCharge;

                //Get GST
                TaxCalculationService _gst = new();
                await _gst.InitializeAsync();
                Globals.GST = _gst.GST;


                var org = await Functions.GetAsync<List<OrganizationsModel>>($"Organizations/Search", true) ?? new List<OrganizationsModel>();
                if (org != null)
                    Globals.Organization = org.FirstOrDefault()!;

                if (Globals.Organization.Expiry <= DateTime.Now)
                {
                    await DisplayAlert("Login Failed", "License Expired.", "OK");

                }
                else
                {
                    //if (Globals.User.UserType.ToUpper() == "WAITER")
                    await Navigation.PushAsync(new MainPage());                    
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
            await DisplayAlert("URL", $"API URL saved successfully.", "Okay");
        }
    }


}