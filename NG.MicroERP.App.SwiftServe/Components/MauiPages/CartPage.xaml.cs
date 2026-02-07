using NG.MicroERP.App.SwiftServe.Helper;
using NG.MicroERP.App.SwiftServe.Services;
using NG.MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace NG.MicroERP.App.SwiftServe.Components.MauiPages;

public partial class CartPage : ContentPage
{
    private CartStateService _cartState;
    private ObservableCollection<ItemsModel> _cartItems;
    private InvoiceModel _party = new();

    public ObservableCollection<ItemsModel> CartItems
    {
        get => _cartItems;
        set
        {
            _cartItems = value;
            OnPropertyChanged();
        }
    }

    // BillNote is handled directly via BillNoteEditor.TextChanged event

    public CartPage()
    {
        InitializeComponent();
        BindingContext = this;
        
        // Get CartStateService
        _cartState = Application.Current?.Handler?.MauiContext?.Services?.GetService<CartStateService>() 
                     ?? new CartStateService();
        
        LoadCartData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateHeader();
        UpdateSummary();
        UpdateParcelDetailsVisibility();
    }

    private void LoadCartData()
    {
        CartItems = new ObservableCollection<ItemsModel>(_cartState?.CartItems ?? new ObservableCollection<ItemsModel>());
        if (BillNoteEditor != null)
        {
            BillNoteEditor.Text = _cartState?.BillNote ?? string.Empty;
        }
        
        UpdateEmptyState();
    }

    private void UpdateHeader()
    {
        if (_cartState?.SelectedTable != null)
        {
            HeaderSubtitle.Text = $"Table: {_cartState.SelectedTable.TableNumber} | Service: {_cartState.ServiceType}";
        }
        else
        {
            HeaderSubtitle.Text = $"Service: {_cartState?.ServiceType ?? "N/A"}";
        }
    }

    private void UpdateEmptyState()
    {
        bool isEmpty = !CartItems.Any();
        EmptyStateLayout.IsVisible = isEmpty;
        CartItemsCollectionView.IsVisible = !isEmpty;
    }

    private void UpdateSummary()
    {
        double totalAmount = CartItems?.Sum(i => i.RetailPrice * i.MaxQty) ?? 0;
        SubTotalLabel.Text = totalAmount.ToString("N2");

        // Calculate service charge
        double serviceAmount = 0;
        if (MyGlobals.ServiceCharge.ChargeType == "PERCENTAGE")
        {
            serviceAmount = (MyGlobals.ServiceCharge.Amount / 100) * totalAmount;
            ServiceChargeLabel.Text = $"Service Charges {MyGlobals.ServiceCharge.Amount:N2}%:";
            ServiceChargeRow.IsVisible = serviceAmount > 0;
        }
        else if (MyGlobals.ServiceCharge.ChargeType == "FLAT")
        {
            serviceAmount = MyGlobals.ServiceCharge.Amount;
            ServiceChargeLabel.Text = "Service Charges:";
            ServiceChargeRow.IsVisible = serviceAmount > 0;
        }
        else
        {
            ServiceChargeRow.IsVisible = false;
        }
        ServiceChargeAmountLabel.Text = serviceAmount.ToString("N2");

        // Calculate tax
        var taxableAmount = totalAmount + serviceAmount;
        var gstAmount = (MyGlobals.GST / 100) * taxableAmount;
        TaxLabel.Text = $"Tax {MyGlobals.GST:N2}%:";
        TaxAmountLabel.Text = gstAmount.ToString("N2");
        TaxRow.IsVisible = MyGlobals.GST > 0;

        // Grand total
        var grandTotal = taxableAmount + gstAmount;
        GrandTotalLabel.Text = grandTotal.ToString("N2");
    }

    private void UpdateParcelDetailsVisibility()
    {
        bool isParcel = MyGlobals._serviceType?.ToUpper() == "PARCEL";
        ParcelDetailsFrame.IsVisible = isParcel;
    }

    private void OnIncreaseQty(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ItemsModel item)
        {
            item.MaxQty++;
            UpdateSummary();
        }
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ItemsModel item)
        {
            if (item.MaxQty > 1)
            {
                item.MaxQty--;
                UpdateSummary();
            }
        }
    }

    private void OnRemoveItem(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ItemsModel item)
        {
            CartItems.Remove(item);
            _cartState?.CartItems?.Remove(item);
            UpdateSummary();
            UpdateEmptyState();
        }
    }

    private void OnInstructionsChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is ItemsModel item)
        {
            item.Description = e.NewTextValue;
        }
    }

    private void OnBillNoteChanged(object sender, TextChangedEventArgs e)
    {
        if (_cartState != null)
        {
            _cartState.BillNote = e.NewTextValue;
        }
    }

    private async void OnPlaceOrderClicked(object sender, EventArgs e)
    {
        if (!CartItems.Any())
        {
            await DisplayAlert("Error", "Cart is empty. Please add items first.", "OK");
            return;
        }

        try
        {
            var tranDate = DateTime.Now;
            var billDetails = CartItems.Select(item => new InvoiceItemReportModel
            {
                ItemId = item.Id,
                StockCondition = "NEW",
                Qty = item.MaxQty,
                UnitPrice = item.RetailPrice,
                TranDate = tranDate,
                Instructions = item.Description ?? "",
                Status = "PENDING",
                ServingSize = item.ServingSize,
                Person = item.Person
            }).ToList();

            var bill = new InvoiceModel
            {
                OrganizationId = MyGlobals.Organization.Id,
                InvoiceType = "BILL",
                Source = "POS",
                TableId = _cartState?.SelectedTable?.Id ?? 0,
                SalesId = MyGlobals.User.EmpId,
                PartyId = 1,
                PartyName = _party.PartyName ?? "",
                PartyPhone = _party.PartyPhone ?? "",
                PartyEmail = _party.PartyEmail ?? "",
                PartyAddress = _party.PartyAddress ?? "",
                TranDate = tranDate,
                ServiceType = _cartState?.ServiceType ?? "Dine-In",
                CreatedBy = MyGlobals.User.Id,
                CreatedOn = tranDate,
                CreatedFrom = Environment.MachineName,
                LocationId = MyGlobals.User.LocationId,
                Description = _cartState?.BillNote ?? "",
                BillAmount = Convert.ToDecimal(billDetails.Sum(d => d.Qty * Convert.ToDouble(d.UnitPrice)))
            };

            var request = new InvoicesModel
            {
                Invoice = bill,
                InvoiceDetails = new ObservableCollection<InvoiceItemReportModel>(billDetails)
            };

            var result = await MyFunctions.PostAsync<InvoicesModel>("Invoice/Insert", request, true);

            if (!result.Success)
            {
                await DisplayAlert("❌ Save Failed", "Record not saved.", "OK");
            }
            else
            {
                await DisplayAlert("✅ Save", "Record saved successfully.", "OK");
                _cartState?.Clear();
                CartItems.Clear();
                
                // Navigate to OrdersPage
                Navigation.PushAsync(new OrdersPage());
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error placing order: {ex.Message}");
            await DisplayAlert("Error", $"Failed to place order: {ex.Message}", "OK");
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        // Navigate back to OrderPage (if we came from there)
        if (Navigation.NavigationStack.Count > 1)
        {
            Navigation.PopAsync();
        }
        else
        {
            // If no navigation stack, go to TablesPage
            Application.Current!.MainPage = new NavigationPage(new TablesPage());
        }
    }
}
