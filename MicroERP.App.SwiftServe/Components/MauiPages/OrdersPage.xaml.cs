using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages.Controls;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Storage;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class OrdersPage : ContentPage
{
    private ObservableCollection<InvoiceModel> _allInvoices = new();
    private ObservableCollection<InvoiceModel> _filteredInvoices = new();
    private DateTime _fromDate = DateTime.Today.AddDays(-6); // Default: last 7 days so order history shows recent orders
    private string _searchQuery = string.Empty;
    private string _serviceTypeFilter = null;
    private System.Timers.Timer _refreshTimer;

    public ObservableCollection<InvoiceModel> FilteredInvoices
    {
        get => _filteredInvoices;
        set
        {
            _filteredInvoices = value;
            OnPropertyChanged();
        }
    }

    public DateTime FromDate
    {
        get => _fromDate;
        set
        {
            _fromDate = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Parameterless constructor for navigation menu (e.g. Order History).</summary>
    public OrdersPage() : this(null) { }

    public OrdersPage(string serviceTypeFilter = null)
    {
        InitializeComponent();
        BindingContext = this;
        NavigationPage.SetTitleView(this, NavigationMenu.CreateTitleView(this, NavMenu));
        _serviceTypeFilter = serviceTypeFilter;
        
        if (!string.IsNullOrEmpty(serviceTypeFilter))
        {
            HeaderTitle.Text = "Online Orders";
            MyGlobals.PageTitle = "Online Orders";
        }
        
        LoadData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MyGlobals.PageTitle = "Orders";
        StartTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTimer();
    }

    private async void LoadData()
    {
        await AuthenticateIfNeeded();
        await LoadBills();
    }

    private async Task AuthenticateIfNeeded()
    {
        if (!string.IsNullOrEmpty(MyGlobals.Token))
        {
            Serilog.Log.Information("Token already available, skipping authentication");
            return;
        }

        try
        {
            string username = Preferences.Get("Username", "");
            string password = Preferences.Get("Password", "");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Serilog.Log.Warning("Username or password not found");
                return;
            }

            string encodedUsername = Uri.EscapeDataString(username);
            string encodedPassword = Uri.EscapeDataString(password);

            var user = await MyFunctions.GetAsync<UsersModel>($"Login/Login/{encodedUsername}/{encodedPassword}", false);

            if (user != null && !string.IsNullOrEmpty(user.Token))
            {
                MyGlobals.Token = user.Token;
                MyGlobals.User = user;
                
                if (string.IsNullOrEmpty(MyGlobals.BaseURI))
                {
                    string savedUrl = Preferences.Get("BaseURI", "").Trim();
                    MyGlobals.BaseURI = !string.IsNullOrEmpty(savedUrl) ? savedUrl : "https://localhost:7019/api/";
                }

                var org = await MyFunctions.GetAsync<List<OrganizationsModel>>("api/Organizations/Search", true) ?? new List<OrganizationsModel>();
                if (org.Any())
                {
                    MyGlobals.Organization = org.FirstOrDefault()!;
                }
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error during authentication: {ex.Message}");
        }
    }

    private async Task LoadBills()
    {
        try
        {
            if (LoadingOverlay != null) { LoadingOverlay.IsVisible = true; LoadingOverlay.Message = "Loading orders..."; }
            OrdersCollectionView.IsVisible = false;
            EmptyStateLayout.IsVisible = false;

            // Date range: FromDate through FromDate+6 (7 days), date-only comparison like web Invoice Summary
            var startDate = FromDate.Date.ToString("yyyy-MM-dd");
            var endDateExclusive = FromDate.Date.AddDays(7).ToString("yyyy-MM-dd");
            string criteria = $"i.InvoiceType = 'SALE INVOICE' AND CAST(ISNULL(i.TranDate, i.CreatedOn) AS DATE) >= CAST('{startDate}' AS DATE) AND CAST(ISNULL(i.TranDate, i.CreatedOn) AS DATE) < CAST('{endDateExclusive}' AS DATE)";
            string encodedCriteria = Uri.EscapeDataString(criteria);

            var res = await MyFunctions.GetAsync<List<InvoiceModel>>($"Invoice/Search/{encodedCriteria}", true);
            if (res != null)
            {
                _allInvoices = new ObservableCollection<InvoiceModel>(res);
                foreach (var item in _allInvoices)
                {
                    item.ElapsedTime = GetElapsedTime(item.CreatedOn);
                }
                
                // Update header if service type filter is set
                if (!string.IsNullOrEmpty(_serviceTypeFilter))
                {
                    HeaderTitle.Text = "Online Orders";
                }
                else
                {
                    HeaderTitle.Text = "Orders";
                }
            }
            else
            {
                _allInvoices = new ObservableCollection<InvoiceModel>();
            }

            ApplySearch();
            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading bills: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load orders: {ex.Message}", "OK");
        }
        finally
        {
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = false;
            OrdersCollectionView.IsVisible = _filteredInvoices.Any();
        }
    }

    private void ApplySearch()
    {
        string query = _searchQuery.ToLowerInvariant();
        var filtered = _allInvoices.Where(b =>
            (string.IsNullOrEmpty(_serviceTypeFilter) || 
             (b.ServiceType?.Equals(_serviceTypeFilter, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (string.IsNullOrEmpty(query) ||
            (b.Code?.ToLower().Contains(query) ?? false) ||
            (b.PartyName?.ToLower().Contains(query) ?? false) ||
            (b.Party?.ToLower().Contains(query) ?? false))
        ).ToList();
        
        FilteredInvoices = new ObservableCollection<InvoiceModel>(filtered);
    }

    private string GetElapsedTime(DateTime? created)
    {
        if (created == null) return "N/A";
        var span = DateTime.UtcNow - created.Value;
        return $"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
    }

    private void StartTimer()
    {
        _refreshTimer = new System.Timers.Timer(1000);
        _refreshTimer.Elapsed += RefreshElapsedTime;
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();
    }

    private void StopTimer()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
    }

    private void RefreshElapsedTime(object sender, System.Timers.ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var item in _allInvoices)
            {
                item.ElapsedTime = GetElapsedTime(item.CreatedOn);
            }
        });
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
            Navigation.PopAsync();
    }

    private void OnDateChanged(object sender, DateChangedEventArgs e)
    {
        FromDate = e.NewDate;
        LoadBills();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchQuery = e.NewTextValue ?? string.Empty;
        ApplySearch();
        UpdateEmptyState();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadBills();
    }

    private void UpdateEmptyState()
    {
        bool isEmpty = !_filteredInvoices.Any();
        EmptyStateLayout.IsVisible = isEmpty;
        OrdersCollectionView.IsVisible = !isEmpty;
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is InvoiceModel invoice)
        {
            bool confirm = await DisplayAlert("Delete", $"Delete Bill ID: {invoice.Code}?", "Yes", "No");
            if (confirm)
            {
                try
                {
                    invoice.UpdatedBy = MyGlobals.User.Id;
                    await MyFunctions.PostAsync<InvoiceModel>("Invoice/SoftDelete", invoice, true);
                    await LoadBills();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
        }
    }

    private void OnViewClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is InvoiceModel invoice)
        {
            Navigation.PushAsync(new OrderDetailsPage("ORDER", invoice.Id));
        }
    }

    private void OnGenerateBillClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is InvoiceModel invoice)
        {
            Navigation.PushAsync(new GenerateBillPage(invoice.Id));
        }
    }

    private void OnNotesClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is InvoiceModel invoice)
        {
            Navigation.PushAsync(new ClientFeedbackPage(invoice.Id));
        }
    }

    private async void OnReadyClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is InvoiceModel invoice)
        {
            bool confirm = await DisplayAlert("Status", $"Update Bill Status ID: {invoice.Code}?", "Yes", "No");
            if (!confirm) return;

            var res = await MyFunctions.PostAsync<InvoiceModel>($"Invoice/InvoiceStatus/{invoice.Id}/READY/0", null, true);
            if (res.Success)
            {
                await DisplayAlert("✅ Done", "The item status is now set to READY.", "Great!");
            }
            else
            {
                await DisplayAlert("❌ Status Update Failed", "Unable to mark the item as READY. Please try again.", "OK");
            }

            await LoadBills();
        }
    }
}

