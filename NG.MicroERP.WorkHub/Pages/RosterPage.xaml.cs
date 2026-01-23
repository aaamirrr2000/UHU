using NG.MicroERP.Shared.Models;
using NG.MicroERP.WorkHub.Helper;

namespace NG.MicroERP.WorkHub.Pages;

public partial class RosterPage : ContentPage
{
    private List<RosterModel> _rosterList = new();

    public RosterPage()
    {
        InitializeComponent();
        datePicker.Date = DateTime.Today;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadRoster();
    }

    private async void LoadRoster()
    {
        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;
            lblEmpty.IsVisible = false;

            int empId = WorkHubGlobals.User.Id;
            DateTime selectedDate = datePicker.Date ?? DateTime.Today;
            string startDate = selectedDate.AddDays(-7).ToString("yyyy-MM-dd");
            string endDate = selectedDate.AddDays(7).ToString("yyyy-MM-dd");
            
            string criteria = $"r.EmployeeId={empId} AND r.RosterDate >= '{startDate}' AND r.RosterDate <= '{endDate}'";

            var roster = await WorkHubFunctions.GetAsync<List<RosterModel>>($"api/Roster/Search/{criteria}", true);

            if (roster != null && roster.Any())
            {
                _rosterList = roster.OrderBy(r => r.RosterDate).ToList();
                rosterCollectionView.ItemsSource = _rosterList;
                rosterCollectionView.IsVisible = true;
                lblEmpty.IsVisible = false;
            }
            else
            {
                rosterCollectionView.ItemsSource = null;
                rosterCollectionView.IsVisible = false;
                lblEmpty.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load roster: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }

    private void OnDateSelected(object? sender, DateChangedEventArgs e)
    {
        LoadRoster();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadRoster();
    }
}
