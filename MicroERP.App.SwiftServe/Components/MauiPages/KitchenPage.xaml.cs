using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages.Controls;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class KitchenPage : ContentPage
{
    private ObservableCollection<KitchenOrderGroup> _orderGroups = new();
    private System.Timers.Timer _refreshTimer;
    private const int RefreshIntervalMs = 4000;

    public ObservableCollection<KitchenOrderGroup> OrderGroups
    {
        get => _orderGroups;
        set
        {
            _orderGroups = value ?? new ObservableCollection<KitchenOrderGroup>();
            OnPropertyChanged();
        }
    }

    public KitchenPage()
    {
        InitializeComponent();
        BindingContext = this;
        Title = "Kitchen";
        NavigationPage.SetTitleView(this, NavigationMenu.CreateTitleView(this, NavMenu));
        LoadOrders();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MyGlobals.PageTitle = "Kitchen";
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
        _refreshTimer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(LoadOrders);
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();
    }

    private void StopRefreshTimer()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    private async void LoadOrders()
    {
        try
        {
            if (LoadingOverlay != null) { LoadingOverlay.IsVisible = true; LoadingOverlay.Message = "Loading orders..."; }
            KitchenCollectionView.IsVisible = false;
            EmptyStateLayout.IsVisible = false;

            // All line items for open orders (not COMPLETE)
            string criteria = "Invoice.IsSoftDeleted = 0 AND ISNULL(Invoice.Status,'') <> 'COMPLETE' AND Invoice.InvoiceType = 'SALE INVOICE'";
            string encoded = Uri.EscapeDataString(criteria);
            List<InvoiceDetailModel>? details = null;
            try
            {
                details = await MyFunctions.GetAsync<List<InvoiceDetailModel>>($"InvoiceDetail/Search/{encoded}", true);
            }
            catch
            {
                // 404 or error when no open orders
            }
            if (details == null || details.Count == 0)
            {
                OrderGroups = new ObservableCollection<KitchenOrderGroup>();
                KitchenCollectionView.IsVisible = false;
                EmptyStateLayout.IsVisible = true;
                return;
            }

            var grouped = details
                .GroupBy(d => d.InvoiceId)
                .Select(g =>
                {
                    var first = g.First();
                    return new KitchenOrderGroup
                    {
                        InvoiceId = g.Key,
                        SeqNo = first.SeqNo ?? "",
                        TableName = first.TableName ?? "—",
                        TableLocation = first.TableLocation ?? "",
                        PartyName = first.PartyName ?? "",
                        Items = new ObservableCollection<InvoiceDetailModel>(g.OrderBy(x => x.Id).ToList())
                    };
                })
                .OrderBy(x => x.InvoiceId)
                .ToList();

            OrderGroups = new ObservableCollection<KitchenOrderGroup>(grouped);
            KitchenCollectionView.IsVisible = true;
            EmptyStateLayout.IsVisible = false;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Kitchen LoadOrders: {Message}", ex.Message);
            await DisplayAlert("Error", "Failed to load kitchen orders.", "OK");
            OrderGroups = new ObservableCollection<KitchenOrderGroup>();
            EmptyStateLayout.IsVisible = true;
        }
        finally
        {
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = false;
        }
    }

    public void OnMarkInProgress(object sender, EventArgs e)
    {
        if (sender is not Button b || b.BindingContext is not InvoiceDetailModel model) return;
        SetItemStatus(model.Id, "IN_PROGRESS");
    }

    public void OnMarkComplete(object sender, EventArgs e)
    {
        if (sender is not Button b || b.BindingContext is not InvoiceDetailModel model) return;
        SetItemStatus(model.Id, "COMPLETE");
    }

    private async void SetItemStatus(int invoiceDetailId, string status)
    {
        try
        {
            await MyFunctions.PostAsync<object>($"InvoiceDetail/InvoiceItemStatus/{invoiceDetailId}/{status}", null, true);
            var item = OrderGroups.SelectMany(g => g.Items).FirstOrDefault(i => i.Id == invoiceDetailId);
            if (item != null)
            {
                item.InvoiceDetailStatus = status;
                item.Status = status;
            }
            LoadOrders();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "SetItemStatus: {Message}", ex.Message);
            await DisplayAlert("Error", "Failed to update status.", "OK");
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
            Navigation.PopAsync();
    }

    private void OnRefreshClicked(object sender, EventArgs e) => LoadOrders();
}

public class KitchenOrderGroup : System.ComponentModel.INotifyPropertyChanged
{
    public int InvoiceId { get; set; }
    public string SeqNo { get; set; } = "";
    public string TableName { get; set; } = "";
    public string TableLocation { get; set; } = "";
    public string PartyName { get; set; } = "";
    public ObservableCollection<InvoiceDetailModel> Items { get; set; } = new();

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name ?? ""));
}
