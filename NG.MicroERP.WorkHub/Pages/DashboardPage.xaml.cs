using NG.MicroERP.WorkHub.Helper;

namespace NG.MicroERP.WorkHub.Pages;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateWelcomeMessage();
    }

    private void UpdateWelcomeMessage()
    {
        if (WorkHubGlobals.User != null && !string.IsNullOrEmpty(WorkHubGlobals.User.Username))
        {
            lblWelcome.Text = $"Welcome, {WorkHubGlobals.User.Username}!";
        }
        else
        {
            lblWelcome.Text = "Welcome!";
        }
    }

    private async void OnAttendanceTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AttendancePage());
    }

    private async void OnRosterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RosterPage());
    }

    private async void OnLeaveRequestTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LeaveRequestPage());
    }

    private async void OnMyLeavesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyLeavesPage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (confirm)
        {
            WorkHubGlobals.User = new();
            WorkHubGlobals.Token = string.Empty;
            Preferences.Remove("Password");
            
            var loginPage = new LoginPage();
            await Navigation.PushAsync(loginPage);
            
            var navigationStack = Navigation.NavigationStack.ToList();
            foreach (var page in navigationStack)
            {
                if (page != loginPage)
                    Navigation.RemovePage(page);
            }
        }
    }
}
