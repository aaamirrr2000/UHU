using MicroERP.App.SwiftServe.Helper;
using Microsoft.Maui.Controls;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class UserTypeWarningPage : ContentPage
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public UserTypeWarningPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Get user information from static MyGlobals
        if (MyGlobals.User != null)
        {
            Username = MyGlobals.User.Username ?? "N/A";
            FullName = MyGlobals.User.FullName ?? "N/A";
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(FullName));
        }
    }

    private async void OnReturnToLoginClicked(object sender, EventArgs e)
    {
        // Clear user data
        MyGlobals.User = null;
        MyGlobals.Token = string.Empty;
        
        // Navigate back to login page by replacing the entire navigation stack
        var loginPage = new LoginPage();
        Application.Current!.MainPage = new NavigationPage(loginPage);
    }
}

