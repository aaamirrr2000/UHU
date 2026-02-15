using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages.Controls;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class OrderDetailsPage : ContentPage
{
    private string _action;
    private int _id;
    private InvoicesModel _invoices = new();
    private ObservableCollection<InvoiceDetailModel> _invoiceDetails = new();
    private System.Timers.Timer _refreshTimer;
    private const int RefreshIntervalMs = 3500;

    public OrderDetailsPage(string action, int id)
    {
        InitializeComponent();
        BindingContext = this;
        Title = action == "ORDER" ? "Order Summary" : "Table Summary";
        NavigationPage.SetTitleView(this, NavigationMenu.CreateTitleView(this, NavMenu));
        _action = action;
        _id = id;
        LoadOrderDetails();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MyGlobals.PageTitle = "Order View";
        StartRefreshTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopRefreshTimer();
    }

    private void StartRefreshTimer()
    {
        _refreshTimer?.Stop();
        _refreshTimer = new System.Timers.Timer(RefreshIntervalMs);
        _refreshTimer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(() => _ = LoadOrderDetails());
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();
    }

    private void StopRefreshTimer()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    public ObservableCollection<InvoiceDetailModel> InvoiceDetails
    {
        get => _invoiceDetails;
        private set
        {
            _invoiceDetails = value ?? new ObservableCollection<InvoiceDetailModel>();
            OnPropertyChanged(nameof(InvoiceDetails));
        }
    }

    private async Task LoadOrderDetails()
    {
        try
        {
            string billUrl = string.Empty;
            string billDetailUrl = string.Empty;

            if (_action == "ORDER")
            {
                billUrl = $"Invoice/Search/i.Id={_id} AND i.InvoiceType='SALE INVOICE'";
                billDetailUrl = $"InvoiceDetail/Search/InvoiceDetail.InvoiceId={_id}";
            }
            else if (_action == "TABLE")
            {
                billUrl = $"Invoice/Search/i.TableId={_id} AND i.InvoiceType='SALE INVOICE' AND i.Status!='COMPLETE'";
            }

            var res = await MyFunctions.GetAsync<List<InvoiceModel>>(billUrl, true);
            if (res != null && res.Count > 0)
            {
                _invoices.Invoice = res.First();

                if (_action == "ORDER")
                {
                    var res1 = await MyFunctions.GetAsync<List<InvoiceDetailModel>>(billDetailUrl, true);
                    if (res1 != null)
                        InvoiceDetails = new ObservableCollection<InvoiceDetailModel>(res1);
                }
                else if (_action == "TABLE")
                {
                    var invoiceIds = string.Join(",", res.Select(i => i.Id));
                    billDetailUrl = $"InvoiceDetail/Search/InvoiceDetail.InvoiceId IN ({invoiceIds})";
                    var res1 = await MyFunctions.GetAsync<List<InvoiceDetailModel>>(billDetailUrl, true);
                    if (res1 != null)
                        InvoiceDetails = new ObservableCollection<InvoiceDetailModel>(res1);
                }

                UpdateOrderHeader();
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading order details: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load order details: {ex.Message}", "OK");
        }
    }

    private void UpdateOrderHeader()
    {
        var inv = _invoices?.Invoice;
        if (inv == null) return;
        if (OrderCodeLabel != null)
            OrderCodeLabel.Text = string.IsNullOrEmpty(inv.Code) ? $"Order #{inv.Id}" : $"Order: {inv.Code}";
        if (OrderMetaLabel != null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(inv.TableName)) parts.Add($"Table: {inv.TableName}");
            if (inv.TranDate.HasValue) parts.Add(inv.TranDate.Value.ToString("g"));
            if (!string.IsNullOrEmpty(inv.PartyName)) parts.Add(inv.PartyName);
            OrderMetaLabel.Text = string.Join(" · ", parts);
            OrderMetaLabel.IsVisible = parts.Count > 0;
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private void OnFinalBillClicked(object sender, EventArgs e)
    {
        var id = _invoices?.Invoice?.Id ?? _id;
        if (id > 0)
            Navigation.PushAsync(new GenerateBillPage(id));
    }
}

