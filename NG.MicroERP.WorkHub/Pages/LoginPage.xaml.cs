using NG.MicroERP.Shared.Models;
using NG.MicroERP.WorkHub.Helper;

namespace NG.MicroERP.WorkHub.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeServices();

        string savedUsername = Preferences.Get("Username", "");
        if (!string.IsNullOrWhiteSpace(savedUsername))
        {
            UsernameEntry.Text = savedUsername;
        }

        if (WorkHubGlobals.BaseURI == null || WorkHubGlobals.BaseURI.Trim() == string.Empty)
            lblURL.Text = WorkHubGlobals.BaseURI;
        else
            lblURL.Text = $"Server: {WorkHubGlobals.BaseURI}";
    }

    private void InitializeServices()
    {
        string savedUrl = Preferences.Get("BaseURI", "").Trim();
        if (!string.IsNullOrEmpty(savedUrl))
        {
            WorkHubGlobals.BaseURI = savedUrl;
        }
        else
        {
            WorkHubGlobals.BaseURI = "https://localhost:7019/api/";
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text!.Trim();
        string password = PasswordEntry.Text!.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Login", "Please enter both username and password.", "Okay");
            return;
        }

        await Login(username, password);
    }

    private async Task Login(string userName, string password)
    {
        InitializeServices();

        LoginSpinner.IsVisible = true;
        LoginSpinner.IsRunning = true;

        try
        {
            if (WorkHubGlobals.BaseURI == null || WorkHubGlobals.BaseURI.Trim() == string.Empty)
                lblURL.Text = WorkHubGlobals.BaseURI;

            var res = await WorkHubFunctions.GetAsync<UsersModel>($"api/Login/Login/{userName}/{password}", false);

            if (res != null && res.Id > 0)
            {
                Preferences.Set("Username", userName);
                Preferences.Set("Password", password);

                WorkHubGlobals.User = res;
                WorkHubGlobals.Token = res.Token ?? string.Empty;

                var org = await WorkHubFunctions.GetAsync<List<OrganizationsModel>>($"api/Organizations/Search", true) ?? new List<OrganizationsModel>();
                if (org != null && org.Any())
                {
                    WorkHubGlobals.Organization = org.FirstOrDefault()!;
                }

                if (WorkHubGlobals.Organization.Expiry <= DateTime.Now)
                {
                    await DisplayAlert("Login Failed", "License Expired.", "OK");
                }
                else
                {
                    // Navigate to Dashboard using Shell navigation
                    await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
                }
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid credentials.", "OK");
            }
        }
        catch (Exception ex)
        {
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
        InitializeServices();

        string savedUrl = Preferences.Get("BaseURI", "").Trim();

        string result = await DisplayPromptAsync(
            "Set Server URL",
            "Enter the API base URL:",
            initialValue: !string.IsNullOrEmpty(savedUrl) ? savedUrl : null,
            maxLength: 200,
            keyboard: Keyboard.Url,
            placeholder: "https://localhost:7019/api/"
        );

        if (!string.IsNullOrWhiteSpace(result))
        {
            Preferences.Set("BaseURI", result.Trim());
            WorkHubGlobals.BaseURI = result.Trim();
            lblURL.Text = $"Server: {WorkHubGlobals.BaseURI}";
            await DisplayAlert("URL", "API URL saved successfully.", "Okay");
        }
    }
}
