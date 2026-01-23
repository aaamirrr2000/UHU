using System.Timers;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.WorkHub.Helper;

namespace NG.MicroERP.WorkHub.Pages;

public partial class AttendancePage : ContentPage
{
    private System.Timers.Timer? _timer;

    public AttendancePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateDateTime();
        LoadTodayAttendance();
        
        // Start timer to update time every second
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _timer?.Dispose();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateDateTime();
        });
    }

    private void UpdateDateTime()
    {
        lblCurrentDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        lblCurrentTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
    }

    private async void LoadTodayAttendance()
    {
        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            int empId = WorkHubGlobals.User.Id;
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            // Use date range comparison instead of CAST to avoid SQL injection helper blocking
            string criteria = $"EmpId={empId} AND QueryDate >= '{today}' AND QueryDate < '{tomorrow}'";

            var attendance = await WorkHubFunctions.GetAsync<List<DailyAttendanceModel>>($"api/DailyAttendance/Search/{Uri.EscapeDataString(criteria)}", true);

            if (attendance != null && attendance.Any())
            {
                var todayAttendance = attendance.FirstOrDefault();
                if (todayAttendance != null)
                {
                    lblClockInTime.Text = todayAttendance.InTime ?? "--:--";
                    lblClockOutTime.Text = todayAttendance.OutTime ?? "--:--";
                    lblStatus.Text = todayAttendance.Status ?? "Not Marked";
                    
                    btnClockIn.IsEnabled = string.IsNullOrEmpty(todayAttendance.InTime);
                    btnClockOut.IsEnabled = !string.IsNullOrEmpty(todayAttendance.InTime) && string.IsNullOrEmpty(todayAttendance.OutTime);
                }
            }
            else
            {
                lblClockInTime.Text = "--:--";
                lblClockOutTime.Text = "--:--";
                lblStatus.Text = "Not Marked";
                btnClockIn.IsEnabled = true;
                btnClockOut.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load attendance: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }

    private async void OnClockInClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Clock In", "Are you sure you want to clock in?", "Yes", "No");
        if (!confirm) return;

        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            // Note: Since there's no specific ClockIn endpoint, we'll use a workaround
            // You may need to create a specific endpoint for this in the API
            // For now, we'll just show a message that this feature needs API implementation
            await DisplayAlert("Info", "Clock In functionality requires API endpoint implementation. Please contact your administrator.", "OK");
            
            // After API endpoint is created, uncomment and use:
            // var result = await WorkHubFunctions.PostAsync<object>($"api/DailyAttendance/ClockIn/{WorkHubGlobals.User.Id}", null, true);
            // if (result.Success)
            // {
            //     await DisplayAlert("Success", "Clock in successful!", "OK");
            //     LoadTodayAttendance();
            // }
            // else
            // {
            //     await DisplayAlert("Error", result.Message, "OK");
            // }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to clock in: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }

    private async void OnClockOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Clock Out", "Are you sure you want to clock out?", "Yes", "No");
        if (!confirm) return;

        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            // Note: Since there's no specific ClockOut endpoint, we'll use a workaround
            // You may need to create a specific endpoint for this in the API
            // For now, we'll just show a message that this feature needs API implementation
            await DisplayAlert("Info", "Clock Out functionality requires API endpoint implementation. Please contact your administrator.", "OK");
            
            // After API endpoint is created, uncomment and use:
            // var result = await WorkHubFunctions.PostAsync<object>($"api/DailyAttendance/ClockOut/{WorkHubGlobals.User.Id}", null, true);
            // if (result.Success)
            // {
            //     await DisplayAlert("Success", "Clock out successful!", "OK");
            //     LoadTodayAttendance();
            // }
            // else
            // {
            //     await DisplayAlert("Error", result.Message, "OK");
            // }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to clock out: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }
}
