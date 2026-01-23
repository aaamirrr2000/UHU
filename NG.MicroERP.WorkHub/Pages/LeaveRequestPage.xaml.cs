using NG.MicroERP.Shared.Models;
using NG.MicroERP.WorkHub.Helper;

namespace NG.MicroERP.WorkHub.Pages;

public partial class LeaveRequestPage : ContentPage
{
    private List<LeaveTypesModel> _leaveTypes = new();

    public LeaveRequestPage()
    {
        InitializeComponent();
        datePickerStart.Date = DateTime.Today;
        datePickerEnd.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLeaveTypes();
    }

    private async Task LoadLeaveTypes()
    {
        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            var leaveTypes = await WorkHubFunctions.GetAsync<List<LeaveTypesModel>>($"api/LeaveTypes/Search", true);

            if (leaveTypes != null && leaveTypes.Any())
            {
                _leaveTypes = leaveTypes;
                pickerLeaveType.ItemsSource = leaveTypes.Select(lt => lt.LeaveName).ToList();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load leave types: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }

    private void OnLeaveTypeChanged(object? sender, EventArgs e)
    {
        // Handle leave type selection change if needed
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        // Get date values once
        DateTime startDate = datePickerStart.Date ?? DateTime.Today;
        DateTime endDate = datePickerEnd.Date ?? DateTime.Today;

        // Validation
        if (pickerLeaveType.SelectedIndex == -1)
        {
            await DisplayAlert("Validation", "Please select a leave type.", "OK");
            return;
        }

        if (endDate < startDate)
        {
            await DisplayAlert("Validation", "End date cannot be before start date.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(editorReason.Text))
        {
            await DisplayAlert("Validation", "Please enter a reason for leave.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Submit Leave Request", 
            $"Submit leave request from {startDate:dd MMM yyyy} to {endDate:dd MMM yyyy}?", 
            "Yes", "No");
        if (!confirm) return;

        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            var selectedLeaveType = _leaveTypes[pickerLeaveType.SelectedIndex];

            var leaveRequest = new LeaveRequestsModel
            {
                OrganizationId = WorkHubGlobals.User.OrganizationId,
                EmpId = WorkHubGlobals.User.Id,
                LeaveTypeId = selectedLeaveType.Id,
                StartDate = startDate,
                EndDate = endDate,
                Reason = editorReason.Text.Trim(),
                ContactNumber = entryContactNumber.Text?.Trim() ?? "",
                Status = "PENDING",
                AppliedDate = DateTime.Now,
                IsActive = 1,
                CreatedBy = WorkHubGlobals.User.Id,
                CreatedOn = DateTime.Now,
                CreatedFrom = "WorkHub Mobile App"
            };

            var result = await WorkHubFunctions.PostAsync<LeaveRequestsModel>("api/LeaveRequests/Insert", leaveRequest, true);

            if (result.Success)
            {
                await DisplayAlert("Success", "Leave request submitted successfully!", "OK");
                // Clear form
                pickerLeaveType.SelectedIndex = -1;
                datePickerStart.Date = DateTime.Today;
                datePickerEnd.Date = DateTime.Today;
                editorReason.Text = "";
                entryContactNumber.Text = "";
            }
            else
            {
                await DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to submit leave request: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }
}
