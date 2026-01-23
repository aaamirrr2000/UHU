using NG.MicroERP.Shared.Models;
using NG.MicroERP.WorkHub.Helper;

namespace NG.MicroERP.WorkHub.Pages;

public partial class MyLeavesPage : ContentPage
{
    private List<LeaveRequestsModel> _leaveRequests = new();

    public MyLeavesPage()
    {
        InitializeComponent();
        datePickerStart.Date = DateTime.Today.AddMonths(-3);
        datePickerEnd.Date = DateTime.Today.AddMonths(3);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadLeaveRequests();
    }

    private async void LoadLeaveRequests()
    {
        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;
            lblEmpty.IsVisible = false;

            int empId = WorkHubGlobals.User.Id;
            DateTime startDateValue = datePickerStart.Date ?? DateTime.Today.AddMonths(-3);
            DateTime endDateValue = datePickerEnd.Date ?? DateTime.Today.AddMonths(3);
            string startDate = startDateValue.ToString("yyyy-MM-dd");
            string endDate = endDateValue.ToString("yyyy-MM-dd");
            
            string criteria = $"EmpId={empId} AND StartDate >= '{startDate}' AND EndDate <= '{endDate}'";

            var leaves = await WorkHubFunctions.GetAsync<List<LeaveRequestsModel>>($"api/LeaveRequests/Search/{criteria}", true);

            if (leaves != null && leaves.Any())
            {
                _leaveRequests = leaves.OrderByDescending(l => l.AppliedDate).ToList();
                leavesCollectionView.ItemsSource = _leaveRequests;
                leavesCollectionView.IsVisible = true;
                lblEmpty.IsVisible = false;
            }
            else
            {
                leavesCollectionView.ItemsSource = null;
                leavesCollectionView.IsVisible = false;
                lblEmpty.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load leave requests: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }

    private void OnDateSelected(object? sender, DateChangedEventArgs e)
    {
        LoadLeaveRequests();
    }
}
